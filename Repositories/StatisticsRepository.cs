using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories;

public class StatisticsRepository
{
    private readonly IMongoCollection<UsersModel> _users;
    private readonly IMongoCollection<OrganizeExamModel> _organizeExam;
    private readonly IMongoCollection<SubjectsModel> _subjects;
    private readonly IMongoCollection<RoomsModel> _rooms;
    private readonly IMongoCollection<ExamsModel> _exams;

    public StatisticsRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<UsersModel>("users");
        _organizeExam = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjects = database.GetCollection<SubjectsModel>("subjects");
        _rooms = database.GetCollection<RoomsModel>("rooms");
        _exams = database.GetCollection<ExamsModel>("exams");
    }

    //Get Subject Name
    public async Task<string?> GetSubjectNameByIdAsync(string subjectId)
    {
        var subject = await _subjects.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
        return subject?.SubjectName;
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
                              && x.RoomId == roomId);

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
}
