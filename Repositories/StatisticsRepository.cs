using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories;

public class StatisticsRepository
{
    private readonly IMongoCollection<UsersModel> _users;
    private readonly IMongoCollection<OrganizeExamModel> _organizeExam;
    private readonly IMongoCollection<SubjectsModel> _subjects;
    private readonly IMongoCollection<RoomsModel> _rooms;
    private readonly IMongoCollection<ExamsModel> _exams;
    private readonly IMongoCollection<OrganizeExamStatisticModel> _organizeExamStats;
    private readonly IMongoCollection<ParticipationViolationModel> _participationViolations;
    private readonly IMongoCollection<GradeStatisticModel> _gradeStats;

    public StatisticsRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<UsersModel>("users");
        _organizeExam = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjects = database.GetCollection<SubjectsModel>("subjects");
        _rooms = database.GetCollection<RoomsModel>("rooms");
        _exams = database.GetCollection<ExamsModel>("exams");
        _organizeExamStats = database.GetCollection<OrganizeExamStatisticModel>("organizeExamStatistics");
        _participationViolations = database.GetCollection<ParticipationViolationModel>("participationViolations");
        _gradeStats = database.GetCollection<GradeStatisticModel>("gradeStatistics");
    }

    //Get Subject Name
    public async Task<string> GetSubjectNameByIdAsync(string subjectId)
    {
        var subject = await _subjects.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
        return subject.SubjectName;
    }

    //Get question bank name
    public async Task<string?> GetQuestionBankNameAsync(string subjectId, string questionBankId)
    {
        var subject = await _subjects.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
        if (subject == null || subject.QuestionBanks == null) return null;

        var qb = subject.QuestionBanks.FirstOrDefault(q => q.QuestionBankId == questionBankId);
        return qb?.QuestionBankName;
    }

    // Get Room Name
    public async Task<string?> GetRoomNameByIdAsync(string roomId)
    {
        var room = await _rooms.Find(r => r.Id == roomId).FirstOrDefaultAsync();
        return room?.RoomName;
    }

    //Get organize exam by id
    public async Task<OrganizeExamModel> GetOrganizeExamById(string organizeExamId)
    {
        return await _organizeExam.Find(o => o.Id == organizeExamId).FirstOrDefaultAsync();
    }

    //Get user by user id
    public async Task<UsersModel> GetUserByUserId(string userId)
    {
        return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
    }

    //Get user score
    public async Task<double?> GetTotalScoreFromTakeExamAsync(
        string userId, string organizeExamId, string sessionId, string roomId)
    {
        var user = await _users.Find(u => u.Id == userId)
                               .FirstOrDefaultAsync();

        var te = user?.TakeExam?
            .FirstOrDefault(x => x.OrganizeExamId == organizeExamId
                              && x.SessionId == sessionId
                              && x.RoomId == roomId
                              && x.Status == "done"
                          );

        return te?.TotalScore;
    }

    //Get User Status
    public async Task<string?> GetTakeExamStatusAsync(
        string userId, string organizeExamId, string sessionId, string roomId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        var te = user?.TakeExam?
            .FirstOrDefault(x => x.OrganizeExamId == organizeExamId
                              && x.SessionId == sessionId
                              && x.RoomId == roomId);
        return te?.Status;
    }

    //Get list user with list Ids
    public async Task<List<UsersModel>> GetUsersByIdsAsync(IEnumerable<string> userIds)
    {
        var ids = userIds?.Distinct().ToList() ?? new();
        if (ids.Count == 0) return new();
        return await _users.Find(Builders<UsersModel>.Filter.In(u => u.Id, ids)).ToListAsync();
    }

    //Get list exam name -> store in directory
    public async Task<Dictionary<string, string>> GetExamNamesByIdsAsync(IEnumerable<string> examIds)
    {
        var ids = examIds?.Distinct().ToList() ?? new();
        if (ids.Count == 0) return new();

        var filter = Builders<ExamsModel>.Filter.In(e => e.Id, ids);
        var projection = Builders<ExamsModel>.Projection
            .Include(e => e.Id)
            .Include(e => e.ExamName);
        var docs = await _exams.Find(filter).Project<ExamsModel>(projection).ToListAsync();

        return docs.ToDictionary(d => d.Id, d => d.ExamName ?? "");
    }

    public async Task<List<QuestionStatDto>> AggregateQuestionStatsAsync(
    string organizeExamId, string examId,
    IEnumerable<string> sessionIds,
    IEnumerable<string> roomIds,
    IEnumerable<string> candidateIds)
    {
        var sessionArr = new BsonArray(sessionIds ?? Array.Empty<string>());
        var roomArr = new BsonArray(roomIds ?? Array.Empty<string>());
        var candidateArr = new BsonArray(candidateIds ?? Array.Empty<string>());

        if (candidateArr.Count == 0)
            return new List<QuestionStatDto>();

        var pipeline = new List<BsonDocument>
    {
        // So khớp _id (ObjectId) với candidateIds (string) an toàn:
        // _id -> string rồi dùng $in với mảng candidateIds
        new("$match", new BsonDocument("$expr",
            new BsonDocument("$in", new BsonArray
            {
                new BsonDocument("$toString", "$_id"),
                candidateArr
            })
        )),

        new("$unwind", "$takeExams"),

        // examId KHÔNG null => match trực tiếp
        new("$match", new BsonDocument
        {
            { "takeExams.organizeExamId", organizeExamId },
            { "takeExams.examId",        examId },
            { "takeExams.sessionId",     new BsonDocument("$in", sessionArr) },
            { "takeExams.roomId",        new BsonDocument("$in", roomArr) }
            // Nếu cần lọc thêm: { "takeExams.status", "done" }
        }),

        new("$unwind", "$takeExams.answers"),

        new("$group", new BsonDocument
        {
            { "_id", "$takeExams.answers.questionId" },
            { "Correct", new BsonDocument("$sum",
                new BsonDocument("$cond", new BsonArray { "$takeExams.answers.isCorrect", 1, 0 })) },
            { "Incorrect", new BsonDocument("$sum",
                new BsonDocument("$cond", new BsonArray { new BsonDocument("$not", new BsonArray { "$takeExams.answers.isCorrect" }), 1, 0 })) }
        }),

        new("$project", new BsonDocument
        {
            { "_id", 0 },
            { "QuestionId", "$_id" },
            { "Correct", 1 },
            { "Incorrect", 1 }
        })
    };

        var docs = await _users.Aggregate<BsonDocument>(pipeline).ToListAsync();

        return docs.Select(d => new QuestionStatDto
        {
            QuestionId = d.GetValue("QuestionId", "").AsString,
            Correct = d.GetValue("Correct", 0).ToInt64(),
            Incorrect = d.GetValue("Incorrect", 0).ToInt64()
        })
        .OrderBy(x => x.QuestionId)
        .ToList();
    }

    public List<string> CollectionCandidateIds (OrganizeExamModel organizeExam)
    {
        var set = new HashSet<string>();
        if (organizeExam.Sessions == null) return set.ToList();

        foreach (var s in organizeExam.Sessions)
        {
            if (s.RoomsInSession == null) continue;

            foreach (var r in s.RoomsInSession)
            {
                if (r.CandidateIds == null) continue;

                foreach (var cid in r.CandidateIds)
                    if (!string.IsNullOrWhiteSpace(cid)) set.Add(cid);
            }
        }
        return set.ToList();
    }

    //Get all question in question bank
    public async Task<List<QuestionItem>> GetQuestionsByOrganizeExamAsync(string subjectId, string questionBankId)
    {
        var subject = await _subjects.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
        var qb = subject?.QuestionBanks?.FirstOrDefault(x => x.QuestionBankId == questionBankId);

        var list = new List<QuestionItem>();
        if (qb?.QuestionList == null) return list;

        foreach (var q in qb.QuestionList)
        {
            if (string.IsNullOrWhiteSpace(q.QuestionId)) continue;

            list.Add(new QuestionItem
            {
                QuestionId = q.QuestionId ?? string.Empty,
                QuestionType = q.QuestionType ?? string.Empty,
                QuestionText = q.QuestionText ?? string.Empty,
                tags = q.Tags ?? new List<string>(),
                Options = (q.Options ?? new List<OptionsModel>()).Select(o => new OptionItem
                {
                    OptionId = o.OptionId ?? string.Empty,
                    OptionText = o.OptionText ?? string.Empty,
                    IsCorrect = o.IsCorrect ?? false,
                    SelectedCount = 0 
                }).ToList()
            });
        }
        return list;
    }

    public async Task<Dictionary<string, Dictionary<string, long>>> AggregateOptionCountsAsync(
    string organizeExamId, IEnumerable<string> candidateIds)
    {
        var counts = new Dictionary<string, Dictionary<string, long>>();

        var objectIds = (candidateIds ?? Array.Empty<string>())
            .Where(s => ObjectId.TryParse(s, out _))
            .Select(ObjectId.Parse)
            .Distinct()
            .ToList();

        if (objectIds.Count == 0) return counts;

        var pipeline = new List<BsonDocument>
        {
            new("$match", new BsonDocument("_id", new BsonDocument("$in", new BsonArray(objectIds)))),

            new("$unwind", "$takeExams"),
            new("$match", new BsonDocument
            {
                { "takeExams.organizeExamId", organizeExamId },
                { "takeExams.status", "terminate" } // chỉ tính bài đã nộp
            }),

            new("$unwind", "$takeExams.answers"),

            // Nếu answerChose null/rỗng thì gán ["__NONE__"]
            new("$set", new BsonDocument
            {
                { "chosen", new BsonDocument("$cond", new BsonArray {
                    new BsonDocument("$gt", new BsonArray {
                        new BsonDocument("$size", new BsonDocument("$ifNull", new BsonArray {
                            "$takeExams.answers.answerChose", new BsonArray()
                        })),
                        0
                    }),
                    "$takeExams.answers.answerChose",
                    new BsonArray { "__NONE__" }
                })}
            }),

            new("$unwind", "$chosen"),

            new("$group", new BsonDocument
            {
                { "_id", new BsonDocument {
                    { "QuestionId", "$takeExams.answers.questionId" },
                    { "OptionId",   "$chosen" }
                }},
                { "SelectedCount", new BsonDocument("$sum", 1) }
            }),

            new("$project", new BsonDocument
            {
                { "_id", 0 },
                { "QuestionId", "$_id.QuestionId" },
                { "OptionId",   "$_id.OptionId" },
                { "SelectedCount", 1 }
            })
        };


        var docs = await _users.Aggregate<BsonDocument>(pipeline).ToListAsync();

        foreach (var d in docs)
        {
            var qid = d.GetValue("QuestionId", "").AsString;
            var oid = d.GetValue("OptionId", "").AsString;
            var cnt = d.GetValue("SelectedCount", 0).ToInt64();
            if (string.IsNullOrEmpty(qid) || string.IsNullOrEmpty(oid)) continue;

            if (!counts.TryGetValue(qid, out var map))
                counts[qid] = map = new Dictionary<string, long>();
            map[oid] = (map.TryGetValue(oid, out var cur) ? cur : 0) + cnt;
        }

        return counts;
    }

    public async Task<long> CountParticipantsAsync(string organizeExamId, IEnumerable<string> candidateIds)
    {
        var candArr = new BsonArray((candidateIds ?? Array.Empty<string>()).Distinct());
        if (candArr.Count == 0) return 0L;

        var pipeline = new List<BsonDocument>
    {
        new("$match", new BsonDocument("$expr",
            new BsonDocument("$in", new BsonArray {
                new BsonDocument("$toString", "$_id"),
                candArr
            })
        )),
        new("$unwind", "$takeExams"),
        new("$match", new BsonDocument {
            { "takeExams.organizeExamId", organizeExamId },
            { "takeExams.status", new BsonDocument("$in", new BsonArray { "done", "terminate" }) }
        }),
        new("$group", new BsonDocument("_id", "$_id")),
        new("$count", "Participants")
    };

        var cursor = await _users.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
        return cursor?.GetValue("Participants", 0).ToInt64() ?? 0L;
    }

    public async Task UpsertOrganizeExamStatisticAsync(OrganizeExamStatisticModel doc)
    {
        if (string.IsNullOrWhiteSpace(doc.Id))
            doc.Id = ObjectId.GenerateNewId().ToString();

        var filter = Builders<OrganizeExamStatisticModel>.Filter
                        .Eq(x => x.OrganizeExamId, doc.OrganizeExamId);

        var options = new ReplaceOptions { IsUpsert = true };

        await _organizeExamStats.ReplaceOneAsync(filter, doc, options);
    }

    public async Task<OrganizeExamStatisticModel?> GetOrganizeExamStatisticAsync(string organizeExamId)
    {
        return await _organizeExamStats
            .Find(x => x.OrganizeExamId == organizeExamId)
            .FirstOrDefaultAsync();
    }

    public async Task UpsertParticipationViolationAsync(ParticipationViolationModel doc)
    {
        if (string.IsNullOrWhiteSpace(doc.Id))
            doc.Id = ObjectId.GenerateNewId().ToString();

        var filter = Builders<ParticipationViolationModel>.Filter
            .Eq(x => x.OrganizeExamId, doc.OrganizeExamId);

        await _participationViolations.ReplaceOneAsync(
            filter, doc, new ReplaceOptions { IsUpsert = true });
    }

    public async Task<ParticipationViolationModel?> GetParticipationViolationSnapshotAsync(string organizeExamId)
    {
        return await _participationViolations
            .Find(x => x.OrganizeExamId == organizeExamId)
            .FirstOrDefaultAsync();
    }

    public async Task UpsertGradeStatisticAsync(GradeStatisticModel doc)
    {
        var filter = Builders<GradeStatisticModel>.Filter
        .Eq(x => x.OrganizeExamId, doc.OrganizeExamId);

        var existing = await _gradeStats.Find(filter).FirstOrDefaultAsync();

        if (existing is null)
        {
            if (string.IsNullOrWhiteSpace(doc.Id))
                doc.Id = ObjectId.GenerateNewId().ToString();

            await _gradeStats.InsertOneAsync(doc);
        }
        else
        {
            // PHẢI giữ nguyên _id cũ
            doc.Id = existing.Id;
            await _gradeStats.ReplaceOneAsync(filter, doc);
        }
    }

    public async Task<GradeStatisticModel?> GetGradeStatisticSnapshotAsync(string organizeExamId)
    {
        return await _gradeStats
            .Find(x => x.OrganizeExamId == organizeExamId)
            .FirstOrDefaultAsync();
    }
}
