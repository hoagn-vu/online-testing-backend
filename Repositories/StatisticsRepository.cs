using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories;

public class StatisticsRepository
{
    private readonly IMongoCollection<UsersModel> _users;
    private readonly IMongoCollection<OrganizeExamModel> _organizeExam;

    public StatisticsRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<UsersModel>("users");
        _organizeExam = database.GetCollection<OrganizeExamModel>("organizeExams");
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
}
