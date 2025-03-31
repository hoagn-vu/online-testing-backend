using DocumentFormat.OpenXml.Wordprocessing;

namespace Backend_online_testing.Services
{
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using Models;
    using DocumentFormat.OpenXml.Packaging;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using OfficeOpenXml;
    using LicenseContext = OfficeOpenXml.LicenseContext;

    public class FileManagementService : IFileManagementService
    {
        private readonly IMongoCollection<UsersModel> _users;
        private readonly IMongoCollection<SubjectsModel> _subjects;
        private readonly AddLogService _logService;
        // private IFileManagementService _fileManagementServiceImplementation;

        public FileManagementService(IMongoDatabase database, AddLogService logService)
        {
            this._users = database.GetCollection<UsersModel>("users");
            this._subjects = database.GetCollection<SubjectsModel>("subjects");
            this._logService = logService;
        }

        public async Task<string> ProcessFileTxt(StreamReader reader, string subjectId, string questionBankId)
        {
            var subjectFilter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
            var subject = await this._subjects.Find(subjectFilter).FirstOrDefaultAsync();
            if (subject == null)
            {
                return "Không tìm thấy phân môn";
            }

            var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
            if (questionBank == null)
            {
                return "Không tìm thấy bộ câu hỏi";
            }

            var questionList = new List<QuestionModel>();
            QuestionModel? currentQuestion = null;
            var isFirstOption = true;
            string? line;
            var lastTag1 = string.Empty;
            var lastTag2 = string.Empty;
            var optionOrder = new Dictionary<string, int> { {"A", 0}, {"B", 1}, {"C", 2}, {"D", 3}, {"E", 4}, {"F", 5}, {"G", 6}, {"H", 7}, {"I", 8} };
            var hasAnyQuestion = false;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (line.StartsWith("{@") && line.EndsWith("-}"))
                {
                    lastTag1 = line.Trim('{', '}', '@', '-');
                }
                else if (line.StartsWith("{$") && line.EndsWith("-}"))
                {
                    lastTag2 = line.Trim('{', '}', '$', '-');
                }
                else if (line.StartsWith("#"))
                {
                    hasAnyQuestion = true;
                    if (currentQuestion != null)
                    {
                        //Kiểm tra số lượng options
                        if (currentQuestion.Options.Count < 2 || currentQuestion.Options.Count > 10)
                        {
                            return "Số lượng lựa chọn phải từ 2 đến 10.";
                        }
                        questionList.Add(currentQuestion);
                    }

                    currentQuestion = new QuestionModel
                    {
                        QuestionText = line.Trim('#'),
                        QuestionId = ObjectId.GenerateNewId().ToString(),
                        QuestionStatus = "available",
                        QuestionType = "single-choice",
                        Options = [],
                        //Options = [],
                        Tags = []
                    };
                    
                    if (lastTag1 != null)
                    {
                        currentQuestion.Tags.Add(lastTag1);
                    }
                    if (lastTag2 != null)
                    {
                        currentQuestion.Tags.Add(lastTag2);
                    }
                    
                    isFirstOption = true;
                }
                else
                {
                    if (currentQuestion == null || string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var match = Regex.Match(line, @"^(A|B|C|D|E|F|G|H|I)[\.\)]\s*(.+)");
                    if (match.Success)
                    {
                        // string optionText = match.Groups[2].Value.Trim();
                        // bool isCorrect = isFirstOption;
                        // isFirstOption = false;
                        //
                        // var optionChoice = new OptionsModel
                        // {
                        //     OptionText = optionText,
                        //     IsCorrect = isCorrect,
                        // };
                        // currentQuestion.Options.Add(optionChoice);
                        var optionLabel = match.Groups[1].Value;
                        var optionText = match.Groups[2].Value.Trim();
                        var isCorrect = isFirstOption;
                        isFirstOption = false;

                        var optionChoice = new OptionsModel
                        {
                            OptionText = optionText,
                            IsCorrect = isCorrect,
                        };
                
                        var insertIndex = optionOrder.ContainsKey(optionLabel) ? optionOrder[optionLabel] : currentQuestion.Options.Count;
                        if (insertIndex >= currentQuestion.Options.Count)
                        {
                            currentQuestion.Options.Add(optionChoice);
                        }
                        else
                        {
                            currentQuestion.Options.Insert(insertIndex, optionChoice);
                        }
                    }
                }
            }
            // Kiểm tra nếu không có câu hỏi nào
            if (!hasAnyQuestion)
            {
                return "File không chứa câu hỏi nào hợp lệ";
            }
            if (currentQuestion != null)
            {
                questionList.Add(currentQuestion);
                //questionList.Add(currentQuestion);
            }

            var updateFilter = Builders<SubjectsModel>.Filter.And(
                Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
                Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb => qb.QuestionBankId == questionBankId)
            );

            var update = Builders<SubjectsModel>.Update.Set("QuestionBanks.$.QuestionList", questionList);
            await this._subjects.UpdateOneAsync(updateFilter, update);

