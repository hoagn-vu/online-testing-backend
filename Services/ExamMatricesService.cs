﻿#pragma warning disable SA1309
namespace Backend_online_testing.Services
{
    using Backend_online_testing.Dtos;
    using Backend_online_testing.Models;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Microsoft.Extensions.Logging.Abstractions;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class ExamMatricesService
    {
        private readonly IMongoCollection<ExamMatricesModel> _examMatrixsCollection;

        public ExamMatricesService(IMongoDatabase database)
        {
            this._examMatrixsCollection = database.GetCollection<ExamMatricesModel>("examMatrices");
        }

        public async Task<(List<ExamMatricesModel>, long)> GetExamMatrices(string? keyword, int page, int pageSize)
        {
            var filter = Builders<ExamMatricesModel>.Filter.Ne(em => em.MatrixStatus, "deleted");

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = Builders<ExamMatricesModel>.Filter.Or(
                    Builders<ExamMatricesModel>.Filter.Regex(ex => ex.MatrixName, new BsonRegularExpression(keyword, "i")),
                    Builders<ExamMatricesModel>.Filter.Regex(ex => ex.MatrixStatus, new BsonRegularExpression(keyword, "i")));
            }

            var examMatrices = await _examMatrixsCollection
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var totalCount = await _examMatrixsCollection.CountDocumentsAsync(filter);

            return (examMatrices, totalCount);
        }

        public async Task<ExamMatricesModel> GetByIdExamMatrix(string id)
        {
            var filter = Builders<ExamMatricesModel>.Filter.Eq(m => m.Id, id);

            return await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<ExamMatricesModel>> SearchByName(string matrixName)
        {
            var filter = Builders<ExamMatricesModel>.Filter.Regex("MatrixName", new BsonRegularExpression(matrixName, "i"));
            return await this._examMatrixsCollection.Find(filter).ToListAsync();
        }

        public async Task<string> AddExamMatrix(ExamMatrixDto examMatrixData)
        {
            // Add log information
            // var addLog = new MatrixLogsModel
            // {
            //    MatrixLogUserId = matrixLogUserId,
            //    MatrixLogType = "Create an exam matrix",
            //    MatrixChangeAt = DateTime.Now,
            // };

            // if (examMatrixData.MatrixLogs == null)
            // {
            //    examMatrixData.MatrixLogs = new List<MatrixLogsModel>();
            // }

            //// Add log to exam matrix data
            // examMatrixData.MatrixLogs.Add(addLog);

            // Create Id
            var newExamMatrix = new ExamMatricesModel
            {
                Id = ObjectId.GenerateNewId().ToString(),
                MatrixName = examMatrixData.MatrixName,
                SubjectId = examMatrixData.SubjectId,
                QuestionBankId = examMatrixData.QuestionBankId,
                MatrixStatus = "Active",
                MatrixTags = new List<MatrixTagsModel>(),
                ExamId = new List<string>(),
                TotalGeneratedExams = 0,
            };

            try
            {
                await this._examMatrixsCollection.InsertOneAsync(newExamMatrix);
                return "Exam matrix created successfully";
            }
            catch (Exception ex)
            {
                return $"Failed: {ex.Message}";
            }
        }

        public async Task<string> AddTag(ExamMatrixAddDto tagsData)
        {
            if (tagsData == null || string.IsNullOrWhiteSpace(tagsData.ExamMatrixId) || tagsData.Tags == null || !tagsData.Tags.Any())
            {
                return "Invalid data";
            }

            var filter = Builders<ExamMatricesModel>.Filter.Eq(e => e.Id, tagsData.ExamMatrixId);
            var examMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();

            if (examMatrix == null)
            {
                return "Exam Matrix not found!";
            }

            if (examMatrix.MatrixTags == null)
            {
                examMatrix.MatrixTags = new List<MatrixTagsModel>();
            }

            var existingTags = examMatrix.MatrixTags.Select(tag => tag.TagName.Trim().ToLower()).ToList();

            var newTags = tagsData.Tags
            .Select(tag => new MatrixTagsModel { TagName = tag.TagName.Trim() })
            .Where(tag => !existingTags.Contains(tag.TagName.ToLower()))
            .ToList();

            if (!newTags.Any())
            {
                return "All tags already exist!";
            }

            var update = Builders<ExamMatricesModel>.Update.PushEach(e => e.MatrixTags, newTags);
            var result = await this._examMatrixsCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0 ? "Tags added successfully" : "Failed to add tags";
        }

        public async Task<string> UpdateExamMatrix(string examMatrixId, ExamMatrixUpdateDto examMatrixData)
        {
            if (examMatrixData == null)
            {
                return "Invalid data";
            }

            // var updateLog = new MatrixLogsModel
            // {
            //    MatrixLogUserId = examMatrixData.MatrixLogUserId,
            //    MatrixLogType = "Update exam matrix",
            //    MatrixChangeAt = DateTime.UtcNow,
            // };
            var filter = Builders<ExamMatricesModel>.Filter.Eq(x => x.Id, examMatrixId);

            var existingExamMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();

            if (existingExamMatrix == null)
            {
                return "Exam matrix not found";
            }

            // Update data
            var update = Builders<ExamMatricesModel>.Update
                .Set(x => x.MatrixName, examMatrixData.MatrixName)
                .Set(x => x.QuestionBankId, examMatrixData.QuestionBankId)
                .Set(x => x.MatrixTags, examMatrixData.MatrixTags)
                .Set(x => x.MatrixStatus, examMatrixData.MatrixStatus)
                .Set(x => x.TotalGeneratedExams, examMatrixData.TotalGenerateExam)
                .Set(x => x.SubjectId, examMatrixData.SubjectId)
                .Set(x => x.ExamId, examMatrixData.ExamId);

            var result = await this._examMatrixsCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0 ? "Exam matrix updated successfully" : "No changes were made";
        }

        public async Task<string> UpdateTag(string examMatrixId, string tagName, int questionCount, double tagScore)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return "Invalid data";
            }

