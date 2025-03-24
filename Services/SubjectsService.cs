using Microsoft.AspNetCore.Mvc;

#pragma warning disable SA1309
namespace Backend_online_testing.Services
{
    using Backend_online_testing.Dtos;
    using Backend_online_testing.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class SubjectsService
    {
        private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
        private readonly IMongoCollection<UsersModel> _usersCollection;

        public SubjectsService(IMongoDatabase database)
        {
            _subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
            _usersCollection = database.GetCollection<UsersModel>("users");
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
                .Where(qb => string.IsNullOrEmpty(keyword) || qb.QuestionBankName.ToLower().Contains(keyword.ToLower()))
                .Select(qb => new QuestionBankDto
                {
                    QuestionBankId = qb.QuestionBankId,
                    QuestionBankName = qb.QuestionBankName,
                    TotalQuestions = qb.QuestionList.Count
                }))
            .ToList();

            var totalQuestionBanks = questionBanks.Count;
            var subjectName = subjects.FirstOrDefault()?.SubjectName;

            return (subjectId, subjectName, questionBanks, totalQuestionBanks);
        }

        // Get questions
        public async Task<(string, string, string, string, List<QuestionModel>, long)> GetQuestions(string subjectId, string questionBankId, string? keyWord, int page, int pageSize)
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
            return (subject.Id, subject.SubjectName, questionBank.QuestionBankId, questionBank.QuestionBankName, paginatedQuestions, totalCount);
        }

        // Add subject
        public async Task<string> AddSubject(string subjectName)
        {
            try
            {
                var subject = new SubjectsModel
                {
                    SubjectName = subjectName,
                    QuestionBanks = new List<QuestionBanksModel>(),
                };

                await this._subjectsCollection.InsertOneAsync(subject);
                return "Add subject successfully";
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
                    QuestionList = new List<QuestionModel>(),
                };

                subject.QuestionBanks.Add(newQuestionBank);

                var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
                await this._subjectsCollection.UpdateOneAsync(s => s.Id == subjectNameId, update);

                return $"Add question bank successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Add Question
        public async Task<string> AddQuestion(string id, string questionBankId, string userId, SubjectQuestionDto question)
        {
            try
            {
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

                var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
                await _subjectsCollection.UpdateOneAsync(s => s.Id == id, update);
                
                var user = _usersCollection.Find(u => u.Id == userId).FirstOrDefault();
                if (user == null) return $"Add question list successfully";
                user.UserLog ??= [];

                user.UserLog.Add(new UserLogsModel
                {
                    LogAction = "create",
                    LogDetails = "Tạo câu hỏi: " + question.QuestionText
                });
                
                var updateLogUser = Builders<UsersModel>.Update.Set(u => u.UserLog, user.UserLog);
                await _usersCollection.UpdateOneAsync(u => u.Id == userId, updateLogUser);

                return $"Add question list successfully";
            }
            catch (Exception ex)
            {
                return $"Error: Add question failure {ex.Message}";
            }
        }

        // Update Subject Name
        public async Task<string> UpdateSubjectName(string id, string subjectName)
        {
            try
            {
                var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, id);
                var update = Builders<SubjectsModel>.Update.Set(s => s.SubjectName, subjectName);

                var result = await this._subjectsCollection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return "Update subject name successfully";
                }

                return "Subject not found or no changes made";
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
        public async Task<string> UpdateQuestion(string id, string questionBankId, string questionId, string userLogId, SubjectQuestionDto questionData)
        {
            try
            {
                var subject = await this._subjectsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
                if (subject == null)
                {
                    return "Subject not found";
                }

                // Find question bank
                var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
                if (questionBank == null)
                {
                    return "QuestionBank not found";
                }

                // Find question
                var questionIndex = questionBank.QuestionList.FindIndex(q => q.QuestionId == questionId);
                if (questionIndex == -1)
                {
                    return "Question not found";
                }

                // Update question data
                questionBank.QuestionList[questionIndex].Options = questionData.Options;
                questionBank.QuestionList[questionIndex].QuestionType = questionData.QuestionType;
                questionBank.QuestionList[questionIndex].QuestionStatus = questionData.QuestionStatus;
                questionBank.QuestionList[questionIndex].QuestionText = questionData.QuestionText;
                questionBank.QuestionList[questionIndex].IsRandomOrder = questionData.IsRandomOrder;
                questionBank.QuestionList[questionIndex].Tags = questionData.Tags;

                // Update data
                var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
                var result = await this._subjectsCollection.UpdateOneAsync(s => s.Id == id, update);

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
