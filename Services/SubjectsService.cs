using backend_online_testing.Models;
using MongoDB.Driver;

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
        public async Task<List<SubjectsModel>> SearchByQuestionBankName(string subjectName,string questionBankName)
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
                    Id = qb.Id,
                    QuestionBankName = qb.QuestionBankName,
                    List = qb.List
                        .Where(q => q.QuestionText.ToLower().Contains(questionName.ToLower())) // Lọc câu hỏi
                        .ToList()
                })
                .ToList()
            });

            return await _subjectsCollection.Find(filter).Project(projection).ToListAsync();
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
