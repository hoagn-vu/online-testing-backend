using backend_online_testing.DTO;
using backend_online_testing.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace backend_online_testing.Services
{
    public class ExamsService
    {
        public readonly IMongoCollection<ExamsModel> _examsCollection;

        public ExamsService(IMongoDatabase database)
        {
            _examsCollection = database.GetCollection<ExamsModel>("Exams");
        }
        //Find all document
        public async Task<List<ExamsModel>> FindExam()
        {
            return await _examsCollection.Find(_ => true).ToListAsync();
        }

        //Find Exam using Name
        public async Task<List<ExamsModel>> FindExamByName(string examName)
        {
            if (string.IsNullOrEmpty(examName))
                return new List<ExamsModel>();

            var filter = Builders<ExamsModel>.Filter.Regex(e => e.ExamName, new BsonRegularExpression(examName, "i")); 

            return await _examsCollection.Find(filter).ToListAsync();
        }

        //Create Exam 
        public async Task<string> CreateExam(ExamDTO createExamData)
        {
            //Check name exam is existed
            var existingExam = await _examsCollection.Find(e => e.ExamName == createExamData.ExamName).FirstOrDefaultAsync();
            if (existingExam != null)
            {
                return "Exam name already exists.";
            }

            var logCreateData = new ExamLogsModel
            {
                ExamLogUserId = createExamData.ExamLogUserId,
                ExamLogType = "Created",
                ExamChangeAt = DateTime.Now
            };

            var newExamData = new ExamsModel
            {
                Id = ObjectId.GenerateNewId().ToString(),
                ExamCode = createExamData.ExamCode,
                ExamName = createExamData.ExamName,
                SubjectId = createExamData.SubjectId,
                ExamStatus = createExamData.ExamStatus,
                QuestionSet = new List<QuestionSetsModel>(),
                ExamLogs = new List<ExamLogsModel> { logCreateData }
            };
            try
            {
                await _examsCollection.InsertOneAsync(newExamData);
                return "Exam created successfully.";
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error inserting exam: {ex.Message}");
                return "Failed to create exam.";
            }
            
        }

        //Update Exam(Not include question)
        public async Task<bool> UpdateExam(ExamDTO updateExamData)
        {
            var logUpdateData = new ExamLogsModel
            {
                ExamLogUserId = updateExamData.ExamLogUserId,
                ExamLogType = "Update",
                ExamChangeAt = DateTime.Now
            };

            var filter = Builders<ExamsModel>.Filter.Eq(e => e.Id, updateExamData.Id);
            var update = Builders<ExamsModel>.Update
                .Set(e => e.ExamName, updateExamData.ExamName)
                .Set(e => e.ExamCode, updateExamData.ExamCode)
                .Set(e => e.SubjectId, updateExamData.SubjectId)
                .Set(e => e.ExamStatus, updateExamData.ExamStatus)
                .Push(e => e.ExamLogs, logUpdateData);
                //.PushEach(e => e.QuestionSet, updateExamData.QuestionSets);

            var result = await _examsCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        //Add question one/list
        public async Task<string> AddExamQuestion([FromBody] ExamQuestionDTO questionData)
        {
            if (questionData == null || string.IsNullOrEmpty(questionData.Id) || questionData.QuestionSets == null || questionData.QuestionSets.Count == 0)
                return "Invalid data";

            var filter = Builders<ExamsModel>.Filter.Eq(e => e.Id, questionData.Id);

            var updateQuestions = Builders<ExamsModel>.Update.PushEach(e => e.QuestionSet, questionData.QuestionSets);

            var result = await _examsCollection.UpdateOneAsync(filter, updateQuestions);

            if (result.ModifiedCount > 0)
            {
                var createQuestionLog = new ExamLogsModel
                {
                    ExamLogUserId = questionData.ExamLogUserId,
                    ExamLogType = "Create Question",
                    ExamChangeAt = DateTime.Now
                };

                var addLog = Builders<ExamsModel>.Update.Push(e => e.ExamLogs, createQuestionLog);
                var logResult = await _examsCollection.UpdateOneAsync(filter, addLog);

                return logResult.ModifiedCount > 0 ? "Question added successfully" : "Failure update log";
            }

            return "Failure create question";
        }

        //Update Question
        public async Task<string> UpdateExamQuestion([FromBody] ExamQuestionDTO questionData)
        {
            if (questionData == null || questionData.Id == null || questionData.QuestionSets == null || questionData.QuestionSets.Count == 0)
                return "Invalid data";

            var questionSet = questionData.QuestionSets.First();
            var questionId = questionSet.QuestionId;
            var questionScore = questionSet.QuestionScore;

            var filter = Builders<ExamsModel>.Filter.And(
                Builders<ExamsModel>.Filter.Eq(e => e.Id, questionData.Id), 
                Builders<ExamsModel>.Filter.ElemMatch(e => e.QuestionSet, qs => qs.QuestionId == questionId)
            );

            var update = Builders<ExamsModel>.Update.Set("QuestionSet.$.QuestionScore", questionScore);

            var result = await _examsCollection.UpdateOneAsync(filter, update);

            var updateQuestionLog = new ExamLogsModel
            {
                ExamLogUserId = questionData.ExamLogUserId,
                ExamLogType = "Update Question",
                ExamChangeAt = DateTime.Now
            };

            if (result.ModifiedCount > 0)
            {
                var addLog = Builders<ExamsModel>.Update.Push(e => e.ExamLogs, updateQuestionLog);

                var logResult = await _examsCollection.UpdateOneAsync(filter, addLog);

                return logResult.ModifiedCount > 0 ? "Question updated successfully" : "Failure update log";
            }
            return "Question not found";
        }

        //Delete Question in Exam
        public async Task<string> DeleteExamQuestion([FromBody] ExamQuestionDTO questionData)
        {
            if (questionData == null || questionData.Id == null || questionData.QuestionSets == null || questionData.QuestionSets.Count == 0)
                return "Invalid data";
            var question = questionData.QuestionSets.First();
            var questionId = question.QuestionId;

            var filter = Builders<ExamsModel>.Filter.And(
                Builders<ExamsModel>.Filter.Eq(e=>e.Id, questionData.Id),
                Builders<ExamsModel>.Filter.ElemMatch(e=>e.QuestionSet, qs=>qs.QuestionId == questionId)
            );

            var delete = Builders<ExamsModel>.Update.PullFilter(e => e.QuestionSet, qs => qs.QuestionId == questionId);

            var result = await _examsCollection.UpdateOneAsync(filter, delete);

            var deleteQuestionLog = new ExamLogsModel
            {
                ExamLogUserId = questionData.ExamLogUserId,
                ExamLogType = "Delete Question",
                ExamChangeAt = DateTime.Now
            };

            if (result.ModifiedCount > 0)
            {
                var filterExam = Builders<ExamsModel>.Filter.Eq(e => e.Id, questionData.Id);

                var updateLog = Builders<ExamsModel>.Update.Push(e => e.ExamLogs, deleteQuestionLog);

                var logResult = await _examsCollection.UpdateOneAsync(filterExam, updateLog);

                return logResult.ModifiedCount > 0 ? "Question deleted successfully" : "Failure update log";
            }
            return "Question not found or already deleted";
        }

        //Insert example data
        public async Task SeedData()
        {
            var exampleExams = new List<ExamsModel>
            {
                new ExamsModel
                {
                    ExamCode = "EX123",
                    ExamName = "Math xam 1",
                    SubjectId = "MATH001",
                    QuestionSet = new List<QuestionSetsModel>
                    {
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 5.0 },
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 3.0 }
                    },
                    ExamStatus = "Active",
                    ExamLogs = new List<ExamLogsModel>
                    {
                        new ExamLogsModel { ExamLogUserId = ObjectId.GenerateNewId().ToString(), ExamLogType = "Created", ExamChangeAt = DateTime.UtcNow }
                    }
                },
                new ExamsModel
                {
                    ExamCode = "EX124",
                    ExamName = "History exam 2",
                    SubjectId = "HIS002",
                    QuestionSet = new List<QuestionSetsModel>
                    {
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 4.0 },
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 2.5 }
                    },
                    ExamStatus = "Pending",
                    ExamLogs = new List<ExamLogsModel>
                    {
                        new ExamLogsModel { ExamLogUserId = ObjectId.GenerateNewId().ToString(), ExamLogType = "Updated", ExamChangeAt = DateTime.UtcNow }
                    }
                },
                new ExamsModel
                {
                    ExamCode = "EX125",
                    ExamName = "Sample Exam 3",
                    SubjectId = "GEO003",
                    QuestionSet = new List<QuestionSetsModel>
                    {
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 6.0 },
                        new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 4.5 }
                    },
                    ExamStatus = "Completed",
                    ExamLogs = new List<ExamLogsModel>
                    {
                        new ExamLogsModel { ExamLogUserId = ObjectId.GenerateNewId().ToString(), ExamLogType = "Finalized", ExamChangeAt = DateTime.UtcNow }
                    }
                }
            };

            await _examsCollection.InsertManyAsync(exampleExams);
        }
    }
}
