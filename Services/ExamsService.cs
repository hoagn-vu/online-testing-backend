namespace Backend_online_testing.Services
{
    using Backend_online_testing.DTO;
    using Backend_online_testing.Dtos;
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class ExamsService
    {
        private readonly IMongoCollection<ExamsModel> _examsCollection;
        private readonly IMongoCollection<SubjectsModel> _subjectsCollection;

        public ExamsService(IMongoDatabase database)
        {
            this._examsCollection = database.GetCollection<ExamsModel>("exams");
            this._subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
        }

        // Find all document
        public async Task<(List<ExamResponseDto>, long)> GetExams(string? keyword, int page, int pageSize)
        {
            var filter = Builders<ExamsModel>.Filter.Ne(ex => ex.ExamStatus, "deleted");
            if (!string.IsNullOrEmpty(keyword))
            {
                // filter = Builders<ExamsModel>.Filter.Regex(ex => ex.ExamName, new BsonRegularExpression(keyword, "i"));
                filter = Builders<ExamsModel>.Filter.Or(
                    Builders<ExamsModel>.Filter.Regex(ex => ex.ExamName, new BsonRegularExpression(keyword, "i")),
                    Builders<ExamsModel>.Filter.Regex(ex => ex.ExamCode, new BsonRegularExpression(keyword, "i")));
            }
            
            var exams = await _examsCollection
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
            
            var totalCount = await _examsCollection.CountDocumentsAsync(filter);

            var examResponseList = new List<ExamResponseDto>();

            foreach (var exam in exams)
            {
                // Lấy thông tin môn học
                var subject = await _subjectsCollection.Find(s => s.Id == exam.SubjectId).FirstOrDefaultAsync();
                var subjectName = subject?.SubjectName ?? string.Empty;

                // Lấy thông tin ngân hàng câu hỏi từ danh sách QuestionBanks trong Subject
                var questionBankName = string.Empty;
                if (subject != null)
                {
                    var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == exam.QuestionBankId);
                    questionBankName = questionBank?.QuestionBankName ?? string.Empty;
                }

                examResponseList.Add(new ExamResponseDto
                {
                    Id = exam.Id,
                    ExamCode = exam.ExamCode,
                    ExamName = exam.ExamName,
                    SubjectId = exam.SubjectId,
                    SubjectName = subjectName,
                    ExamStatus = exam.ExamStatus,
                    QuestionBankId = exam.QuestionBankId,
                    QuestionBankName = questionBankName
                });
            }

            return (examResponseList, totalCount);
        }

        // Get question by exam code
        public async Task<ExamsModel?> GetExamByCodeAsync(string examId)
        {
            return await _examsCollection
                .Find(e => e.Id == examId)
                .FirstOrDefaultAsync();
        }
        public async Task<(string,ExamDetailResponseDto?)> GetExamQuestionsWithDetailsAsync(string examId)
        {
            var exam = await _examsCollection.Find(e => e.Id == examId).FirstOrDefaultAsync();
            if (exam == null)
            {
                return ("error-exam", null);
            }

            // Tìm subject
            var subject = await _subjectsCollection
                .Find(s => s.Id == exam.SubjectId)
                .FirstOrDefaultAsync();

            if (subject == null)
            {
                return ("error-subject", null);
            }

            var questionBank = subject.QuestionBanks
                .FirstOrDefault(qb => qb.QuestionBankId == exam.QuestionBankId);

            if (questionBank == null)
            {
                return ("error-questionBank", null);
            }

            var detailedQuestions = new List<ExamQuestionDetailDTO>();

            foreach (var examQ in exam.QuestionSet)
            {
                var realQuestion = questionBank.QuestionList
                    .FirstOrDefault(q => q.QuestionId == examQ.QuestionId);

                if (realQuestion == null)
                {
                    Console.WriteLine($"    ❌ Không tìm thấy câu hỏi trong questionBank!");
                }
                else
                {
                    string chapter = realQuestion.Tags?.Count > 0 ? realQuestion.Tags[0] : "";
                    string level = realQuestion.Tags?.Count > 1 ? realQuestion.Tags[1] : "";

                    detailedQuestions.Add(new ExamQuestionDetailDTO
                    {
                        
                        QuestionId = realQuestion.QuestionId,
                        QuestionText = realQuestion.QuestionText,
                        Chapter = chapter,
                        Level = level,
                        QuestionScore = (double)examQ.QuestionScore,
                        Options = realQuestion.Options.Select(o => new OptionDetailDTO
                        {
                            OptionId = o.OptionId,
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect
                        }).ToList()
                    });
                }
            }
            var examDetail = new ExamDetailResponseDto
            {
                Id = exam.Id,
                ExamCode = exam.ExamCode,
                ExamName = exam.ExamName,
                SubjectName = subject.SubjectName,
                QuestionBankName = questionBank.QuestionBankName,
                ListQuestion = detailedQuestions,

            };

            return ("Ok",examDetail);
        }

        // Create Exam
        public async Task<string> CreateExam(ExamDto createExamData)
        {
            // Check name exam is existed
            var existingExam = await this._examsCollection.Find(e => e.ExamName == createExamData.ExamName).FirstOrDefaultAsync();
            if (existingExam != null)
            {
                return "Exam name already exists.";
            }

            // var logCreateData = new ExamLogsModel
            // {
            //    ExamLogUserId = createExamData.ExamLogUserId,
            //    ExamLogType = "Created",
            //    ExamChangeAt = DateTime.Now,
            // };
            var newExamData = new ExamsModel
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ExamCode = createExamData.ExamCode,
                ExamName = createExamData.ExamName,
                SubjectId = createExamData.SubjectId,
                ExamStatus = createExamData.ExamStatus,
                QuestionBankId = createExamData.QuestionBankId,
                QuestionSet = new List<QuestionSetsModel>(),

                // ExamLogs = new List<ExamLogsModel> { logCreateData },
            };

            try
            {
                await this._examsCollection.InsertOneAsync(newExamData);
                return "Exam created successfully.";
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error inserting exam: {ex.Message}");
                return $"Failed to create exam: {ex.Message}";
            }
        }

        // Update Exam(Not include question)
        public async Task<bool> UpdateExam(ExamDto updateExamData, string examId, string userLogId)
        {
            // var logUpdateData = new ExamLogsModel
            // {
            //    ExamLogUserId = userLogId,
            //    ExamLogType = "Update",
            //    ExamChangeAt = DateTime.Now,
            // };
            var filter = Builders<ExamsModel>.Filter.Eq(e => e.Id, examId);
            var update = Builders<ExamsModel>.Update
                .Set(e => e.ExamName, updateExamData.ExamName)
                .Set(e => e.ExamCode, updateExamData.ExamCode)
                .Set(e => e.SubjectId, updateExamData.SubjectId)
                .Set(e => e.ExamStatus, updateExamData.ExamStatus)
                .Set(e => e.QuestionBankId, updateExamData.QuestionBankId);

            // .Push(e => e.ExamLogs, logUpdateData);
            var result = await this._examsCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // Add question one/list
        public async Task<string> AddExamQuestion([FromBody] ExamQuestionDTO questionData, string examId, string userLogId)
        {
            if (questionData == null)
            {
                return "Invalid data";
            }

            var filter = Builders<ExamsModel>.Filter.Eq(e => e.Id, examId);
            var exam = await this._examsCollection.Find(filter).FirstOrDefaultAsync();

            if (exam == null)
            {
                return "Exam not found";
            }

            var existingQuestions = exam.QuestionSet.Select(q => q.QuestionId).ToHashSet();
            var newQuestions = questionData.QuestionSets.Where(q => !existingQuestions.Contains(q.QuestionId)).ToList();

            if (!newQuestions.Any())
            {
                return "Question already exists";
            }

            var updateQuestions = Builders<ExamsModel>.Update.PushEach(e => e.QuestionSet, newQuestions);
            var result = await this._examsCollection.UpdateOneAsync(filter, updateQuestions);

            return result.ModifiedCount > 0 ? "Question added successfully" : "Failure create question";
        }

        // Update Question
        public async Task<string> UpdateExamQuestion(string examId, string questionId, string userLogId, double questionScore)
        {
            // if (questionData == null || examId == null || questionData.QuestionSets == null || questionData.QuestionSets.Count == 0)
            // {
            //    return "Invalid data";
            // }

            // var questionSet = questionData.QuestionSets.First();
            // var questionScore = questionSet.QuestionScore;
            var filter = Builders<ExamsModel>.Filter.And(
                Builders<ExamsModel>.Filter.Eq(e => e.Id, examId),
                Builders<ExamsModel>.Filter.ElemMatch(e => e.QuestionSet, qs => qs.QuestionId == questionId));

            var update = Builders<ExamsModel>.Update.Set("QuestionSet.$.QuestionScore", questionScore);

            var result = await this._examsCollection.UpdateOneAsync(filter, update);

            // var updateQuestionLog = new ExamLogsModel
            // {
            //    ExamLogUserId = userLogId,
            //    ExamLogType = "Update Question",
            //    ExamChangeAt = DateTime.Now,
            // };
            if (result.ModifiedCount > 0)
            {
                // var addLog = Builders<ExamsModel>.Update.Push(e => e.ExamLogs, updateQuestionLog);
                // var logResult = await this._examsCollection.UpdateOneAsync(filter, addLog);
                return "Question updated successfully";
            }

            return "Question not found";
        }

        // Delete Question in Exam
        public async Task<string> DeleteExamQuestion(string examId, string questionId, string userLogId)
        {
            // if (questionData == null || examId == null || questionData.QuestionSets == null || questionData.QuestionSets.Count == 0)
            // {
            //    return "Invalid data";
            // }
            // var question = questionData.QuestionSets.First();
            var filter = Builders<ExamsModel>.Filter.And(
                Builders<ExamsModel>.Filter.Eq(e => e.Id, examId),
                Builders<ExamsModel>.Filter.ElemMatch(e => e.QuestionSet, qs => qs.QuestionId == questionId));

            var delete = Builders<ExamsModel>.Update.PullFilter(e => e.QuestionSet, qs => qs.QuestionId == questionId);

            var result = await this._examsCollection.UpdateOneAsync(filter, delete);

            // var deleteQuestionLog = new ExamLogsModel
            // {
            //    ExamLogUserId = userLogId,
            //    ExamLogType = "Delete Question",
            //    ExamChangeAt = DateTime.Now,
            // };
            if (result.ModifiedCount > 0)
            {
                // var filterExam = Builders<ExamsModel>.Filter.Eq(e => e.Id, examId);

                // var updateLog = Builders<ExamsModel>.Update.Push(e => e.ExamLogs, deleteQuestionLog);
                // var logResult = await this._examsCollection.UpdateOneAsync(filterExam, updateLog);
                return "Question deleted successfully";
            }

            return "Question not found or already deleted";
        }

        // Insert example data
        public async Task SeedData()
        {
            var exampleExams = new List<ExamsModel>
            {
                new ExamsModel
                {
                    ExamCode = "EX123",
                    ExamName = "Math xam 1",
                    SubjectId = "MATH001",
                    QuestionBankId = "67ce3d5ac07467bf499bfdfe",
                    QuestionSet = new List<QuestionSetsModel>
                    {
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 5.0 },
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 3.0 },
                    },
                    ExamStatus = "Active",
                },
                new ExamsModel
                {
                    ExamCode = "EX124",
                    ExamName = "History exam 2",
                    SubjectId = "HIS002",
                    QuestionBankId = "67ce3d5ac07467bf499bfdfe",
                    QuestionSet = new List<QuestionSetsModel>
                    {
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 4.0 },
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 2.5 },
                    },
                    ExamStatus = "Pending",
                },
                new ExamsModel
                {
                    ExamCode = "EX125",
                    ExamName = "Sample Exam 3",
                    SubjectId = "GEO003",
                    QuestionBankId = "67ce3d5ac07467bf499bfdfe",
                    QuestionSet = new List<QuestionSetsModel>
                    {
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 6.0 },
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 4.5 },
                    },
                    ExamStatus = "Completed",
                },
            };

            await this._examsCollection.InsertManyAsync(exampleExams);
        }
    }
}
