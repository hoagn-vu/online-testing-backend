using backend_online_testing.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace backend_online_testing.Services
{
    public class FileManagementService
    {
        private readonly IMongoCollection<UsersModel> _users;
        private readonly IMongoCollection<SubjectsModel> _subjects;
        private readonly AddLogService _logService;

        public FileManagementService(IMongoDatabase database, AddLogService logService){
            _users = database.GetCollection<UsersModel>("Users");
            _subjects = database.GetCollection<SubjectsModel>("Subjects");
            _logService = logService;
        }

        public async Task<string> ProcessFileTxt(StreamReader reader, string subjectId, string userLogId)
        {
            QuestionBanksModel questionBanks;
            string questionBankName = "";
            string questionType = "";
            List<QuestionBanksModel> questionBankList = new List<QuestionBanksModel>();
            QuestionBanksModel currentQuestionBank = null;
            List<QuestionListModel> questionList = new List<QuestionListModel>();
            QuestionListModel currentQuestion = null;
            //OptionsModel optionChoice = null;

            string line;
            //foreach (var line in File.ReadLines(filePath)) {
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("@"))
                {
                    questionBankName = line.Trim('@', '{', '}', '-');
                    if (currentQuestionBank != null && questionList != null)
                    {
                        currentQuestionBank.List = questionList;
                        questionBankList.Add(currentQuestionBank);
                    }
                    currentQuestionBank = new QuestionBanksModel();
                    currentQuestionBank.QuestionBankName = questionBankName;
                    currentQuestionBank.QuestionBankStatus = "Active";
                }
                else if (line.StartsWith("$"))
                {
                    questionType = line.Trim('$', '{', '}', '-');
                }
                else if (line.StartsWith("#"))
                {
                    if (currentQuestion != null)
                    {
                        questionList.Add(currentQuestion);
                    }

                    var questionName = line.Trim('#');

                    currentQuestion = new QuestionListModel();
                    currentQuestion.QuestionText = questionName;
                    currentQuestion.QuestionId = ObjectId.GenerateNewId().ToString();
                    currentQuestion.QuestionStatus = "Active";
                    currentQuestion.QuestionType = questionType;
                    currentQuestion.Options = new List<OptionsModel>();
                }
                else
                {
                    if (currentQuestion == null || string.IsNullOrWhiteSpace(line))
                        continue;

                    var match = Regex.Match(line, @"^(A|B|C|D|E|F|G|H|I)[\.\)]\s*(.+)");
                    bool isCorrect = false;
                    string optionText = line.Trim();

                    if (match.Success)
                    {
                        string optionLabel = match.Groups[1].Value; // Get A, B, C, D
                        optionText = match.Groups[2].Value.Trim(); // Lấy phần nội dung còn lại

                        if (optionLabel == "A") //If a answer is true
                        {
                            isCorrect = true;
                        }
                    }

                    var optionChoice = new OptionsModel
                    {
                        OptionText = optionText,
                        IsCorrect = isCorrect
                    };

                    currentQuestion.Options.Add(optionChoice);
                }
            }

            if (currentQuestion != null)
            {
                questionList.Add(currentQuestion);
            }

            if (currentQuestionBank != null && questionList != null)
            {
                currentQuestionBank.List = questionList;
                questionBankList.Add(currentQuestionBank);
            }

            //return questionList;
            //return questionBankList;
            //Find subject by id
            var subjectFilter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
            var subject = await _subjects.Find(subjectFilter).FirstOrDefaultAsync();

            if (subject == null) {
                return "Not found subject";
            }
            //Insert to database
            var update = Builders<SubjectsModel>.Update.PushEach(s => s.QuestionBanks, questionBankList);
            await _subjects.UpdateOneAsync(subjectFilter, update);

            //Update user logg
            var logData = new UserLogsModel
            {
                LogAction = "Created",
                LogDetails = "Add list question using file docx",
                LogAt = DateTime.Now
            };

            await _logService.AddActionLog(userLogId, logData);

            return "Insert question bank successfully";
        }

        public async Task<string> ProcessFileDocx(Stream fileStream, string subjectId, string userLogId)
        {
            var text = new StringBuilder();

            using (var wordDoc = WordprocessingDocument.Open(fileStream, false))
            {
                foreach (var paragraph in wordDoc.MainDocumentPart.Document.Body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    text.AppendLine(paragraph.InnerText);
                }
            }

            // Tạo StreamReader từ nội dung docx
            using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(text.ToString()))))
            {
                return await ProcessFileTxt(reader, subjectId, userLogId);
            }
        }

        public async Task<List<object>> UsersFileExcel(Stream fileStream, string userLogId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var usersList = new List<UsersModel>();
            var usersResponse = new List<object>();

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    string userName = worksheet.Cells[row, 1].Text.Trim();

                    var existingUser = _users.Find(u => u.UserName == userName).FirstOrDefault();
                    if (existingUser != null)
                    {
                        usersResponse.Add(new
                        {
                            UserName = userName,
                            FullName = worksheet.Cells[row, 3].Text.Trim(),
                            Role = worksheet.Cells[row, 6].Text.Trim(),
                            Status = "Đã tồn tại"
                        });
                        continue;
                    }

                    var user = new UsersModel
                    {
                        Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                        UserName = userName,
                        AccountStatus = "Active",
                        UserCode = worksheet.Cells[row, 2].Text.Trim(),
                        FullName = worksheet.Cells[row, 3].Text.Trim(),
                        Gender = worksheet.Cells[row, 4].Text.Trim(),
                        DateOfBirth = worksheet.Cells[row, 5].Text.Trim(),
                        Role = worksheet.Cells[row, 6].Text.Trim(),
                        Password = worksheet.Cells[row, 2].Text.Trim(),
                    };

                    //Find user log data
                    var userLogInfo = await _users.Find(u => u.Id == userLogId).FirstOrDefaultAsync();

                    user.UserLog = new List<UserLogsModel>();

                    user.UserLog.Add(new UserLogsModel
                    {
                        LogId = ObjectId.GenerateNewId().ToString(),
                        LogAction = "Created",
                        LogDetails = $"User account is created by account {(userLogInfo?.UserName ?? "Unknown")} and name {(userLogInfo?.FullName ?? "Unknown")}",
                        LogAt = DateTime.UtcNow
                    });

                    //Add user to user list
                    usersList.Add(user);
                    usersResponse.Add(new
                    {
                        UserName = user.UserName,
                        FullName = user.FullName,
                        Role = user.Role,
                        Status = "Added successfully"
                    });
                }
            }
            if (usersList == null || !usersList.Any())
            {
                return usersResponse;
            }

            await _users.InsertManyAsync(usersList);

            //Log admin action
            var logData = new UserLogsModel {
                 LogAction = "Created",
                 LogDetails = "Add user using file excel",
                 LogAt = DateTime.Now
            };

            await _logService.AddActionLog(userLogId, logData);

            return usersResponse;
        }

        public async Task<List<object>> GroupUser(Stream fileStream, string userLogId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var usersList = new List<UsersModel>();
            var usersResponse = new List<object>();

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    string userName = worksheet.Cells[row, 1].Text.Trim();
                    string groupName = worksheet.Cells[row, 6].Text.Trim();

                    var existingUser = _users.Find(u => u.UserName == userName).FirstOrDefault();
                    //If user is not existing in group
                    if (existingUser != null)
                    {
                        existingUser.GroupName ??= new List<string>();

                        if (!existingUser.GroupName.Contains(groupName))
                        {
                            existingUser.GroupName.Add(groupName);

                            var filter = Builders<UsersModel>.Filter.Eq(u => u.Id, existingUser.Id);
                            var update = Builders<UsersModel>.Update.Set(u => u.GroupName, existingUser.GroupName);
                            await _users.UpdateOneAsync(filter, update);

                            usersResponse.Add(new
                            {
                                UserName = userName,
                                FullName = worksheet.Cells[row, 3].Text.Trim(),
                                Role = worksheet.Cells[row, 6].Text.Trim(),
                                Status = $"Update {userName} to group {groupName}"
                            });

                            //Find user log data
                            var userLogInfo = await _users.Find(u => u.Id == userLogId).FirstOrDefaultAsync();

                            var newLog = new UserLogsModel
                            {
                                LogId = ObjectId.GenerateNewId().ToString(),
                                LogAction = "Created",
                                LogDetails = $"User account is added to group {groupName} by account {(userLogInfo?.UserName ?? "Unknown")} and name {(userLogInfo?.FullName ?? "Unknown")}",
                                LogAt = DateTime.UtcNow
                            };

                            var filterLog = Builders<UsersModel>.Filter.Eq(u => u.Id, existingUser.Id);
                            var logUpdate = Builders<UsersModel>.Update.Push(u => u.UserLog, newLog);
                            await _users.UpdateOneAsync(filter, logUpdate);
                        }
                        //If user is existin in group
                        else
                        {
                            usersResponse.Add(new
                            {
                                UserName = userName,
                                FullName = worksheet.Cells[row, 3].Text.Trim(),
                                Role = worksheet.Cells[row, 6].Text.Trim(),
                                Status = $"{userName} is already in group {groupName}"
                            });
                        }
                        continue;
                    } else
                    {
                        usersResponse.Add(new
                        {
                            UserName = userName,
                            FullName = worksheet.Cells[row, 3].Text.Trim(),
                            Role = worksheet.Cells[row, 6].Text.Trim(),
                            Status = "Not found user"
                        });
                    }
                }
            }

            var logData = new UserLogsModel
            {
                LogAction = "Update",
                LogDetails = "Update group user using excel file"
            };

            await _logService.AddActionLog(userLogId, logData);

            return usersResponse;
        }
    }
}
