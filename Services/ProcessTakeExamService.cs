using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Services;

public class ProcessTakeExamService
{
    private readonly IMongoCollection<OrganizeExamModel> _organizeExamCollection;
    private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
    private readonly IMongoCollection<RoomsModel> _roomsCollection;
    private readonly IMongoCollection<UsersModel> _usersCollection;
    private readonly IMongoCollection<ExamsModel> _examsCollection;
    private readonly IMongoCollection<ExamMatricesModel> _examMatricesCollection;

    public ProcessTakeExamService(IMongoDatabase database)
    {
        _organizeExamCollection = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
        _roomsCollection = database.GetCollection<RoomsModel>("rooms");
        _usersCollection = database.GetCollection<UsersModel>("users");
        _examsCollection = database.GetCollection<ExamsModel>("exams");
        _examMatricesCollection = database.GetCollection<ExamMatricesModel>("examMatrices");
    }

    public async Task<string?> ToggleSessionStatus(string organizeExamId, string sessionId)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(x => x.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(x => x.Sessions, s => s.SessionId == sessionId)
        );
        
        var session = (await _organizeExamCollection.Find(filter).FirstOrDefaultAsync())?
            .Sessions.FirstOrDefault(s => s.SessionId == sessionId);

        if (session == null) return null;
        
        var newStatus = session.SessionStatus == "active" ? "closed" : "active";
        var update = Builders<OrganizeExamModel>.Update.Set("sessions.$.sessionStatus", newStatus);

        var result = await _organizeExamCollection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0 ? newStatus : null;
    }

    public async Task<string?> ToggleRoomStatus(string organizeExamId, string sessionId, string roomId)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(x => x.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(x => x.Sessions, s => s.SessionId == sessionId)
        );
        
        var session = (await _organizeExamCollection.Find(filter).FirstOrDefaultAsync())?
            .Sessions.FirstOrDefault(s => s.SessionId == sessionId);

        if (session == null) return null;
        
        var room = session.RoomsInSession.FirstOrDefault(r => r.RoomInSessionId == roomId);
        if (room == null) return null;


        var newStatus = room.RoomStatus == "active" ? "closed" : "active";
        var update = Builders<OrganizeExamModel>.Update.Set("sessions.$.rooms.$[room].roomStatus", newStatus);

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new JsonArrayFilterDefinition<BsonDocument>($"{{ 'room.roomId': '{roomId}' }}")
        };
        
        var candidateIds = room.CandidateIds;
        var userFilter = Builders<UsersModel>.Filter.In(u => u.Id, candidateIds);
        var userUpdate = Builders<UsersModel>.Update.Set("takeExams.$[].status", newStatus);

        await _usersCollection.UpdateManyAsync(userFilter, userUpdate);

        var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };
        var result = await _organizeExamCollection.UpdateOneAsync(filter, update, updateOptions);

        return result.ModifiedCount > 0 ? newStatus : null;
    }

    public async Task<List<ProcessTakeExamDto>> GetActiveExam(string userId)
    {
        List<ProcessTakeExamDto> allActiveExams = [];
        var user = await _usersCollection.Find(usr => usr.Id == userId).FirstOrDefaultAsync();
        if (user == null) return allActiveExams;
    
        List<string> accessStatus = ["active", "notin", "inexam", "outexam"];
        if (user.TakeExam == null) return allActiveExams;
        var activeExams = user.TakeExam.Where(e => accessStatus.Contains(e.Status)).ToList();
            
        foreach (var ex in activeExams)
        {
            var orgExam = _organizeExamCollection.Find(oe => oe.Id == ex.OrganizeExamId).FirstOrDefault();
            var session = orgExam?.Sessions.FirstOrDefault(s => s.SessionId == ex.SessionId);
                
            allActiveExams.Add(new ProcessTakeExamDto
            {
                OrganizeExamId = ex.OrganizeExamId,
                OrganizeExamName = orgExam != null ? orgExam.OrganizeExamName : "",
                SessionId = ex.SessionId,
                SessionName = session != null ? session.SessionName : "",
                RoomId = ex.RoomId,
                ExamType = orgExam != null ? orgExam.ExamType : "",
                MatrixId = orgExam != null ? orgExam.MatrixId : "",
                ExamId = ex.ExamId,
                Status = ex.Status,
            });
        }

        return allActiveExams;
    }

    // public async Task<string> TakeExam(string organizeExamId, string sessionId, string roomId, string userId)
    // {
    //     
    // }
}