            return "Tải tệp câu hỏi thành công";
            throw new NotImplementedException();
        }
        
        // public async Task<string> ProcessFileTxt(StreamReader reader, string subjectId, string userLogId)
        // {
        //     var questionBankName = string.Empty;
        //     var questionType = string.Empty;
        //     var questionBankList = new List<QuestionBanksModel>();
        //     var currentQuestionBank = new QuestionBanksModel();
        //     var questionList = new List<QuestionModel>();
        //     var currentQuestion = new QuestionModel();
        //
        //     string? line;
        //
        //     while ((line = await reader.ReadLineAsync()) != null)
        //     {
        //         if (line.StartsWith("@"))
        //         {
        //             questionBankName = line.Trim('@', '{', '}', '-');
        //             if (currentQuestionBank != null)
        //             {
        //                 currentQuestionBank.QuestionList = questionList;
        //                 questionBankList.Add(currentQuestionBank);
        //             }
        //
        //             currentQuestionBank = new QuestionBanksModel();
        //             currentQuestionBank.QuestionBankName = questionBankName;
        //             currentQuestionBank.QuestionBankStatus = "Active";
        //         }
        //         else if (line.StartsWith("$"))
        //         {
        //             questionType = line.Trim('$', '{', '}', '-');
        //         }
        //         else if (line.StartsWith("#"))
        //         {
        //             if (currentQuestion != null)
        //             {
        //                 questionList.Add(currentQuestion);
        //             }
        //
        //             var questionName = line.Trim('#');
        //
        //             currentQuestion = new QuestionModel();
        //             currentQuestion.QuestionText = questionName;
        //             currentQuestion.QuestionId = ObjectId.GenerateNewId().ToString();
        //             currentQuestion.QuestionStatus = "Active";
        //             currentQuestion.QuestionType = questionType;
        //             currentQuestion.Options = new List<OptionsModel>();
        //         }
        //         else
        //         {
        //             if (currentQuestion == null || string.IsNullOrWhiteSpace(line))
        //             {
        //                 continue;
        //             }
        //
        //             var match = Regex.Match(line, @"^(A|B|C|D|E|F|G|H|I)[\.\)]\s*(.+)");
        //             bool isCorrect = false;
        //             string optionText = line.Trim();
        //
        //             if (match.Success)
        //             {
        //                 string optionLabel = match.Groups[1].Value; // Get A, B, C, D
        //                 optionText = match.Groups[2].Value.Trim(); // Lấy phần nội dung còn lại
        //
        //                 if (optionLabel == "A") // If an answer is true
        //                 {
        //                     isCorrect = true;
        //                 }
        //             }
        //
        //             var optionChoice = new OptionsModel
        //             {
        //                 OptionText = optionText,
        //                 IsCorrect = isCorrect,
        //             };
        //
        //             currentQuestion.Options.Add(optionChoice);
        //         }
        //     }
        //
        //     if (currentQuestion != null)
        //     {
        //         questionList.Add(currentQuestion);
        //     }
        //
        //     if (currentQuestionBank != null && questionList != null)
        //     {
        //         currentQuestionBank.QuestionList = questionList;
        //         questionBankList.Add(currentQuestionBank);
        //     }
        //
        //     // Return questionList;
        //     // Return questionBankList;
        //     // Find subject by id
        //     var subjectFilter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
        //     var subject = await this._subjects.Find(subjectFilter).FirstOrDefaultAsync();
        //
        //     if (subject == null)
        //     {
        //         return "Not found subject";
        //     }
        //
        //     // Insert to database
        //     var update = Builders<SubjectsModel>.Update.PushEach(s => s.QuestionBanks, questionBankList);
        //     await this._subjects.UpdateOneAsync(subjectFilter, update);
        //
        //     // Update user logg
        //     var logData = new UserLogsModel
        //     {
        //         LogAction = "Created",
        //         LogDetails = "Add list question using file docx",
        //         LogAt = DateTime.Now,
        //     };
        //
        //     await this._logService.AddActionLog(userLogId, logData);
        //
        //     return "Insert question bank successfully";
        // }

