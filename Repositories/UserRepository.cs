using Backend_online_testing.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Backend_online_testing.Dtos;

namespace Backend_online_testing.Repositories;

public interface IUsersRepository
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UsersModel?> GetByIdAsync(string id);
    Task CreateUserAsync(UsersModel user);
    Task<bool> UpdateUserAsync(string id, UsersModel user);
    Task<bool> DeleteUserAsync(string id);
}

public class UserRepository
{
    private readonly IMongoCollection<UsersModel> _users;
    private readonly IMongoCollection<SubjectsModel> _subjects;
    private readonly IMongoCollection<OrganizeExamModel> _organizeExams;
    private readonly IMongoCollection<ExamsModel> _exams;
    private readonly IMongoCollection<RoomsModel> _rooms;

    public UserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<UsersModel>("users");
        _subjects = database.GetCollection<SubjectsModel>("subjects");
        _exams = database.GetCollection<ExamsModel>("exams");
        _organizeExams = database.GetCollection<OrganizeExamModel>("organizeExams");
        _rooms = database.GetCollection<RoomsModel>("rooms");
    }

    //Count record
    public async Task<long> CountAsync(FilterDefinition<UsersModel> filter)
    {
        var baseFilter = Builders<UsersModel>.Filter.Ne(u => u.AccountStatus, "deleted");

        var combined = filter != null
            ? Builders<UsersModel>.Filter.And(baseFilter, filter)
            : baseFilter;

        return await _users.CountDocumentsAsync(combined);
    }

    //Get all user (exclude deleted, no password)
    public async Task<List<UserDto>> GetUsersAsync(FilterDefinition<UsersModel>? filter, int skip, int limit)
    {
        var baseFilter = Builders<UsersModel>.Filter.Ne(u => u.AccountStatus, "deleted");

        var combined = filter != null
            ? Builders<UsersModel>.Filter.And(baseFilter, filter)
            : baseFilter;

        var projection = Builders<UsersModel>.Projection.Expression(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            FullName = u.FullName,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role ?? string.Empty,
            UserCode = u.UserCode,
            Gender = u.Gender ?? string.Empty,
            DateOfBirth = u.DateOfBirth ?? string.Empty,
            GroupName = u.GroupName ?? new List<string>(),
            Authenticate = u.Authenticate ?? new List<string>(),
            AccountStatus = u.AccountStatus
        });

        return await _users.Find(combined)
            .Project(projection)
            .SortByDescending(u => u.Id)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
    }

    //Get user by id
    public async Task<UserDto> GetUserByIdAsync(string id)
    {
        var filter = Builders<UsersModel>.Filter.And(
            Builders<UsersModel>.Filter.Eq(u => u.Id, id),
            Builders<UsersModel>.Filter.Ne(u => u.AccountStatus, "deleted")
        );

        var projection = Builders<UsersModel>.Projection.Expression(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            FullName = u.FullName,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role ?? string.Empty,
            UserCode = u.UserCode,
            Gender = u.Gender ?? string.Empty,
            DateOfBirth = u.DateOfBirth ?? string.Empty,
            GroupName = u.GroupName ?? new List<string>(),
            Authenticate = u.Authenticate ?? new List<string>(),
            AccountStatus = u.AccountStatus
        });

        return await _users.Find(filter).Project(projection).FirstOrDefaultAsync();
    }

    //Find user by username
    public async Task<UsersModel?> GetByUsernameAsync(string username)
    {
        return await _users.Find(u => u.UserName == username).FirstOrDefaultAsync();
    }

    //Add new user
    public async Task InsertAsync(UsersModel user)
    {
        await _users.InsertOneAsync(user);
    }

    //Update user information
    public async Task<UpdateResult> UpdateAsync(string id, UpdateDefinition<UsersModel> update)
    {
        var filter = Builders<UsersModel>.Filter.Eq(u => u.Id, id);
        return await _users.UpdateOneAsync(filter, update);
    }

    //Delete user
    public async Task<DeleteResult> DeleteAsync(string id)
    {
        var filter = Builders<UsersModel>.Filter.Eq(u => u.Id, id);
        return await _users.DeleteOneAsync(filter);
    }

    /*
     * Get review user exam
     */
    public async Task<UsersModel> FindUserAsync(string userId)
    {
        var filter = Builders<UsersModel>.Filter.And(
            Builders<UsersModel>.Filter.Eq(u => u.Id, userId),
            Builders<UsersModel>.Filter.Ne(u => u.AccountStatus, "deleted")
        );
        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<SubjectsModel?> FindSubjectAsync(string subjectId)
        => await _subjects.Find(s => s.Id == subjectId).FirstOrDefaultAsync();

    public async Task<QuestionBanksModel?> FindQuestionBankAsync(
        string subjectId, string questionBankId)
    {
        var subject = await FindSubjectAsync(subjectId);
        return subject?.QuestionBanks?.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
    }

    public async Task<OrganizeExamModel?> FindOrganizeExamAsync(string id)
        => await _organizeExams.Find(o => o.Id == id).FirstOrDefaultAsync();

    public async Task<RoomsModel?> FindRoomAsync(string id)
        => await _rooms.Find(r => r.Id == id).FirstOrDefaultAsync();

    public async Task<ExamsModel?> FindExamAsync(string id)
        => await _exams.Find(e => e.Id == id).FirstOrDefaultAsync();
}
