using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Services
{
    public interface IExamMatricesService
    {
        Task<(List<ExamMatrixDto>, long)> GetExamMatrices(string? keyword, int page, int pageSize);
        Task<ExamMatricesModel> GetByIdExamMatrix(string id);
        Task<string> AddExamMatrix(ExamMatrixRequestDto matrixData);
        Task<(string?, string?)> UpdateExamMatrix(string examMatrixId, ExamMatrixUpdateDto examMatrixData);
        Task<string> DeleteExamMatrix(string examMatrixId, string matrixLogUserId);
        Task<(string, List<MatrixOptionsDto>)> GetMatrixOptions(string subjectId, string? questionBankId);
        Task<(string status, string? matrixName, List<GenerateExamByMatrixResponseDto>?)> GenerateExamAsync(GenerateExamByMatrixRequestDto request);

    }
    
    public class ExamMatricesService : IExamMatricesService
    {
        private readonly IMongoCollection<ExamMatricesModel> _examMatrixsCollection;
        private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
        private readonly IExamMatricesRepository _repo;

        public ExamMatricesService(IMongoDatabase database, IExamMatricesRepository repo)
        {
            _examMatrixsCollection = database.GetCollection<ExamMatricesModel>("examMatrices");
            _subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
            _repo = repo;
        }

        public async Task<(List<ExamMatrixDto>, long)> GetExamMatrices(string? keyword, int page, int pageSize)
        {
            var filter = Builders<ExamMatricesModel>.Filter.Ne(em => em.MatrixStatus, "deleted");

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = Builders<ExamMatricesModel>.Filter.Or(
                    Builders<ExamMatricesModel>.Filter.Regex(ex => ex.MatrixName, new BsonRegularExpression(keyword, "i")),
                    Builders<ExamMatricesModel>.Filter.Regex(ex => ex.MatrixStatus, new BsonRegularExpression(keyword, "i")));
            }
            
            var matricesResponse =  new List<ExamMatrixDto>();
            
            var examMatrices = await _examMatrixsCollection
                .Find(filter)
                .SortByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            foreach (var matrix in examMatrices)
            {
                var subject = await _subjectsCollection.Find(subject => subject.Id == matrix.SubjectId).FirstOrDefaultAsync();
                var questionBankName = subject.QuestionBanks.Find(qb => qb.QuestionBankId == matrix.QuestionBankId)?.QuestionBankName;

                matricesResponse.Add(new ExamMatrixDto
                {
                    Id = matrix.Id,
                    MatrixName = matrix.MatrixName,
                    SubjectId = matrix.SubjectId,
                    SubjectName = subject.SubjectName,
                    QuestionBankId = matrix.QuestionBankId ?? string.Empty,
                    QuestionBankName = questionBankName ?? string.Empty,
                    MatrixTags = matrix.MatrixTags,
                    TotalGeneratedExams = matrix.ExamIds.Count,
                    ExamIds = matrix.ExamIds,
                    MatrixType = matrix.MatrixType,
                });
            }

            var totalCount = await _examMatrixsCollection.CountDocumentsAsync(filter);

            return (matricesResponse, totalCount);
        }

        public async Task<ExamMatricesModel> GetByIdExamMatrix(string id)
        {
            var filter = Builders<ExamMatricesModel>.Filter.Eq(m => m.Id, id);

            return await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<string> AddExamMatrix(ExamMatrixRequestDto matrixData)
        {
            var newExamMatrix = new ExamMatricesModel
            {
                MatrixName = matrixData.MatrixName,
                SubjectId = matrixData.SubjectId,
                QuestionBankId = matrixData.QuestionBankId,
                MatrixTags = matrixData.MatrixTags,
                MatrixType = matrixData.MatrixType
            };

            try
            {
                await _examMatrixsCollection.InsertOneAsync(newExamMatrix);
                return "Tạo ma trận thành công";
            }
            catch (Exception ex)
            {
                return $"Failed: {ex.Message}";
            }
        }

        // public async Task<string> AddTag(ExamMatrixAddDto tagsData)
        // {
        //     if (tagsData == null || string.IsNullOrWhiteSpace(tagsData.ExamMatrixId) || tagsData.Tags == null || !tagsData.Tags.Any())
        //     {
        //         return "Invalid data";
        //     }
        //
        //     var filter = Builders<ExamMatricesModel>.Filter.Eq(e => e.Id, tagsData.ExamMatrixId);
        //     var examMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();
        //
        //     if (examMatrix == null)
        //     {
        //         return "Exam Matrix not found!";
        //     }
        //
        //     if (examMatrix.MatrixTags == null)
        //     {
        //         examMatrix.MatrixTags = new List<MatrixTagsModel>();
        //     }
        //
        //     var existingTags = examMatrix.MatrixTags.Select(tag => tag.TagName.Trim().ToLower()).ToList();
        //
        //     var newTags = tagsData.Tags
        //     .Select(tag => new MatrixTagsModel { TagName = tag.TagName.Trim() })
        //     .Where(tag => !existingTags.Contains(tag.TagName.ToLower()))
        //     .ToList();
        //
        //     if (!newTags.Any())
        //     {
        //         return "All tags already exist!";
        //     }
        //
        //     var update = Builders<ExamMatricesModel>.Update.PushEach(e => e.MatrixTags, newTags);
        //     var result = await this._examMatrixsCollection.UpdateOneAsync(filter, update);
        //
        //     return result.ModifiedCount > 0 ? "Tags added successfully" : "Failed to add tags";
        // }

        public async Task<(string?, string?)> UpdateExamMatrix(string examMatrixId, ExamMatrixUpdateDto examMatrixData)
        {
            if (examMatrixData == null)
            {
                return (null, "Invalid data");
            }

            var filter = Builders<ExamMatricesModel>.Filter.Eq(x => x.Id, examMatrixId);

            var existingExamMatrix = await _examMatrixsCollection.Find(filter).FirstOrDefaultAsync();

            if (existingExamMatrix == null)
            {
                return (null, "Exam matrix not found");
            }

            var updates = new List<UpdateDefinition<ExamMatricesModel>>();

            if (!string.IsNullOrEmpty(examMatrixData.MatrixName))
                updates.Add(Builders<ExamMatricesModel>.Update.Set(x => x.MatrixName, examMatrixData.MatrixName));

            if (!string.IsNullOrEmpty(examMatrixData.MatrixStatus))
                updates.Add(Builders<ExamMatricesModel>.Update.Set(x => x.MatrixStatus, examMatrixData.MatrixStatus));
            
            if (!string.IsNullOrEmpty(examMatrixData.SubjectId))
                updates.Add(Builders<ExamMatricesModel>.Update.Set(x => x.SubjectId, examMatrixData.SubjectId));
            
            if (examMatrixData.MatrixTags != null && examMatrixData.MatrixTags.Any())
                updates.Add(Builders<ExamMatricesModel>.Update.Set(x => x.MatrixTags, examMatrixData.MatrixTags));

            if (!string.IsNullOrEmpty(examMatrixData.QuestionBankId))
                updates.Add(Builders<ExamMatricesModel>.Update.Set(x => x.QuestionBankId, examMatrixData.QuestionBankId));

            if (!updates.Any())
                return (null, "No valid data provided for update");

            var updateDefinition = Builders<ExamMatricesModel>.Update.Combine(updates);

            var result = await _examMatrixsCollection.UpdateOneAsync(filter, updateDefinition);

            return result.ModifiedCount > 0 ? (existingExamMatrix.MatrixName, "Exam matrix updated successfully") : (existingExamMatrix.MatrixName, "No changes were made");
        }

        // public async Task<string> UpdateTag(string examMatrixId, string tagName, int questionCount, double tagScore)
        // {
        //     if (string.IsNullOrWhiteSpace(tagName))
        //     {
        //         return "Invalid data";
        //     }
        //
        //     var filter = Builders<ExamMatricesModel>.Filter.Eq(x => x.Id, examMatrixId);
        //     var existingExamMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();
        //
        //     if (existingExamMatrix == null)
        //     {
        //         return "Exam matrix not found";
        //     }
        //
        //     var tagFilter = Builders<ExamMatricesModel>.Filter.And(
        //         Builders<ExamMatricesModel>.Filter.Eq(x => x.Id, examMatrixId),
        //         Builders<ExamMatricesModel>.Filter.ElemMatch(x => x.MatrixTags, tag => tag.TagName == tagName)
        //     );
        //
        //     var update = Builders<ExamMatricesModel>.Update
        //         .Set("MatrixTags.$.QuestionCount", questionCount)
        //         .Set("MatrixTags.$.TagScore", tagScore);
        //
        //     var result = await this._examMatrixsCollection.UpdateOneAsync(tagFilter, update);
        //
        //     return result.ModifiedCount > 0 ? "Tags updated successfully" : "Tag not found or no changes were made";
        // }

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

        // // Can use update tags to upadte list tag again
        // public async Task<string> DeleteTag(string examMatrixId, string tagName, string matrixLogUserId)
        // {
        //     if (string.IsNullOrEmpty(examMatrixId) || string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(matrixLogUserId))
        //     {
        //         return "Invalid input data";
        //     }
        //
        //     var filter = Builders<ExamMatricesModel>.Filter.Eq(x => x.Id, examMatrixId);
        //     var existingExamMatrix = await this._examMatrixsCollection.Find(filter).FirstOrDefaultAsync();
        //
        //     if (existingExamMatrix == null)
        //     {
        //         return "Exam matrix not found";
        //     }
        //
        //     bool tagExists = existingExamMatrix.MatrixTags.Any(t => t.TagName == tagName);
        //     if (!tagExists)
        //     {
        //         return $"Tag '{tagName}' not found";
        //     }
        //
        //     // Delete record having tagname = TagName
        //     var update = Builders<ExamMatricesModel>.Update
        //         .PullFilter(x => x.MatrixTags, Builders<MatrixTagsModel>.Filter.Eq(t => t.TagName, tagName));
        //
        //     var result = await this._examMatrixsCollection.UpdateOneAsync(filter, update);
        //
        //     if (result.ModifiedCount > 0)
        //     {
        //         return "Tag deleted successfully";
        //     }
        //
        //     return "Tag not found or already removed";
        // }

        // public async Task SeedData()
        // {
        //     var sampleData = new List<ExamMatricesModel>
        //     {
        //         new ExamMatricesModel
        //         {
        //             Id = ObjectId.GenerateNewId().ToString(),
        //             MatrixName = "Matrix math exam",
        //             MatrixStatus = "Active",
        //             TotalGeneratedExams = 1,
        //             SubjectId = "MATH01",
        //             ExamId = new List<string> { "67b7e27f152621e5bcd6c232" },
        //             QuestionBankId = "67ce3d5ac07467bf499bfdfe",
        //             MatrixTags = new List<MatrixTagsModel>
        //             {
        //                 new MatrixTagsModel { TagName = "Dễ", QuestionCount = 10, TagScore = 1 },
        //                 new MatrixTagsModel { TagName = "Khó", QuestionCount = 5, TagScore = 3 },
        //             },
        //         },
        //         new ExamMatricesModel
        //         {
        //             Id = ObjectId.GenerateNewId().ToString(),
        //             MatrixName = "Matrix history exam",
        //             MatrixStatus = "Inactive",
        //             TotalGeneratedExams = 3,
        //             SubjectId = "HIS001",
        //             ExamId = new List<string> { "67b7e27f152621e5bcd6c232" },
        //             QuestionBankId = "67ce3d5ac07467bf499bfdfe",
        //             MatrixTags = new List<MatrixTagsModel>
        //             {
        //                 new MatrixTagsModel { TagName = "Trung bình", QuestionCount = 8, TagScore = 2 },
        //                 new MatrixTagsModel { TagName = "Rất khó", QuestionCount = 2, TagScore = 5 },
        //             },
        //         },
        //         new ExamMatricesModel
        //         {
        //             Id = ObjectId.GenerateNewId().ToString(),
        //             MatrixName = "Matrix chemistry exam",
        //             MatrixStatus = "Active",
        //             TotalGeneratedExams = 7,
        //             SubjectId = "GEO001",
        //             ExamId = new List<string> { "67b7e27f152621e5bcd6c232" },
        //             QuestionBankId = "67ce3d5ac07467bf499bfdfe",
        //             MatrixTags = new List<MatrixTagsModel>
        //             {
        //                 new MatrixTagsModel { TagName = "Cơ bản", QuestionCount = 12, TagScore = 1 },
        //                 new MatrixTagsModel { TagName = "Vận dụng cao", QuestionCount = 3, TagScore = 4 },
        //             },
        //         },
        //     };
        //
        //     await this._examMatrixsCollection.InsertManyAsync(sampleData);
        // }

        public async Task<(string, List<MatrixOptionsDto>)> GetMatrixOptions(string subjectId, string? questionBankId)
        {
            var builder = Builders<ExamMatricesModel>.Filter;
            var filter = builder.Ne(x => x.MatrixStatus, "deleted");

            if (!string.IsNullOrEmpty(subjectId))
            {
                filter &= builder.Eq(x => x.SubjectId, subjectId);
            }

            if (!string.IsNullOrEmpty(questionBankId))
            {
                filter &= builder.Eq(x => x.QuestionBankId, questionBankId);
            }

            var result = await _examMatrixsCollection
                .Find(filter)
                .SortByDescending(x => x.Id)
                .Project(mt => new MatrixOptionsDto
                {
                    Id = mt.Id,
                    MatrixName = mt.MatrixName,
                    SubjectId = mt.SubjectId
                })
                .ToListAsync();

            return ("ok", result);
        }
        
        public async Task<(string status, string? matrixName, List<GenerateExamByMatrixResponseDto>?)> GenerateExamAsync(GenerateExamByMatrixRequestDto request)
        {
            var examMatrix = await _repo.GetExamMatrixByIdAsync(request.ExamMatrixId);
            if (examMatrix == null)
                return ("exam-matrix-not-found", null, null);

            var subject = await _repo.GetSubjectByIdAsync(examMatrix.SubjectId);
            if (subject == null)
                return ("subject-not-found", null, null);

            var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == examMatrix.QuestionBankId);
            if (questionBank == null)
                return ("question-bank-not-found", null, null);

            var generatedExams = new List<GenerateExamByMatrixResponseDto>();

            for (int i = 0; i < request.NumberGenerate; i++)
            {
                var exam = new ExamsModel
                {
                    ExamCode = $"EXAM-{DateTime.UtcNow.Ticks}-{i+1}",
                    ExamName = $"Exam {i+1} for {examMatrix.MatrixName}",
                    SubjectId = examMatrix.SubjectId,
                    QuestionBankId = examMatrix.QuestionBankId,
                    MatrixId = examMatrix.Id,
                    ExamStatus = "available",
                    QuestionSet = []
                };

                if (examMatrix.MatrixType == "both")
                {
                    foreach (var tag in examMatrix.MatrixTags)
                    {
                        var pool = questionBank.QuestionList
                            .Where(q => q.Tags != null
                                        && q.Tags.Count >= 2
                                        && q.Tags[0] == tag.Chapter
                                        && q.Tags[1] == tag.Level)
                            .ToList();

                        if (pool.Count < tag.QuestionCount)
                            return ($"not-enough-questions-both-{tag.Chapter}-{tag.Level}", null, null);

                        var questions = pool
                            .OrderBy(_ => Guid.NewGuid())
                            .Take(tag.QuestionCount)
                            .ToList();

                        foreach (var q in questions)
                        {
                            exam.QuestionSet.Add(new QuestionSetsModel
                            {
                                QuestionId = q.QuestionId,
                                QuestionScore = tag.Score / tag.QuestionCount,
                            });
                        }
                    }
                }
                else if (examMatrix.MatrixType == "chapter")
                {
                    foreach (var tag in examMatrix.MatrixTags)
                    {
                        var pool = questionBank.QuestionList
                            .Where(q => q.Tags != null
                                        && q.Tags.Count >= 1
                                        && !string.IsNullOrEmpty(q.Tags[0])
                                        && q.Tags[0] == tag.Chapter)
                            .ToList();

                        if (pool.Count < tag.QuestionCount)
                            return ($"not-enough-questions-chapter-{tag.Chapter}", null, null);

                        var questions = pool
                            .OrderBy(_ => Guid.NewGuid())
                            .Take(tag.QuestionCount)
                            .ToList();

                        foreach (var q in questions)
                        {
                            exam.QuestionSet.Add(new QuestionSetsModel
                            {
                                QuestionId = q.QuestionId,
                                QuestionScore = tag.Score / tag.QuestionCount,
                            });
                        }
                    }
                }
                else if (examMatrix.MatrixType == "level")
                {
                    foreach (var tag in examMatrix.MatrixTags)
                    {
                        var pool = questionBank.QuestionList
                            .Where(q => q.Tags != null
                                        && q.Tags.Count >= 2
                                        && !string.IsNullOrEmpty(q.Tags[1])
                                        && q.Tags[1] == tag.Level)
                            .ToList();

                        if (pool.Count < tag.QuestionCount)
                            return ($"not-enough-questions-level-{tag.Level}", null, null);

                        var questions = pool
                            .OrderBy(_ => Guid.NewGuid())
                            .Take(tag.QuestionCount)
                            .ToList();

                        foreach (var q in questions)
                        {
                            exam.QuestionSet.Add(new QuestionSetsModel
                            {
                                QuestionId = q.QuestionId,
                                QuestionScore = tag.Score / tag.QuestionCount,
                            });
                        }
                    }
                }

                await _repo.InsertExamAsync(exam);

                examMatrix.ExamIds.Add(exam.Id);
                await _repo.UpdateExamMatrixAsync(examMatrix);

                generatedExams.Add(new GenerateExamByMatrixResponseDto
                {
                    ExamId = exam.Id,
                    ExamCode = exam.ExamCode,
                    ExamName = exam.ExamName,
                    SubjectId = exam.SubjectId,
                    QuestionBankId = exam.QuestionBankId,
                    QuestionSet = exam.QuestionSet
                });
            }

            return ("success", examMatrix.MatrixName, generatedExams);
        }

        
        
    }
}
