using Microsoft.AspNetCore.Mvc;

#pragma warning disable SA1309
namespace Backend_online_testing.Services;

using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using DocumentFormat.OpenXml.Office.SpreadSheetML.Y2023.MsForms;
using MongoDB.Bson;
using MongoDB.Driver;

public class SubjectsService
{
    private readonly SubjectRepository _subjectRepository;

    public SubjectsService(SubjectRepository subjectRepository)
    {
        private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
        private readonly IMongoCollection<UsersModel> _usersCollection;
        private readonly IMongoCollection<LogsModel> _logsCollection;

        public SubjectsService(IMongoDatabase database)
        {
            _subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
            _usersCollection = database.GetCollection<UsersModel>("users");
            _logsCollection = database.GetCollection<LogsModel>("logs");
        }

        // Find all
        public async Task<(List<SubjectDto>, long)> GetSubjects(string? keyword, int page, int pageSize)
        {
            var filter = Builders<SubjectsModel>.Filter.Ne(sub => sub.SubjectStatus, "deleted");
            if (!string.IsNullOrEmpty(keyword))
            {
                filter = Builders<SubjectsModel>.Filter.Regex(sub => sub.SubjectName, new BsonRegularExpression(keyword, "i"));
            }

            // Get all records
            var totalCount = await _subjectsCollection.CountDocumentsAsync(filter);

            // Get neccessary filed
            var projection = Builders<SubjectsModel>.Projection
                .Expression(sub => new SubjectDto
                {
                    Id = sub.Id,
                    SubjectName = sub.SubjectName,
                    SubjectStatus = sub.SubjectStatus,
                    TotalQuestionBanks = sub.QuestionBanks.Count
                });

            var subjects = await this._subjectsCollection
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .Project(projection)
                .ToListAsync();

            return (subjects, totalCount);
        }
        
        public async Task<List<SubjectOptionsDto>> GetAllSubjects()
        {
            var filter = Builders<SubjectsModel>.Filter.Ne(sub => sub.SubjectStatus, "deleted");
            
            // Get neccessary filed
            var projection = Builders<SubjectsModel>.Projection
                .Expression(sub => new SubjectOptionsDto
                {
                    Id = sub.Id,
                    SubjectName = sub.SubjectName,
                });

            var subjects = await this._subjectsCollection
                .Find(filter)
                .Project(projection)
                .ToListAsync();

            return subjects;
        }

        // Search or Get All Question Bank Name
        public async Task<(string, string?, List<QuestionBankDto>, long)> GetQuestionBanks(string subjectId, string? keyword, int page, int pageSize)
        {
            // var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
            var filter = Builders<SubjectsModel>.Filter.And(
                Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
                Builders<SubjectsModel>.Filter.Ne(s => s.SubjectStatus, "deleted")
            );

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = Builders<SubjectsModel>.Filter.And(
                    filter,
                    Builders<SubjectsModel>.Filter.ElemMatch(
                        s => s.QuestionBanks,
                        Builders<QuestionBanksModel>.Filter.Regex(q => q.QuestionBankName, new BsonRegularExpression(keyword, "i"))));
            }

            var subjects = await this._subjectsCollection
                .Find(filter)
                .ToListAsync();
            
            var questionBanks = subjects
            .SelectMany(s => s.QuestionBanks
                .Where(qb => string.IsNullOrEmpty(keyword) || qb.QuestionBankName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) && !qb.QuestionBankStatus.Equals("deleted", StringComparison.CurrentCultureIgnoreCase))
                .Select(qb => new QuestionBankDto
                {
                    QuestionBankId = qb.QuestionBankId,
                    QuestionBankName = qb.QuestionBankName,
                    TotalQuestions = qb.QuestionList.Count
                }))
            .ToList();

    // Get questions
    public async Task<(string, string, string, string, List<string>, List<string>, List<QuestionModel>, long)> GetQuestions(string subjectId, string questionBankId, string? keyWord, int page, int pageSize)
    {
        //var filter = Builders<SubjectsModel>.Filter.And(
        //    Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
        //    Builders<SubjectsModel>.Filter.Ne(s => s.SubjectStatus, "deleted")
        //);

            return (subjectId, subjectName, questionBanks, totalQuestionBanks);
        }     
        
