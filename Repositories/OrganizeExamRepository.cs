// Backend_online_testing.Repositories
using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;

namespace Backend_online_testing.Repositories;

public class OrganizeExamRepository
{
    private readonly IMongoCollection<OrganizeExamModel> _organizeExams;
    private readonly IMongoCollection<BsonDocument> _organizeExamsRaw;
    private readonly IMongoCollection<SubjectsModel> _subjects;
    private readonly IMongoCollection<RoomsModel> _rooms;
    private readonly IMongoCollection<UsersModel> _users;
    private readonly IMongoCollection<ExamsModel> _exams;
    private readonly IMongoCollection<ExamMatricesModel> _examMatrices;
    private readonly IMongoCollection<GroupUserModel> _groupUser;

    public OrganizeExamRepository(IMongoDatabase database)
    {
        _organizeExams = database.GetCollection<OrganizeExamModel>("organizeExams");
        _organizeExamsRaw = database.GetCollection<BsonDocument>("organizeExams");
        _subjects = database.GetCollection<SubjectsModel>("subjects");
        _rooms = database.GetCollection<RoomsModel>("rooms");
        _users = database.GetCollection<UsersModel>("users");
        _exams = database.GetCollection<ExamsModel>("exams");
        _examMatrices = database.GetCollection<ExamMatricesModel>("examMatrices");
        _groupUser = database.GetCollection<GroupUserModel>("groupUser");
    }

    public async Task<UsersModel?> GetUserByIdAsync(string userId)
    {
        return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
    }

    public async Task<OrganizeExamModel?> GetOrganizeExamByIdAsync(string organizeExamId)
    {
        return await _organizeExams.Find(o => o.Id == organizeExamId).FirstOrDefaultAsync();
    }

    public async Task<RoomsModel?> GetRoomByIdAsync(string roomId)
    {
        return await _rooms.Find(r => r.Id == roomId).FirstOrDefaultAsync();
    }

    // Get list user id from list group user
    public async Task<List<string>> GetUserIdsByGroupIdsAsync(List<string> groupIds)
    {
        if (groupIds is null || groupIds.Count == 0) return new();

        var filter = Builders<GroupUserModel>.Filter.In(x => x.Id, groupIds);
        var groups = await _groupUser.Find(filter)
                                     .Project(x => x.ListUser)
                                     .ToListAsync();

        return groups.SelectMany(x => x ?? new List<string>())
                     .Where(id => !string.IsNullOrWhiteSpace(id))
                     .Distinct()
                     .ToList();
    }

    // check supevisor id
    public async Task<bool> AreAllSupervisorsAsync(List<string> supervisorIds)
    {
        var ids = supervisorIds?.Distinct().ToList() ?? new();
        if (ids.Count == 0) return true;

        var filter = Builders<UsersModel>.Filter.In(x => x.Id, ids) &
                     Builders<UsersModel>.Filter.Eq(x => x.Role, "supervisor");

        var count = await _users.CountDocumentsAsync(filter);
        return count == ids.Count;
    }

    // Get candidate by id and sort by full name
    public async Task<List<string>> GetOrderedCandidatesAsync(List<string> userIds)
    {
        var ids = userIds?.Distinct().ToList() ?? new();
        if (ids.Count == 0) return new();

        var filter = Builders<UsersModel>.Filter.In(x => x.Id, ids) &
                     Builders<UsersModel>.Filter.Eq(x => x.Role, "candidate");
        // filter &= Builders<UsersModel>.Filter.Eq(x => x.AccountStatus, "active");

        var list = await _users.Find(filter)
                               .Project(u => new { u.Id, u.FirstName, u.LastName })
                               .ToListAsync();

        return list
            //.OrderBy(x => x.FirstName, StringComparer.OrdinalIgnoreCase) // FirstName priority
            //.ThenBy(x => x.LastName, StringComparer.OrdinalIgnoreCase)   // then, LastName
            //.Select(x => x.Id)
            //.ToList();
            .OrderBy(x => x.FirstName, StringComparer.Create(new CultureInfo("vi-VN"), ignoreCase: true))
            .ThenBy(x => x.LastName, StringComparer.Create(new CultureInfo("vi-VN"), ignoreCase: true))
            .Select(x => x.Id)
            .ToList();
            }


