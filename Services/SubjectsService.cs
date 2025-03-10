using backend_online_testing.Dtos;
using backend_online_testing.Models;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace backend_online_testing.Services
{
    public class SubjectsService
    {
        private readonly IMongoCollection<SubjectsModel> _subjectsCollection;

        public SubjectsService(IMongoDatabase database)
        {
            _subjectsCollection = database.GetCollection<SubjectsModel>("Subjects");
        }
        //Find all
        public async Task<List<SubjectsModel>> GetAllSubjects()
        {
            return await _subjectsCollection.Find(_ => true).ToListAsync();
        }
        //Search by Subject name
        public async Task<List<SubjectsModel>> SearchBySubjectName(string subjectName)
        {
            var filter = Builders<SubjectsModel>.Filter.Regex(s => s.SubjectName, new MongoDB.Bson.BsonRegularExpression(subjectName, "i"));
            return await _subjectsCollection.Find(filter).ToListAsync();
        }
        //Search by Question Bank Name
        public async Task<List<SubjectsModel>> SearchByQuestionBankName(string subjectName, string questionBankName)
        {
            var filter = Builders<SubjectsModel>.Filter.And(
                Builders<SubjectsModel>.Filter.Regex(s => s.SubjectName, new MongoDB.Bson.BsonRegularExpression(subjectName, "i")),
                Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb =>
                qb.QuestionBankName.ToLower().Contains(questionBankName.ToLower()))
            );

            var projection = Builders<SubjectsModel>.Projection.Expression(subject => new SubjectsModel
            {
                Id = subject.Id,
                SubjectName = subject.SubjectName,
                QuestionBanks = subject.QuestionBanks
                .Where(qb => qb.QuestionBankName.ToLower().Contains(questionBankName.ToLower()))
                .ToList()
            });

            return await _subjectsCollection.Find(filter).Project(projection).ToListAsync();
        }

        public async Task<List<SubjectsModel>> SearchByQuestionName(string subjectName, string questionBankName, string questionName)
        {
            var filter = Builders<SubjectsModel>.Filter.And(
                Builders<SubjectsModel>.Filter.Eq(s => s.SubjectName, subjectName), // Khớp 100%
                Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb =>
                    qb.QuestionBankName == questionBankName && // Khớp 100%
                    qb.List.Any(q => q.QuestionText.ToLower().Contains(questionName.ToLower())) // Chứa questionName
                )
            );

            var projection = Builders<SubjectsModel>.Projection.Expression(subject => new SubjectsModel
            {
                Id = subject.Id,
                SubjectName = subject.SubjectName,
                QuestionBanks = subject.QuestionBanks
                .Where(qb => qb.QuestionBankName == questionBankName) // Chỉ lấy đúng QuestionBankName
                .Select(qb => new QuestionBanksModel
                {
                    QuestionBankId = qb.QuestionBankId,
                    QuestionBankName = qb.QuestionBankName,
                    List = qb.List
                        .Where(q => q.QuestionText.ToLower().Contains(questionName.ToLower())) // Lọc câu hỏi
                        .ToList()
                })
                .ToList()
            });

            return await _subjectsCollection.Find(filter).Project(projection).ToListAsync();
        }
        //Add subject
        public async Task<string> AddSubject(string subjectName)
        {
            try
            {
                var subject = new SubjectsModel
                {
                    SubjectName = subjectName,
                    QuestionBanks = new List<QuestionBanksModel>()
                };

                await _subjectsCollection.InsertOneAsync(subject);
                return ("Add subject successfully");
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        //Add question bank
        public async Task<string> AddQuestionBank(string subjectNameId, string questionBankName)
        {
            try
            {
                var subject = await _subjectsCollection.Find(s => s.Id == subjectNameId).FirstOrDefaultAsync();

                if (subject == null)
                {
                    return "Not found subject";
                }

                var newQuestionBank = new QuestionBanksModel
                {
                    QuestionBankName = questionBankName,
                    List = new List<QuestionListModel>()
                };

                subject.QuestionBanks.Add(newQuestionBank);

                var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
                await _subjectsCollection.UpdateOneAsync(s => s.Id == subjectNameId, update);

                return $"Add question bank successfully";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        //Add Question List
        public async Task<string> AddQuestionsList(string id, string questionBankId, string questionLogUserId, List<SubjectQuestionDto> questionsList)
        {
            try
            {
                var subject = await _subjectsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
                if (subject == null)
                {
                    return "Not found subject";
                }
                var questionBank = subject.QuestionBanks.Find(qb => qb.QuestionBankId == questionBankId);
                if (questionBank == null)
                {
                    return "Not found question bank";
                }

                foreach (var questionDto in questionsList)
                {
                    var questionAddLog = new QuestionLogsModel
                    {
                        QuestionLogType = "Added question",
                        QuestionLogUserId = questionLogUserId,
                        QuestionLogAt = DateTime.Now,
                    };

                    var newQuestion = new QuestionListModel
                    {
                        Options = questionDto.Options,
                        QuestionType = questionDto.QuestionType,
                        QuestionText = questionDto.QuestionText,
                        QuestionStatus = questionDto.QuestionStatus,
                        IsRandomOrder = questionDto.IsRandomOrder,
                        Tags = questionDto.Tags,
                        QuestionLogs = new List<QuestionLogsModel> { questionAddLog }
                    };

                    questionBank.List.Add(newQuestion);
                }

                var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
                await _subjectsCollection.UpdateOneAsync(s => s.Id == id, update);

                return $"Add question list successfully";
            }
            catch (Exception ex)
            {
                return $"Error: Add question failure {ex.Message}";
            }
        }
        //Update Subject Name
        public async Task<string> UpdateSubjectName(string id, string subjectName)
        {
            try
            {
                var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, id);
                var update = Builders<SubjectsModel>.Update.Set(s => s.SubjectName, subjectName);

                var result = await _subjectsCollection.UpdateOneAsync(filter, update);

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

        //Update Question Bank Name
        public async Task<string> UpdateQuestionBankName(string id, string questionBankId, string questionBankName)
        {
            try
            {
                var filter = Builders<SubjectsModel>.Filter.And(
                    Builders<SubjectsModel>.Filter.Eq(s => s.Id, id),
                    Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb => qb.QuestionBankId == questionBankId)
                );

                var update = Builders<SubjectsModel>.Update.Set("QuestionBanks.$.QuestionBankName", questionBankName);

                var result = await _subjectsCollection.UpdateOneAsync(filter, update);

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

        //Update Question List
        public async Task<string> UpdateQuestion(string id, string questionBankId, string questionId, string userLogId, SubjectQuestionDto questionData)
        {
            try
            {
                var subject = await _subjectsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
                if (subject == null)
                    return "Subject not found";

                // Find question bank
                var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
                if (questionBank == null)
                    return "QuestionBank not found";

                // Find question
                var questionIndex = questionBank.List.FindIndex(q => q.QuestionId == questionId);
                if (questionIndex == -1)
                    return "Question not found";
                var updateLog = new QuestionLogsModel
                {
                    QuestionLogType = "Updated question",
                    QuestionLogUserId = userLogId,
                    QuestionLogAt = DateTime.Now
                };

                // Update question data
                questionBank.List[questionIndex].Options = questionData.Options;
                questionBank.List[questionIndex].QuestionType = questionData.QuestionType;
                questionBank.List[questionIndex].QuestionStatus = questionData.QuestionStatus;
                questionBank.List[questionIndex].QuestionText = questionData.QuestionText;
                questionBank.List[questionIndex].IsRandomOrder = questionData.IsRandomOrder;
                questionBank.List[questionIndex].Tags = questionData.Tags;
                questionBank.List[questionIndex].QuestionLogs.Add(updateLog);

                // Update data
                var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
                var result = await _subjectsCollection.UpdateOneAsync(s => s.Id == id, update);

                return result.ModifiedCount > 0 ? "Update question successfully" : "Update failed";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
        //Delete subject
        public async Task<string> DeleteSubject(string subjectId)
        {
            try
            {
                var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);

                var update = Builders<SubjectsModel>.Update.Set(s => s.SubjectStatus, "Deleted/Disable");

                var result = await _subjectsCollection.UpdateOneAsync(filter, update);

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
        //Delete question bank
        public async Task<string> DeleteQuestionBank(string subjectId, string questionBankId)
        {
            try
            {
                var filter = Builders<SubjectsModel>.Filter.And(
                    Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
                    Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb => qb.QuestionBankId == questionBankId)
                );

                var update = Builders<SubjectsModel>.Update
                    .Set("questionBanks.$.questionBankStatus", "Deleted/Disable"); // Hoặc "Disabled"

                var result = await _subjectsCollection.UpdateOneAsync(filter, update);

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
        //Delete question in question list
        public async Task<string> DeleteQuestion(string subjectId, string questionBankId, string questionId, string userLogId)
        {
            var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
            var subject = await _subjectsCollection.Find(filter).FirstOrDefaultAsync();

            if (subject == null)
            {
                return "Subject not found";
            }

            var questionBank = subject.QuestionBanks?.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
            if (questionBank == null)
            {
                return "Question bank not found";
            }

            var question = questionBank.List.FirstOrDefault(q => q.QuestionId == questionId);
            if (question == null)
            {
                return "Question not found";
            }

            question.QuestionStatus = "Delete/Disable";

            question.QuestionLogs ??= new List<QuestionLogsModel>();
            question.QuestionLogs.Add(new QuestionLogsModel
            {
                QuestionLogUserId = userLogId,
                QuestionLogType = "Disabled",
                QuestionLogAt = DateTime.UtcNow
            });

            var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
            var result = await _subjectsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                return "Question disabled successfully";
            }
            else
            {
                return "Update failed or no changes were made";
            }
        }

        //Insert sample data
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
                            List = new List<QuestionListModel>
                            {
                                new QuestionListModel
                                {
                                    QuestionText = "2+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition"}, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = true },
                                        new OptionsModel { OptionText = "5", IsCorrect = false }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel> // Thêm logs
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                },
                                new QuestionListModel
                                {
                                    QuestionText = "3+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition"}, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = false },
                                        new OptionsModel { OptionText = "5", IsCorrect = true }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel> // Thêm logs
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                },
                                new QuestionListModel
                                {
                                    QuestionText = "3+10=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition"}, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "13", IsCorrect = true },
                                        new OptionsModel { OptionText = "14", IsCorrect = false },
                                        new OptionsModel { OptionText = "25", IsCorrect = false }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel> // Thêm logs
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                }
                            }
                        },
                        new QuestionBanksModel
                        {
                            QuestionBankName = "Chapter 2",
                            List = new List<QuestionListModel>
                            {
                                new QuestionListModel
                                {
                                    QuestionText = "2+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition"}, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = true },
                                        new OptionsModel { OptionText = "5", IsCorrect = false }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel> // Thêm logs
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                },
                                new QuestionListModel
                                {
                                    QuestionText = "3+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition"}, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = false },
                                        new OptionsModel { OptionText = "5", IsCorrect = true }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel> // Thêm logs
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                },
                                new QuestionListModel
                                {
                                    QuestionText = "3+10=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition"}, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "13", IsCorrect = true },
                                        new OptionsModel { OptionText = "14", IsCorrect = false },
                                        new OptionsModel { OptionText = "25", IsCorrect = false }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel> // Thêm logs
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                }
                            }
                        },
                        new QuestionBanksModel
                        {
                            QuestionBankName = "Chapter 3",
                            List = new List<QuestionListModel>
                            {
                                new QuestionListModel
                                {
                                    QuestionText = "2+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition"}, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = true },
                                        new OptionsModel { OptionText = "5", IsCorrect = false }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel> // Thêm logs
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                },
                                new QuestionListModel
                                {
                                    QuestionText = "3+2=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition"}, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "3", IsCorrect = false },
                                        new OptionsModel { OptionText = "4", IsCorrect = false },
                                        new OptionsModel { OptionText = "5", IsCorrect = true }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel> // Thêm logs
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                },
                                new QuestionListModel
                                {
                                    QuestionText = "3+10=",
                                    QuestionType = "Multiple Choice",
                                    QuestionStatus = "Active",
                                    IsRandomOrder = true,
                                    Tags = new List<string> { "math", "addition"}, // Thêm tags
                                    Options = new List<OptionsModel>
                                    {
                                        new OptionsModel { OptionText = "13", IsCorrect = true },
                                        new OptionsModel { OptionText = "14", IsCorrect = false },
                                        new OptionsModel { OptionText = "25", IsCorrect = false }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel> // Thêm logs
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                }
                            }
                        },
                    }
                },
                new SubjectsModel
                {
                    SubjectName = "Physics",
                    QuestionBanks = new List<QuestionBanksModel>
                    {
                        new QuestionBanksModel
                        {
                            QuestionBankName = "First Term",
                            List = new List<QuestionListModel>
                            {
                                new QuestionListModel
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
                                        new OptionsModel { OptionText = "3x10^10 m/s", IsCorrect = false }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel>
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                },
                                new QuestionListModel
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
                                        new OptionsModel { OptionText = "Newton's Third Law", IsCorrect = false }
                                    },
                                    QuestionLogs = new List<QuestionLogsModel>
                                    {
                                        new QuestionLogsModel
                                        {
                                            QuestionLogType = "Created",
                                            QuestionLogUserId = "67be73d4bf5972f0ae87fb37",
                                            QuestionLogAt = DateTime.UtcNow
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            };
            await _subjectsCollection.InsertManyAsync(sampleData);
        }
    }
}