        public async Task<List<QuestionBankOptionsDto>> GetQuestionBanksPerSubject(string subjectId)
        {
            // var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
            var filter = Builders<SubjectsModel>.Filter.And(
                Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
                Builders<SubjectsModel>.Filter.Ne(s => s.SubjectStatus, "deleted")
            );

            var subjects = await this._subjectsCollection
                .Find(filter)
                .ToListAsync();
            
            var questionBanks = subjects
            .SelectMany(s => s.QuestionBanks
                .Where(qb => !qb.QuestionBankStatus.Equals("deleted", StringComparison.CurrentCultureIgnoreCase))
                .Select(qb => new QuestionBankOptionsDto
                {
                    QuestionBankId = qb.QuestionBankId,
                    QuestionBankName = qb.QuestionBankName,
                }))
            .ToList();

            return questionBanks;
        }

        // Get questions
        public async Task<(string, string, string, string, List<string>, List<string>, List<QuestionModel>, long)> GetQuestions(string subjectId, string questionBankId, string? keyWord, int page, int pageSize)
        {
            var filter = Builders<SubjectsModel>.Filter.And(
                Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
                Builders<SubjectsModel>.Filter.Ne(s => s.SubjectStatus, "deleted")
            );
            
            var subject = await this._subjectsCollection
               .Find(filter)
               .FirstOrDefaultAsync();

            var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);

            var filteredQuestions = string.IsNullOrEmpty(keyWord)
                ? questionBank?.QuestionList
                : questionBank?.QuestionList.Where(q => q.QuestionText.Contains(keyWord, StringComparison.OrdinalIgnoreCase))
                                   .ToList();


            var totalCount = filteredQuestions?.Count ?? 0;
            