    // Check room id
    public async Task<bool> RoomsExistInMasterAsync(IEnumerable<string> roomIds)
    {
        var ids = roomIds?.Distinct().ToList() ?? new();
        if (ids.Count == 0) return true;

        var filter = Builders<RoomsModel>.Filter.In(x => x.Id, ids);
        var count = await _rooms.CountDocumentsAsync(filter);
        return count == ids.Count;
    }

    // Check whether roomId in session
    public async Task<bool> AnyRoomExistsInSessionAsync(
        string organizeExamId, string sessionId, IEnumerable<string> roomIds)
    {
        var idList = roomIds?.Distinct().ToList() ?? new();
        if (idList.Count == 0) return false;

        if (!ObjectId.TryParse(organizeExamId, out var orgOid)) return false;
        if (!ObjectId.TryParse(sessionId, out var sessionOid)) return false;

        var filter =
            Builders<BsonDocument>.Filter.Eq("_id", orgOid) &
            Builders<BsonDocument>.Filter.ElemMatch("sessions",
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("_id", sessionOid),
                    Builders<BsonDocument>.Filter.ElemMatch("rooms",
                        Builders<BsonDocument>.Filter.In("roomId", idList))
                )
            );

        return await _organizeExamsRaw.Find(filter).Limit(1).AnyAsync();
    }

    // Push room to sessions.$.rooms, if all rooms not in session
    public async Task<bool> AddRoomsIfAllAbsentAsync(
        string organizeExamId,
        string sessionId,
        List<(string RoomId, List<string> SupervisorIds, List<string> CandidateIds, string? RoomStatus)> roomsToAdd)
    {
        if (roomsToAdd is null || roomsToAdd.Count == 0) return true;

        if (!ObjectId.TryParse(organizeExamId, out var orgOid)) return false;
        if (!ObjectId.TryParse(sessionId, out var sessionOid)) return false;

        var roomIds = roomsToAdd.Select(r => r.RoomId).Distinct().ToList();

        var filter =
            Builders<BsonDocument>.Filter.Eq("_id", orgOid) &
            Builders<BsonDocument>.Filter.ElemMatch("sessions",
                Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Eq("_id", sessionOid),
                    Builders<BsonDocument>.Filter.Not(
                        Builders<BsonDocument>.Filter.ElemMatch("rooms",
                            Builders<BsonDocument>.Filter.In("roomId", roomIds)
                        )
                    )
                )
            );

        var toPushArray = new BsonArray(
            roomsToAdd.Select(r => new BsonDocument
            {
                { "roomId",        r.RoomId },
                { "supervisorIds", new BsonArray(r.SupervisorIds ?? new()) },
                { "candidateIds",  new BsonArray(r.CandidateIds  ?? new()) },
                { "roomStatus",    r.RoomStatus ?? "closed" }
            })
        );

        var update = Builders<BsonDocument>.Update.PushEach("sessions.$[s].rooms", toPushArray);
        var options = new UpdateOptions
        {
            ArrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("s._id", sessionOid))
            }
        };

        var res = await _organizeExamsRaw.UpdateOneAsync(filter, update, options);
        return res.ModifiedCount > 0;
    }

    //Add take exam for candidate
    public async Task AddTakeExamsForCandidatesAsync(
    string organizeExamId, string sessionId, string roomId, List<string> candidateIds)
    {
        if (candidateIds is null || candidateIds.Count == 0) return;

        var takeExam = new TakeExamsModel
        {
            Id = ObjectId.GenerateNewId().ToString(),
            OrganizeExamId = organizeExamId,
            SessionId = sessionId,
            RoomId = roomId,
            Status = "not_started",
            Progress = 0,
            ViolationCount = 0,
            Answers = new List<AnswersModel>()
        };

        // Add user not exist
        var baseFilter = Builders<UsersModel>.Filter.In(u => u.Id, candidateIds);
        var notExists = Builders<UsersModel>.Filter.Not(
            Builders<UsersModel>.Filter.ElemMatch(u => u.TakeExam,
                te => te.OrganizeExamId == organizeExamId
                   && te.SessionId == sessionId
                   && te.RoomId == roomId
            )
        );
        var filter = Builders<UsersModel>.Filter.And(baseFilter, notExists);

        var update = Builders<UsersModel>.Update.Push(u => u.TakeExam, takeExam);

        await _users.UpdateManyAsync(filter, update);
    }

    //Add track exam for supervisor
    public async Task AddTrackExamsForSupervisorsAsync(
    string organizeExamId, string sessionId, string roomId, List<string> supervisorIds, string? roomStatus = "closed")
    {
        if (supervisorIds is null || supervisorIds.Count == 0) return;

        var trackExam = new TrackExamsModel
        {
            Id = ObjectId.GenerateNewId().ToString(),
            OrganizeExamId = organizeExamId,
            SessionId = sessionId,
            RoomId = roomId,
            RoomSessionStatus = roomStatus
        };
        // Add user not exist
        var baseFilter = Builders<UsersModel>.Filter.In(u => u.Id, supervisorIds);
        var notExists = Builders<UsersModel>.Filter.Not(
            Builders<UsersModel>.Filter.ElemMatch(u => u.TrackExam,
                tr => tr.OrganizeExamId == organizeExamId
                   && tr.SessionId == sessionId
                   && tr.RoomId == roomId
            )
        );
        var filter = Builders<UsersModel>.Filter.And(baseFilter, notExists);

        var update = Builders<UsersModel>.Update.Push(u => u.TrackExam, trackExam);

        await _users.UpdateManyAsync(filter, update);
    }

    public async Task<OrganizeExamModel?> GetByIdAsync(string id)
    {
        return await _organizeExams.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateStatusAsync(string id, string status)
    {
        var update = Builders<OrganizeExamModel>.Update.Set(x => x.OrganizeExamStatus, status);
        var result = await _organizeExams.UpdateOneAsync(x => x.Id == id, update);
        return result.ModifiedCount > 0;
    }
    
    public async Task<int> GetMatrixTotalQuestionAsync(string id)
    {
        var matrix = await _examMatrices.Find(x => x.Id == id).FirstOrDefaultAsync();
        
        return matrix.MatrixTags.Sum(tag => tag.QuestionCount);
    }
    
    // public async Task AddRoomScheduleAsync(string organizeExamId, string sessionId, string roomId, int totalCandidates)
    // {
    //     // Lấy thông tin Session từ OrganizeExam để biết StartAt / FinishAt
    //     var exam = await _organizeExams.Find(x => x.Id == organizeExamId).FirstOrDefaultAsync();
    //     var session = exam?.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
    //     if (session == null) throw new InvalidOperationException("Session not found for adding RoomSchedule");
    //
    //     var newSchedule = new RoomScheduleModel
    //     {
    //         StartAt = session.StartAt,
    //         FinishAt = session.FinishAt,
    //         TotalCandidates = totalCandidates,
    //         OrganizeExamId = organizeExamId,
    //         SessionId = sessionId
    //     };
    //
    //     var filter = Builders<RoomsModel>.Filter.Eq(r => r.Id, roomId);
    //     var update = Builders<RoomsModel>.Update.Push(r => r.RoomSchedule, newSchedule);
    //
    //     await _rooms.UpdateOneAsync(filter, update);
    // }


    
    
    
    public async Task<bool> RoomsExistInMasterAsync(List<string> roomIds)
    {
        var count = await _rooms.CountDocumentsAsync(r => roomIds.Contains(r.Id));
        return count == roomIds.Count;
    }

    // public async Task<bool> AreAllSupervisorsAsync(List<string> supervisorIds)
    // {
    //     var count = await _users.CountDocumentsAsync(u => supervisorIds.Contains(u.Id) && u.Role == "supervisor");
    //     return count == supervisorIds.Count;
    // }

    // public async Task<List<string>> GetUserIdsByGroupIdsAsync(List<string> groupIds)
    // {
    //     var users = await _users.Find(u => u.GroupName.Any(g => groupIds.Contains(g))).ToListAsync();
    //     return users.Select(u => u.Id).ToList();
    // }

    // public async Task<List<string>> GetOrderedCandidatesAsync(List<string> userIds)
    // {
    //     var users = await _users.Find(u => userIds.Contains(u.Id)).ToListAsync();
    //     return users.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).Select(u => u.Id).ToList();
    // }

    public async Task<bool> AnyRoomExistsInSessionAsync(string organizeExamId, string sessionId, List<string> roomIds)
    {
        var exam = await _organizeExams.Find(x => x.Id == organizeExamId).FirstOrDefaultAsync();
        if (exam == null) return false;

        var session = exam.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session == null) return false;

        return session.RoomsInSession.Any(r => roomIds.Contains(r.RoomInSessionId));
    }

    public async Task<List<RoomsModel>> GetRoomsByIdsAsync(List<string> roomIds)
    {
        return await _rooms.Find(r => roomIds.Contains(r.Id)).ToListAsync();
    }

    public async Task<List<string>> GetCandidateIdsInSessionAsync(string organizeExamId, string sessionId)
    {
        var exam = await _organizeExams.Find(x => x.Id == organizeExamId).FirstOrDefaultAsync();
        var session = exam?.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
        return session?.RoomsInSession.SelectMany(r => r.CandidateIds).Distinct().ToList() ?? new List<string>();
    }

    public async Task<List<string>> GetSupervisorIdsInSessionAsync(string organizeExamId, string sessionId)
    {
        var exam = await _organizeExams.Find(x => x.Id == organizeExamId).FirstOrDefaultAsync();
        var session = exam?.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
        return session?.RoomsInSession.SelectMany(r => r.SupervisorIds).Distinct().ToList() ?? new List<string>();
    }

    public async Task<bool> HasDuplicateSessionTimeAsync(string organizeExamId, string sessionId)
    {
        var exam = await _organizeExams.Find(x => x.Id == organizeExamId).FirstOrDefaultAsync();
        if (exam == null) return false;

        var session = exam.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session == null) return false;

        return exam.Sessions.Any(s => s.SessionId != sessionId && s.StartAt == session.StartAt && s.FinishAt == session.FinishAt);
    }

    // public async Task<bool> AddRoomsIfAllAbsentAsync(string organizeExamId, string sessionId,
    //     List<(string RoomId, List<string> SupervisorIds, List<string> CandidateIds, string? RoomStatus)> rooms)
    // {
    //     var filter = Builders<OrganizeExamModel>.Filter.And(
    //         Builders<OrganizeExamModel>.Filter.Eq(x => x.Id, organizeExamId),
    //         Builders<OrganizeExamModel>.Filter.ElemMatch(x => x.Sessions, s => s.SessionId == sessionId)
    //     );
    //
    //     var update = Builders<OrganizeExamModel>.Update.PushEach(
    //         "sessions.$.rooms",
    //         rooms.Select(r => new SessionRoomsModel
    //         {
    //             RoomInSessionId = r.RoomId,
    //             SupervisorIds = r.SupervisorIds,
    //             CandidateIds = r.CandidateIds,
    //             RoomStatus = r.RoomStatus ?? "closed"
    //         })
    //     );
    //
    //     var result = await _organizeExams.UpdateOneAsync(filter, update);
    //     return result.ModifiedCount > 0;
    // }

    // public async Task AddTakeExamsForCandidatesAsync(string organizeExamId, string sessionId, string roomId, List<string> candidateIds)
    // {
    //     if (candidateIds.Count == 0) return;
    //
    //     var update = Builders<UsersModel>.Update.PushEach(u => u.TakeExam, candidateIds.Select(id => new TakeExamsModel
    //     {
    //         OrganizeExamId = organizeExamId,
    //         SessionId = sessionId,
    //         RoomId = roomId,
    //         Status = "not_started",
    //         RoomStatus = "closed"
    //     }).ToList());
    //
    //     await _users.UpdateManyAsync(u => candidateIds.Contains(u.Id), update);
    // }

    // public async Task AddTrackExamsForSupervisorsAsync(string organizeExamId, string sessionId, string roomId, List<string> supervisorIds, string roomStatus)
    // {
    //     if (supervisorIds.Count == 0) return;
    //
    //     var update = Builders<UsersModel>.Update.PushEach(u => u.TrackExam, supervisorIds.Select(id => new TrackExamsModel
    //     {
    //         OrganizeExamId = organizeExamId,
    //         SessionId = sessionId,
    //         RoomId = roomId,
    //         RoomSessionStatus = roomStatus
    //     }).ToList());
    //
    //     await _users.UpdateManyAsync(u => supervisorIds.Contains(u.Id), update);
    // }

    public async Task AddRoomScheduleAsync(string organizeExamId, string sessionId, string roomId, int totalCandidates)
    {
        var exam = await _organizeExams.Find(x => x.Id == organizeExamId).FirstOrDefaultAsync();
        var session = exam?.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session == null) return;

        var update = Builders<RoomsModel>.Update.Push(r => r.RoomSchedule, new RoomScheduleModel
        {
            StartAt = session.StartAt,
            FinishAt = session.FinishAt,
            OrganizeExamId = organizeExamId,
            SessionId = sessionId,
            TotalCandidates = totalCandidates
        });

        await _rooms.UpdateOneAsync(r => r.Id == roomId, update);
    }
}
