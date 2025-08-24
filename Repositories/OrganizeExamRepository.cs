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


}
