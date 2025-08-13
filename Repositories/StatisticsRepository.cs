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

    public StatisticsRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<UsersModel>("users");
        _organizeExam = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjects = database.GetCollection<SubjectsModel>("subjects");
        _rooms = database.GetCollection<RoomsModel>("rooms");
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

    //Get subject name by id

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
}