            var paginatedQuestions = (filteredQuestions ?? [])
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            // var result = new QuestionDto
            // {
            //     SubjectId = subject.Id,
            //     SubjectName = subject.SubjectName,
            //     QuestionBankId = questionBank.QuestionBankId,
            //     QuestionBankName = questionBank.QuestionBankName,
            //     Questions = paginatedQuestions,
            // };
            //
            // return new List<QuestionDto> { result };
            return (subject.Id, subject.SubjectName, questionBank.QuestionBankId, questionBank.QuestionBankName, questionBank.AllChapter, questionBank.AllLevel,paginatedQuestions, totalCount);
        }

        // Add subject
        public async Task<string> AddSubject(string subjectName)
        {
            try
            {
                var subject = new SubjectsModel
                {
                    SubjectName = subjectName,
                    QuestionBanks = [],
                };

                await _subjectsCollection.InsertOneAsync(subject);
                return "Add subject successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        
        // Update subject
        public async Task<string> UpdateSubject(string? subjectId, string subjectName)
        {
            try
            {
                var subject = await _subjectsCollection.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
                subject.SubjectName = subjectName;
                await _subjectsCollection.ReplaceOneAsync(s => s.Id == subjectId, subject);
                return "Cập nhật phân môn thành công!";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Add question bank
        public async Task<string> AddQuestionBank(string subjectNameId, string questionBankName)
        {
            try
            {
                var subject = await this._subjectsCollection.Find(s => s.Id == subjectNameId).FirstOrDefaultAsync();

                if (subject == null)
                {
                    return "Not found subject";
                }

                var newQuestionBank = new QuestionBanksModel
                {
                    QuestionBankName = questionBankName,
                    QuestionList = [],
                };

                subject.QuestionBanks.Add(newQuestionBank);

                var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
                await this._subjectsCollection.UpdateOneAsync(s => s.Id == subjectNameId, update);

        var questionBank = subject.QuestionBanks.Find(qb => qb.QuestionBankId == questionBankId);
        if (questionBank == null)
        {
            return "Not found question bank";
        }
            
        var newQuestion = new QuestionModel
        {
            Options = question.Options,
            QuestionType = question.QuestionType,
            QuestionText = question.QuestionText,
            QuestionStatus = question.QuestionStatus,
            IsRandomOrder = question.IsRandomOrder,
            Tags = question.Tags,
        };

        questionBank.QuestionList.Add(newQuestion);
        if (!questionBank.AllChapter.Contains(newQuestion.Tags[0]))
        {
            questionBank.AllChapter.Add(newQuestion.Tags[0]);
        } 
        if (!questionBank.AllLevel.Contains(newQuestion.Tags[1]))
        {
            questionBank.AllLevel.Add(newQuestion.Tags[1]);
        }

        // Add Question
        public async Task<string> AddQuestion(string id, string questionBankId, string userId, SubjectQuestionDto question)
        {
            // try
            // {
                var subject = await this._subjectsCollection.Find(s => s.Id == id & s.SubjectStatus != "deleted").FirstOrDefaultAsync();
                if (subject == null)
                {
                    return "Not found subject";
                }

                var questionBank = subject.QuestionBanks.Find(qb => qb.QuestionBankId == questionBankId);
                if (questionBank == null)
                {
                    return "Not found question bank";
                }
                
                var newQuestion = new QuestionModel
                {
                    Options = question.Options,
                    QuestionType = question.QuestionType,
                    QuestionText = question.QuestionText,
                    QuestionStatus = question.QuestionStatus,
                    IsRandomOrder = question.IsRandomOrder,
                    Tags = question.Tags,
                };

                questionBank.QuestionList.Add(newQuestion);
                if (!questionBank.AllChapter.Contains(newQuestion.Tags[0]))
                {
                    questionBank.AllChapter.Add(newQuestion.Tags[0]);
                } 
                if (!questionBank.AllLevel.Contains(newQuestion.Tags[1]))
                {
                    questionBank.AllLevel.Add(newQuestion.Tags[1]);
                }

    //Add multi question
    public async Task<string> AddMultiQuestion(string subjectId, string questionBankId, string userId, List<SubjectQuestionDto> questions)
    {
        try
        {
            var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null)
            {
                return "Not found subject";
            }

            var questionBank = subject.QuestionBanks.Find(qb => qb.QuestionBankId == questionBankId);
            if (questionBank == null)
            {
                return "Not found question bank";
            }

            foreach (var question in questions)
            {
                var newQuestion = new QuestionModel
                {
                    Options = question.Options,
                    QuestionType = question.QuestionType,
                    QuestionText = question.QuestionText,
                    QuestionStatus = question.QuestionStatus,
                    IsRandomOrder = question.IsRandomOrder,
                    Tags = question.Tags,
                };

                questionBank.QuestionList.Add(newQuestion);

                if (question.Tags != null && question.Tags.Count >= 2)
                {
                    if (!questionBank.AllChapter.Contains(question.Tags[0]))
                    {
                        questionBank.AllChapter.Add(question.Tags[0]);
                    }
                    if (!questionBank.AllLevel.Contains(question.Tags[1]))
                    {
                        questionBank.AllLevel.Add(question.Tags[1]);
                    }
                }
            }

            await _subjectRepository.AddQuestionAsync(subjectId, subject);

            return $"Đã thêm {questions.Count} câu hỏi thành công";
        }
        catch (Exception ex)
        {
            // Có thể log lỗi tại đây nếu cần: _logger.LogError(ex, ...);
            return $"Lỗi xảy ra khi thêm câu hỏi: {ex.Message}";
        }
    }


    // Update Subject Name
    public async Task<string> UpdateSubjectName(string subjectId, string subjectName)
    {
        try
        {
            var result = await _subjectRepository.UpdateSubjectNameAsync(subjectId, subjectName);

                var logInsert = new LogsModel
                {
                    MadeBy = userId,
                    LogAction = "create",
                    LogDetails = "Tạo câu hỏi: " + question.QuestionText
                };
                await _logsCollection.InsertOneAsync(logInsert);
                
                // var user = _usersCollection.Find(u => u.Id == userId).FirstOrDefault();
                // if (user == null) return $"Add question list successfully";
                // user.UserLog ??= [];
                //
                // user.UserLog.Add(new UserLogsModel
                // {
                //     LogAction = "create",
                //     LogDetails = "Tạo câu hỏi: " + question.QuestionText
                // });
                //
                // var updateLogUser = Builders<UsersModel>.Update.Set(u => u.UserLog, user.UserLog);
                // await _usersCollection.UpdateOneAsync(u => u.Id == userId, updateLogUser);

                return $"Thêm câu hỏi thành công";
            // }
            // catch (Exception ex)
            // {
            //     return $"Error: Thêm câu hỏi thất bại {ex.Message}";
            // }
        }

        // Update Subject Name
        public async Task<string> UpdateSubjectName(string id, string subjectName)
        {
            //var result = await this._subjectsCollection.UpdateOneAsync(filter, update);
            var result = await _subjectRepository.UpdateQuestionBankNameAsync(subjectId, questionBankId, questionBankName);

                var result = await this._subjectsCollection.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0 ? "Update subject name successfully" : "Subject not found or no changes made";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Update Question Bank Name
        public async Task<string> UpdateQuestionBankName(string id, string questionBankId, string questionBankName)
        {
            try
            {
                var filter = Builders<SubjectsModel>.Filter.And(
                    Builders<SubjectsModel>.Filter.Eq(s => s.Id, id),
                    Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb => qb.QuestionBankId == questionBankId));

                var update = Builders<SubjectsModel>.Update.Set("QuestionBanks.$.QuestionBankName", questionBankName);

                var result = await this._subjectsCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return "Update question bank name successfully";
                }

                return "Question bank not found or no changes made";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Update Question List
        public async Task<string> UpdateQuestion(string id, string questionBankId, string questionId, string userId, SubjectQuestionDto questionData)
        {
            //var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
            //var update = Builders<SubjectsModel>.Update.Set(s => s.SubjectStatus, "Deleted/Disable");
            //var result = await this._subjectsCollection.UpdateOneAsync(filter, update);
            var result = await _subjectRepository.DeleteSubject(subjectId);

                return result.ModifiedCount > 0 ? "Update question successfully" : "Update failed";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Delete subject
        public async Task<string> DeleteSubject(string subjectId)
        {
            try
            {
                var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);

                var update = Builders<SubjectsModel>.Update.Set(s => s.SubjectStatus, "Deleted/Disable");

                var result = await this._subjectsCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return "Delete subject successfully";
                }
                else
                {
                    return "Subject not found";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Delete question bank
        public async Task<string> DeleteQuestionBank(string subjectId, string questionBankId)
        {
            try
            {
                var filter = Builders<SubjectsModel>.Filter.And(
                    Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
                    Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb => qb.QuestionBankId == questionBankId));

                var update = Builders<SubjectsModel>.Update
                    .Set("questionBanks.$.questionBankStatus", "Deleted/Disable"); // Hoặc "Disabled"

                var result = await this._subjectsCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return "Delete question bank successfully";
                }

                return "Not found question bank";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Delete question in question list
        public async Task<string> DeleteQuestion(string subjectId, string questionBankId, string questionId, string userLogId)
        {
            var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
            var subject = await this._subjectsCollection.Find(filter).FirstOrDefaultAsync();

            if (subject == null)
            {
                return "Subject not found";
            }

            var questionBank = subject.QuestionBanks?.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
            if (questionBank == null)
            {
                return "Question bank not found";
            }

            var question = questionBank.QuestionList.FirstOrDefault(q => q.QuestionId == questionId);
            if (question == null)
            {
                return "Question not found";
            }

            question.QuestionStatus = "Delete/Disable";

            var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
            var result = await this._subjectsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                return "Question disabled successfully";
            }
            else
            {
                return "Update failed or no changes were made";
            }
        }
        
        // Classification tag for create matrix
        public async Task<List<TagsClassification>> GetTagsClassificationAsync(string subjectId, string questionBankId)
        {
            var subject = await _subjectsCollection.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
            if (subject == null) return [];

            var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
            if (questionBank == null) return [];
            
            var tagsDictionary = new Dictionary<(string Chapter, string Level), int>();

            foreach (var question in questionBank.QuestionList)
            {
                if (question.Tags is { Count: >= 2 })
                {
                    var key = (question.Tags[0], question.Tags[1]);
                    if (!tagsDictionary.TryAdd(key, 1))
                    {
                        tagsDictionary[key]++;
                    }
                }
            }

            return tagsDictionary.Select(kvp => new TagsClassification
            {
                Chapter = kvp.Key.Chapter,
                Level = kvp.Key.Level,
                Total = kvp.Value
            }).ToList();
        }

        // Insert sample data
        public async Task InsertSampleDataAsync()
        {
            var sampleData = new List<SubjectsModel>
            {
                new SubjectsModel
                {
                    SubjectName = "Mathematics",
                    QuestionBanks = new List<QuestionBanksModel>
                    {
                        new QuestionBanksModel
                        {
                            QuestionBankName = "Chapter 1",
                            QuestionList = new List<QuestionModel>
                            {
                                new QuestionModel
                                {
                                    QuestionText = "2+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition" }, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = true },
                                        new OptionsModel { OptionText = "5", IsCorrect = false },
                                    },
                                },
                                new QuestionModel
                                {
                                    QuestionText = "3+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition" }, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = false },
                                        new OptionsModel { OptionText = "5", IsCorrect = true },
                                    },
                                },
                                new QuestionModel
                                {
                                    QuestionText = "3+10=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition" }, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "13", IsCorrect = true },
                                        new OptionsModel { OptionText = "14", IsCorrect = false },
                                        new OptionsModel { OptionText = "25", IsCorrect = false },
                                    },
                                },
                            },
                        },
                        new QuestionBanksModel
                        {
                            QuestionBankName = "Chapter 2",
                            QuestionList = new List<QuestionModel>
                            {
                                new QuestionModel
                                {
                                    QuestionText = "2+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition" }, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = true },
                                        new OptionsModel { OptionText = "5", IsCorrect = false },
                                    },
                                },
                                new QuestionModel
                                {
                                    QuestionText = "3+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition" }, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = false },
                                        new OptionsModel { OptionText = "5", IsCorrect = true },
                                    },
                                },
                                new QuestionModel
                                {
                                    QuestionText = "3+10=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition" }, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "13", IsCorrect = true },
                                        new OptionsModel { OptionText = "14", IsCorrect = false },
                                        new OptionsModel { OptionText = "25", IsCorrect = false },
                                    },
                                },
                            },
                        },
                        new QuestionBanksModel
                        {
                            QuestionBankName = "Chapter 3",
                            QuestionList = new List<QuestionModel>
                            {
                                new QuestionModel
                                {
                                    QuestionText = "2+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition" }, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = true },
                                        new OptionsModel { OptionText = "5", IsCorrect = false },
                                    },
                                },
                                new QuestionModel
                                {
                                    QuestionText = "3+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition" }, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = false },
                                        new OptionsModel { OptionText = "5", IsCorrect = true },
                                    },
                                },
                                new QuestionModel
                                {
                                    QuestionText = "3+10=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition" }, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "13", IsCorrect = true },
                                        new OptionsModel { OptionText = "14", IsCorrect = false },
                                        new OptionsModel { OptionText = "25", IsCorrect = false },
                                    },
                                },
                            },
                        },
                    },
                },
                new SubjectsModel
                {
                    SubjectName = "Physics",
                    QuestionBanks = new List<QuestionBanksModel>
                    {
                        new QuestionBanksModel
                        {
                            QuestionBankName = "First Term",
                            QuestionList = new List<QuestionModel>
                            {
                                new QuestionModel
                                {
                                    QuestionText = "What is the speed of light?",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "physics", "light", "speed" },
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3x10^8 m/s", IsCorrect = true },
                                        new OptionsModel { OptionText = "3x10^6 m/s", IsCorrect = false },
                                        new OptionsModel { OptionText = "3x10^10 m/s", IsCorrect = false },
                                    },
                                },
                                new QuestionModel
                                {
                                    QuestionText = "Which law explains why we need seat belts?",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "physics", "newton", "law" },
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "Newton's First Law", IsCorrect = true },
                                        new OptionsModel { OptionText = "Newton's Second Law", IsCorrect = false },
                                        new OptionsModel { OptionText = "Newton's Third Law", IsCorrect = false },
                                    },
                                },
                            },
                        },
                    },
                },
            };

            await this._subjectsCollection.InsertManyAsync(sampleData);
        }
        
        
    }
}