        public async Task<string> ProcessFileDocx(Stream fileStream, string subjectId, string questionBankId)
        {
            // Kiểm tra stream ngay từ đầu
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream), "File stream cannot be null");
            }

            if (fileStream.Length == 0)
            {
                throw new ArgumentException("File stream cannot be empty", nameof(fileStream));
            }

            var text = new StringBuilder();

            using (var wordDoc = WordprocessingDocument.Open(fileStream, false))
            {
                if (wordDoc.MainDocumentPart?.Document.Body == null)
                {
                    return "File DOCX không có nội dung"; // hoặc throw exception
                }

                foreach (var paragraph in wordDoc.MainDocumentPart.Document.Body.Elements<Paragraph>())
                {
                    text.AppendLine(paragraph.InnerText);
                }
            }

            // Tạo StreamReader từ nội dung docx
            using (var reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(text.ToString()))))
            {

                return await this.ProcessFileTxt(reader, subjectId, questionBankId);
            }
            throw new NotImplementedException();
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

                    var existingUser = this._users.Find(u => u.UserName == userName).FirstOrDefault();
                    if (existingUser != null)
                    {
                        usersResponse.Add(new
                        {
                            UserName = userName,
                            FullName = worksheet.Cells[row, 3].Text.Trim(),
                            Role = worksheet.Cells[row, 6].Text.Trim(),
                            Status = "Đã tồn tại",
                        });
                        continue;
                    }

                    var user = new UsersModel
                    {
                        UserName = userName,
                        AccountStatus = "active",
                        UserCode = worksheet.Cells[row, 2].Text.Trim(),
                        FullName = worksheet.Cells[row, 3].Text.Trim(),
                        Gender = worksheet.Cells[row, 4].Text.Trim(),
                        DateOfBirth = worksheet.Cells[row, 5].Text.Trim(),
                        Role = worksheet.Cells[row, 6].Text.Trim(),
                        Password = worksheet.Cells[row, 2].Text.Trim(),
                    };

                    // Find user log data
                    var userLogInfo = await this._users.Find(u => u.Id == userLogId).FirstOrDefaultAsync();

                    user.UserLog = new List<UserLogsModel>();

                    user.UserLog.Add(new UserLogsModel
                    {
                        LogId = ObjectId.GenerateNewId().ToString(),
                        LogAction = "Created",
                        LogDetails = $"User account is created by account {userLogInfo?.UserName ?? "Unknown"} and name {userLogInfo?.FullName ?? "Unknown"}",
                        LogAt = DateTime.UtcNow,
                    });

                    // Add user to user list
                    usersList.Add(user);
                    usersResponse.Add(new
                    {
                        UserName = user.UserName,
                        FullName = user.FullName,
                        Role = user.Role,
                        Status = "Added successfully",
                    });
                }
            }

            if (usersList.Count == 0)
            {
                return usersResponse;
            }

            await this._users.InsertManyAsync(usersList);

            // Log admin action
            var logData = new UserLogsModel
            {
                 LogAction = "Created",
                 LogDetails = "Add user using file excel",
                 LogAt = DateTime.Now,
            };

            await this._logService.AddActionLog(userLogId, logData);

            return usersResponse;
        }

        public async Task<List<object>> GroupUser(Stream fileStream, string userLogId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // var usersList = new List<UsersModel>();
            var usersResponse = new List<object>();

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    string userName = worksheet.Cells[row, 1].Text.Trim();
                    string groupName = worksheet.Cells[row, 6].Text.Trim();
                    var existingUser = this._users.Find(u => u.UserName == userName).FirstOrDefault();

                    // If user is not existing in group
                    if (existingUser != null)
                    {
                        if (!existingUser.GroupName.Contains(groupName))
                        {
                            existingUser.GroupName.Add(groupName);

                            var filter = Builders<UsersModel>.Filter.Eq(u => u.Id, existingUser.Id);
                            var update = Builders<UsersModel>.Update.Set(u => u.GroupName, existingUser.GroupName);
                            await this._users.UpdateOneAsync(filter, update);

                            usersResponse.Add(new
                            {
                                UserName = userName,
                                FullName = worksheet.Cells[row, 3].Text.Trim(),
                                Role = worksheet.Cells[row, 6].Text.Trim(),
                                Status = $"Update {userName} to group {groupName}",
                            });

                            // Find user log data
                            var userLogInfo = await this._users.Find(u => u.Id == userLogId).FirstOrDefaultAsync();

                            var newLog = new UserLogsModel
                            {
                                LogId = ObjectId.GenerateNewId().ToString(),
                                LogAction = "Created",
                                LogDetails = $"User account is added to group {groupName} by account {userLogInfo?.UserName ?? "Unknown"} and name {userLogInfo?.FullName ?? "Unknown"}",
                                LogAt = DateTime.UtcNow,
                            };

                            var filterLog = Builders<UsersModel>.Filter.Eq(u => u.Id, existingUser.Id);
                            var logUpdate = Builders<UsersModel>.Update.Push(u => u.UserLog, newLog);
                            await this._users.UpdateOneAsync(filter, logUpdate);
                        }
                        else
                        {
                            usersResponse.Add(new
                            {
                                UserName = userName,
                                FullName = worksheet.Cells[row, 3].Text.Trim(),
                                Role = worksheet.Cells[row, 6].Text.Trim(),
                                Status = $"{userName} is already in group {groupName}",
                            });
                        }

                        continue;
                    }
                    else
                    {
                        usersResponse.Add(new
                        {
                            UserName = userName,
                            FullName = worksheet.Cells[row, 3].Text.Trim(),
                            Role = worksheet.Cells[row, 6].Text.Trim(),
                            Status = "Not found user",
                        });
                    }
                }
            }

            var logData = new UserLogsModel
            {
                LogAction = "Update",
                LogDetails = "Update group user using excel file",
            };

            await _logService.AddActionLog(userLogId, logData);

            return usersResponse;
        }
    }
}
