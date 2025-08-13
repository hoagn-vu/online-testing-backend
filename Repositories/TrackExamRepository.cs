using Amazon.S3.Model;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories;

public class TrackExamRepository
{
    private readonly IMongoCollection<UsersModel> _users;
    private readonly IMongoCollection<OrganizeExamModel> _organizeExam;
    private readonly IMongoCollection<SubjectsModel> _subjects;
    private readonly IMongoCollection<RoomsModel> _rooms;

    public TrackExamRepository (IMongoDatabase database)
    {
        _users = database.GetCollection<UsersModel>("users");
        _organizeExam = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjects = database.GetCollection<SubjectsModel>("subjects");
        _rooms = database.GetCollection<RoomsModel>("rooms");
    }

    //Create track exam session
    public async Task<bool> CreateTrackExamAsync(string userId, CreateTrackExamDto trackExamDto)
    {
        var filter = Builders<UsersModel>.Filter.Eq(u => u.Id, userId);

        var update = Builders<UsersModel>.Update.Push(u => u.TrackExam, new TrackExamsModel
        {
            //Id = Guid.NewGuid().ToString(),
            OrganizeExamId = trackExamDto.OrganizeExamId,
            SessionId = trackExamDto.SessionId,
            RoomId = trackExamDto.RoomId,
        });

        var result = await _users.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0;
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

    //Delete a track exam session
    public async Task<bool> DeleteTrackExamByIdAsync(string userId, string trackExamId)
    {
        var update = Builders<UsersModel>.Update.PullFilter(
            u => u.TrackExam,
            te => te.Id == trackExamId
        );

        var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
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
}
