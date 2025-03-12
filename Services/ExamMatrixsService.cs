#pragma warning disable SA1309
namespace Backend_online_testing.Services
{
    using Backend_online_testing.Dtos;
    using Backend_online_testing.Models;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Microsoft.Extensions.Logging.Abstractions;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class ExamMatrixsService
    {
        private readonly IMongoCollection<ExamMatrixsModel> _examMatrixsCollection;

        public ExamMatrixsService(IMongoDatabase database)
        {
            this._examMatrixsCollection = database.GetCollection<ExamMatrixsModel>("ExamMatrixs");
        }

        public async Task<(List<ExamMatrixsModel>, long)> GetAllExamMatrix(string? keyword, int page, int pageSize)
        {
            var filter = Builders<ExamMatrixsModel>.Filter.Empty;

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = Builders<ExamMatrixsModel>.Filter.Or(
                    Builders<ExamMatrixsModel>.Filter.Regex(ex => ex.MatrixName, new BsonRegularExpression(keyword, "i")),
                    Builders<ExamMatrixsModel>.Filter.Regex(ex => ex.MatrixStatus, new BsonRegularExpression(keyword, "i")));
            }

            var examMatrixs = await this._examMatrixsCollection
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var totalRecords = await this._examMatrixsCollection.CountDocumentsAsync(filter);

            return (examMatrixs, totalRecords);
        }

        public async Task<ExamMatrixsModel> GetByIdExamMatrix(string id)
        {
            var filter = Builders<ExamMatrixsModel>.Filter.Eq(m => m.Id, id);

            return await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<ExamMatrixsModel>> SearchByName(string matrixName)
        {
            var filter = Builders<ExamMatrixsModel>.Filter.Regex("MatrixName", new BsonRegularExpression(matrixName, "i"));
            return await this._examMatrixsCollection.Find(filter).ToListAsync();
        }

        public async Task<string> AddExamMatrix(ExamMatrixsModel examMatrixData, string matrixLogUserId)
        {
            if (examMatrixData == null)
            {
                return "Failure: Invalid exam matrix data";
            }

            // Add log information
            var addLog = new MatrixLogsModel
            {
                MatrixLogUserId = matrixLogUserId,
                MatrixLogType = "Create an exam matrix",
                MatrixChangeAt = DateTime.Now,
            };

            // if (examMatrixData.MatrixLogs == null)
            // {
            //    examMatrixData.MatrixLogs = new List<MatrixLogsModel>();
            // }

            //// Add log to exam matrix data
            // examMatrixData.MatrixLogs.Add(addLog);

            // Create Id
            examMatrixData.Id = ObjectId.GenerateNewId().ToString();

            try
            {
                await this._examMatrixsCollection.InsertOneAsync(examMatrixData);
                return "Exam matrix created successfully";
            }
            catch (Exception ex)
            {
                return $"Failed: {ex.Message}";
            }
        }

        public async Task<string> AddTag(ExamMatrixAddDto examMatrixData)
        {
            var filter = Builders<ExamMatrixsModel>.Filter.Eq(e => e.Id, examMatrixData.ExamMatrixId);

            if (examMatrixData == null || string.IsNullOrWhiteSpace(examMatrixData.ExamMatrixId) || examMatrixData.Tags == null || !examMatrixData.Tags.Any())
            {
                return "Invalid data";
            }

            var update = Builders<ExamMatrixsModel>.Update
                .PushEach(e => e.MatrixTags, examMatrixData.Tags);

            var result = await this._examMatrixsCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0 ? "Tags added successfully" : "Failed to add tags. Exam matrix not found.";
        }

        public async Task<string> UpdateExamMatrix(string examMatrixId, ExamMatrixUpdateDto examMatrixData)
        {
            if (examMatrixData == null)
            {
                return "Invalid data";
            }

            var updateLog = new MatrixLogsModel
            {
                MatrixLogUserId = examMatrixData.MatrixLogUserId,
                MatrixLogType = "Update exam matrix",
                MatrixChangeAt = DateTime.UtcNow,
            };

            var filter = Builders<ExamMatrixsModel>.Filter.Eq(x => x.Id, examMatrixData.ExamMatrixId);

            var existingExamMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();

            if (existingExamMatrix == null)
            {
                return "Exam matrix not found";
            }

            // Update data
            var update = Builders<ExamMatrixsModel>.Update
                .Set(x => x.MatrixName, examMatrixData.ExamMatrixName)
                .Set(x => x.MatrixStatus, examMatrixData.ExamMatrixStatus)
                .Set(x => x.TotalGeneratedExams, examMatrixData.TotalGenerateExam)
                .Set(x => x.SubjectId, examMatrixData.SubjectId)
                .Set(x => x.ExamId, examMatrixData.ExamId);

            var result = await this._examMatrixsCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0 ? "Exam matrix updated successfully" : "No changes were made";
        }

        public async Task<string> UpdateTag(string examMatricId, ExamMatrixUpdateDto tagsData)
        {
            if (tagsData == null)
            {
                return "Invalid data";
            }

            var updateLog = new MatrixLogsModel
            {
                MatrixLogUserId = tagsData.MatrixLogUserId,
                MatrixLogType = "Update tags matrix",
                MatrixChangeAt = DateTime.Now,
            };

            var filter = Builders<ExamMatrixsModel>.Filter.Eq(x => x.Id, tagsData.ExamMatrixId);

            var existingExamMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();

            if (existingExamMatrix == null)
            {
                return "Exam matrix not found";
            }

            var update = Builders<ExamMatrixsModel>.Update
                .Set(x => x.MatrixTags, tagsData.Tags);

            var result = await this._examMatrixsCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0 ? "Tags updated successfully" : "No changes were made";
        }

        public async Task<string> DeleteExamMatrix(string examMatrixId, string matrixLogUserId)
        {
            if (string.IsNullOrEmpty(examMatrixId))
            {
                return "Invalid ExamMatrixId";
            }

            var filter = Builders<ExamMatrixsModel>.Filter.Eq(x => x.Id, examMatrixId);
            var existingExamMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();

            if (existingExamMatrix == null)
            {
                return "Exam matrix not found";
            }

            var update = Builders<ExamMatrixsModel>.Update
                .Set(x => x.MatrixStatus, "Unavailable");

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

            var filter = Builders<ExamMatrixsModel>.Filter.Eq(x => x.Id, examMatrixId);
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
            var update = Builders<ExamMatrixsModel>.Update
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
            var sampleData = new List<ExamMatrixsModel>
            {
                new ExamMatrixsModel
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
                new ExamMatrixsModel
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
                new ExamMatrixsModel
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