            var filter = Builders<ExamMatricesModel>.Filter.Eq(x => x.Id, examMatrixId);
            var existingExamMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();

            if (existingExamMatrix == null)
            {
                return "Exam matrix not found";
            }

            var tagFilter = Builders<ExamMatricesModel>.Filter.And(
                Builders<ExamMatricesModel>.Filter.Eq(x => x.Id, examMatrixId),
                Builders<ExamMatricesModel>.Filter.ElemMatch(x => x.MatrixTags, tag => tag.TagName == tagName)
            );

            var update = Builders<ExamMatricesModel>.Update
                .Set("MatrixTags.$.QuestionCount", questionCount)
                .Set("MatrixTags.$.TagScore", tagScore);

            var result = await this._examMatrixsCollection.UpdateOneAsync(tagFilter, update);

            return result.ModifiedCount > 0 ? "Tags updated successfully" : "Tag not found or no changes were made";
        }

        public async Task<string> DeleteExamMatrix(string examMatrixId, string matrixLogUserId)
        {
            if (string.IsNullOrEmpty(examMatrixId))
            {
                return "Invalid ExamMatrixId";
            }

            var filter = Builders<ExamMatricesModel>.Filter.Eq(x => x.Id, examMatrixId);
            var existingExamMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();

            if (existingExamMatrix == null)
            {
                return "Exam matrix not found";
            }

            var update = Builders<ExamMatricesModel>.Update
                .Set(x => x.MatrixStatus, "Disable");

            var result = await this._examMatrixsCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0 ? "Exam matrix marked as unavailable" : "No changes were made";
        }

        // Can use update tags to upadte list tag again
        public async Task<string> DeleteTag(string examMatrixId, string tagName, string matrixLogUserId)
        {
            if (string.IsNullOrEmpty(examMatrixId) || string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(matrixLogUserId))
            {
                return "Invalid input data";
            }

            var filter = Builders<ExamMatricesModel>.Filter.Eq(x => x.Id, examMatrixId);
            var existingExamMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();

            if (existingExamMatrix == null)
            {
                return "Exam matrix not found";
            }

            bool tagExists = existingExamMatrix.MatrixTags.Any(t => t.TagName == tagName);
            if (!tagExists)
            {
                return $"Tag '{tagName}' not found";
            }

            // Delete record having tagname = TagName
            var update = Builders<ExamMatricesModel>.Update
                .PullFilter(x => x.MatrixTags, Builders<MatrixTagsModel>.Filter.Eq(t => t.TagName, tagName));

            var result = await this._examMatrixsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                return "Tag deleted successfully";
            }

            return "Tag not found or already removed";
        }

        public async Task SeedData()
        {
            var sampleData = new List<ExamMatricesModel>
            {
                new ExamMatricesModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    MatrixName = "Matrix math exam",
                    MatrixStatus = "Active",
                    TotalGeneratedExams = 1,
                    SubjectId = "MATH01",
                    ExamId = new List<string> { "67b7e27f152621e5bcd6c232" },
                    QuestionBankId = "67ce3d5ac07467bf499bfdfe",
                    MatrixTags = new List<MatrixTagsModel>
                    {
                        new MatrixTagsModel { TagName = "Dễ", QuestionCount = 10, TagScore = 1 },
                        new MatrixTagsModel { TagName = "Khó", QuestionCount = 5, TagScore = 3 },
                    },
                },
                new ExamMatricesModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    MatrixName = "Matrix history exam",
                    MatrixStatus = "Inactive",
                    TotalGeneratedExams = 3,
                    SubjectId = "HIS001",
                    ExamId = new List<string> { "67b7e27f152621e5bcd6c232" },
                    QuestionBankId = "67ce3d5ac07467bf499bfdfe",
                    MatrixTags = new List<MatrixTagsModel>
                    {
                        new MatrixTagsModel { TagName = "Trung bình", QuestionCount = 8, TagScore = 2 },
                        new MatrixTagsModel { TagName = "Rất khó", QuestionCount = 2, TagScore = 5 },
                    },
                },
                new ExamMatricesModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    MatrixName = "Matrix chemistry exam",
                    MatrixStatus = "Active",
                    TotalGeneratedExams = 7,
                    SubjectId = "GEO001",
                    ExamId = new List<string> { "67b7e27f152621e5bcd6c232" },
                    QuestionBankId = "67ce3d5ac07467bf499bfdfe",
                    MatrixTags = new List<MatrixTagsModel>
                    {
                        new MatrixTagsModel { TagName = "Cơ bản", QuestionCount = 12, TagScore = 1 },
                        new MatrixTagsModel { TagName = "Vận dụng cao", QuestionCount = 3, TagScore = 4 },
                    },
                },
            };

            await this._examMatrixsCollection.InsertManyAsync(sampleData);
        }
    }
}
