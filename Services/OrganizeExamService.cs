using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Services;

public class OrganizeExamService
{
    private readonly IMongoCollection<OrganizeExamModel> _organizeExamCollection;
    private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
    // private readonly IMongoCollection<ExamMatricesModel> _examMatricesCollection;

    public OrganizeExamService(IMongoDatabase database)
    {
        _organizeExamCollection = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
    }
    
    public async Task<(List<OrganizeExamDto>, long)> GetOrganizeExams(string? keyword, int page, int pageSize)
    {
        // Tạo bộ lọc để loại bỏ các kỳ thi bị xóa
        var filters = new List<FilterDefinition<OrganizeExamModel>>
        {
            Builders<OrganizeExamModel>.Filter.Ne(ex => ex.OrganizeExamStatus, "deleted")
        };

        // Nếu có keyword, thêm bộ lọc tìm kiếm theo tên
        if (!string.IsNullOrEmpty(keyword))
        {
            var keywordFilter = Builders<OrganizeExamModel>.Filter.Regex(ex => ex.OrganizeExamName, new BsonRegularExpression(keyword, "i"));
            filters.Add(keywordFilter);
        }

        var finalFilter = Builders<OrganizeExamModel>.Filter.And(filters);

        // Lấy danh sách kỳ thi tổ chức
        var organizeExamsTask = _organizeExamCollection
            .Find(finalFilter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .Project(o => new  // Projection để giảm dữ liệu tải về
            {
                o.Id,
                o.OrganizeExamName,
                o.Duration,
                o.TotalQuestions,
                o.MaxScore,
                o.SubjectId,
                o.ExamType,
                o.MatrixId,
                o.Exams,
                o.OrganizeExamStatus
            })
            .ToListAsync();

        // Đếm tổng số bản ghi phù hợp
        var totalCountTask = _organizeExamCollection.CountDocumentsAsync(finalFilter);

        // Chạy song song cả hai truy vấn
        await Task.WhenAll(organizeExamsTask, totalCountTask);
        var organizeExams = organizeExamsTask.Result;
        var totalCount = totalCountTask.Result;

        // Lấy danh sách Subject trước để tránh query từng cái một
        var subjectIds = organizeExams.Select(o => o.SubjectId).Distinct().ToList();
        var matrixIds = organizeExams.Where(o => !string.IsNullOrEmpty(o.MatrixId)).Select(o => o.MatrixId!).Distinct().ToList();
        
        var allSubjectIds = subjectIds.Concat(matrixIds).Distinct().ToList();
        var subjects = await _subjectsCollection.Find(s => allSubjectIds.Contains(s.Id))
                                                .ToListAsync();
        
        var subjectDict = subjects.ToDictionary(s => s.Id, s => s.SubjectName);

        // Ánh xạ dữ liệu sang DTO
        var examResponseList = organizeExams.Select(organizeExam => new OrganizeExamDto
        {
            Id = organizeExam.Id,
            OrganizeExamName = organizeExam.OrganizeExamName,
            Duration = organizeExam.Duration,
            TotalQuestions = organizeExam.TotalQuestions,
            MaxScore = organizeExam.MaxScore,
            SubjectId = organizeExam.SubjectId,
            SubjectName = subjectDict.GetValueOrDefault(organizeExam.SubjectId, string.Empty) ?? string.Empty,
            ExamType = organizeExam.ExamType,
            MatrixId = organizeExam.MatrixId,
            MatrixName = subjectDict.GetValueOrDefault(organizeExam.MatrixId ?? string.Empty, string.Empty),
            Exams = organizeExam.Exams?.Select(exam => new ExamInOrganizeExamDto
            {
                Id = exam.Id,
                ExamCode = exam.ExamCode,
                ExamName = exam.ExamName
            }).ToList(),
            OrganizeExamStatus = organizeExam.OrganizeExamStatus,
        }).ToList();

        return (examResponseList, totalCount);
    }
    
    // public async Task<(List<OrganizeExamDto>, long)> GetOrganizeExams(string? keyword, int page, int pageSize)
    // {
    //     var pipeline = new List<BsonDocument>();
    //
    //     // Join với Subjects để lấy SubjectName
    //     pipeline.Add(new BsonDocument("$lookup", new BsonDocument
    //     {
    //         { "from", "Subjects" },
    //         { "localField", "subjectId" },
    //         { "foreignField", "_id" },
    //         { "as", "subjectInfo" }
    //     }));
    //
    //     // Unwind mảng subjectInfo (nếu có)
    //     pipeline.Add(new BsonDocument("$unwind", new BsonDocument
    //     {
    //         { "path", "$subjectInfo" },
    //         { "preserveNullAndEmptyArrays", true }
    //     }));
    //
    //     // Lọc bỏ OrganizeExamStatus = "deleted"
    //     var filterConditions = new List<BsonDocument>
    //     {
    //         new BsonDocument("organizeExamStatus", new BsonDocument("$ne", "deleted"))
    //     };
    //
    //     // Nếu có keyword, tìm theo OrganizeExamName hoặc SubjectName
    //     if (!string.IsNullOrEmpty(keyword))
    //     {
    //         var regex = new BsonDocument("$regex", keyword);
    //         regex.Add("$options", "i");
    //
    //         filterConditions.Add(new BsonDocument("$or", new BsonArray
    //         {
    //             new BsonDocument("organizeExamName", regex),
    //             new BsonDocument("subjectInfo.subjectName", regex)
    //         }));
    //     }
    //
    //     pipeline.Add(new BsonDocument("$match", new BsonDocument("$and", new BsonArray(filterConditions))));
    //
    //     // Tính tổng số kết quả phù hợp
    //     var countStage = new BsonDocument("$count", "totalCount");
    //
    //     // Lấy số lượng bản ghi phù hợp
    //     var countResult = await _organizeExamCollection.Aggregate<BsonDocument>(new[] { pipeline.First(), countStage }).FirstOrDefaultAsync();
    //     var totalCount = countResult?["totalCount"]?.AsInt64 ?? 0;
    //
    //     // Phân trang
    //     pipeline.Add(new BsonDocument("$skip", (page - 1) * pageSize));
    //     pipeline.Add(new BsonDocument("$limit", pageSize));
    //
    //     // Chạy pipeline
    //     var organizeExams = await _organizeExamCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();
    //
    //     // Chuyển đổi kết quả sang DTO
    //     var examResponseList = organizeExams.Select(o => new OrganizeExamDto
    //     {
    //         Id = o["_id"].AsString,
    //         OrganizeExamName = o["organizeExamName"].AsString,
    //         Duration = o.GetValue("duration", 0).AsInt32,
    //         TotalQuestions = o.GetValue("totalQuestions", BsonNull.Value)?.AsNullableInt32,
    //         MaxScore = o.GetValue("maxScore", 0).AsInt32,
    //         SubjectId = o["subjectId"].AsString,
    //         SubjectName = o.GetValue("subjectInfo", new BsonDocument()).GetValue("subjectName", "").AsString,
    //         ExamType = o["examType"].AsString,
    //         MatrixId = o.GetValue("matrixId", BsonNull.Value)?.AsString,
    //         MatrixName = string.Empty, // Có thể mở rộng lookup cho MatrixId nếu cần
    //         Exams = o.GetValue("examSet", new BsonArray()).Select(exam => new ExamInOrganizeExamDto
    //         {
    //             Id = exam["_id"].AsString,
    //             ExamCode = exam["examCode"].AsString,
    //             ExamName = exam["examName"].AsString
    //         }).ToList(),
    //         OrganizeExamStatus = o["organizeExamStatus"].AsString
    //     }).ToList();
    //
    //     return (examResponseList, totalCount);
    // }

}