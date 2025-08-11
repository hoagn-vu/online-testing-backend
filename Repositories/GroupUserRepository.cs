using Backend_online_testing.Models;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Bson;
using MongoDB.Driver;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace Backend_online_testing.Repositories;

public class GroupUserRepository
{
    private readonly IMongoCollection<GroupUserModel> _groupUser;
    private readonly IMongoCollection<UsersModel> _user;

    public GroupUserRepository(IMongoDatabase database)
    {
        _groupUser = database.GetCollection<GroupUserModel>("groupUser");
        _user = database.GetCollection<UsersModel>("users");
    }

    private static readonly FilterDefinition<GroupUserModel> GroupUserBaseFilter = Builders<GroupUserModel>.Filter.Empty;

    private FilterDefinition<GroupUserModel> GroupUserBaseFilterByName(string? keyword)
    {
        var builder = Builders<GroupUserModel>.Filter;

        if (string.IsNullOrEmpty(keyword))
        {
            return GroupUserBaseFilter;
        }

        return builder.And(
                GroupUserBaseFilter,
                builder.Regex(s => s.GroupName, new BsonRegularExpression(keyword, "i"))
        );
    }
    //Count group user
    public async Task<long> CountGroupUserAsync(string? keyword)
    {
        var filter = GroupUserBaseFilterByName(keyword);

        return await _groupUser.CountDocumentsAsync(filter);
    }   


    //Get all group user
    public async Task<List<GroupUserModel>> GetAllAsync(string? keyword, int page, int pageSize)
    {
        var filter = GroupUserBaseFilterByName(keyword);

        return await _groupUser.Find(filter)
            .SortByDescending(g => g.Id)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    //Get by group user id
    public async Task<GroupUserModel> GetByIdAsync(string id)
    {
        return await _groupUser.Find(g => g.Id == id).FirstAsync();
    }

    //Find list user id by list usercode 
    public async Task<List<string>> GetUserIdsByUserCodesAsync(List<string> userCodes)
    {
        var users = await _user
        .Find(u => userCodes.Contains(u.UserCode))
        .ToListAsync();

        return users.Select(u => u.Id).ToList();
    }
    //Find user by user id
    public async Task<UsersModel> GetUserById(string userId)
    {
        return await _user
        .Find(u => u.Id == userId)
        .FirstOrDefaultAsync();
    }

    //Create group user
    public async Task CreateGroupUserAsync(GroupUserModel group)
    {
        await _groupUser.InsertOneAsync(group);
    }


    //Add multi user to group user
    public async Task<bool> AddUserIdsToGroupAsync(string groupId, List<string> userIds)
    {
        if (userIds == null || userIds.Count == 0)
            return false;

        var update = Builders<GroupUserModel>.Update
            .AddToSet(g => g.ListUser, userIds.First())
            .AddToSetEach(g => g.ListUser, userIds);

        var result = await _groupUser.UpdateOneAsync(
            g => g.Id == groupId,
            update
        );

        return result.ModifiedCount > 0;
    }

    //Update group name
    public async Task<bool> UpdateGroupNameAsync(string groupId, string newGroupName)
    {
        var update = Builders<GroupUserModel>.Update.Set(g => g.GroupName, newGroupName);

        var result = await _groupUser.UpdateOneAsync(
            g => g.Id == groupId,
            update
        );

        return result.ModifiedCount > 0;
    }

    //Delete user by id
    public async Task<bool> DeleteByIdAsync(string id)
    {
        var result = await _groupUser.DeleteOneAsync(g => g.Id == id);
        return result.DeletedCount > 0;
    }

    //Delete by user code
    public async Task<bool> RemoveUserCodeFromGroupAsync(string groupId, string userCode)
    {
        var update = Builders<GroupUserModel>.Update.Pull(g => g.ListUser, userCode);

        var result = await _groupUser.UpdateOneAsync(
            g => g.Id == groupId,
            update
        );

        return result.ModifiedCount > 0;
    }

    //Count user after filter
    public async Task<long> CountUsersByIdsAsync(IEnumerable<string> userIds, string? keyword)
    {
        var f = Builders<UsersModel>.Filter;
        var filter = f.In(u => u.Id, userIds);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var safe = System.Text.RegularExpressions.Regex.Escape(keyword.Trim());
            var rx = new BsonRegularExpression(safe, "i");

            // Find by name or gender
            var orFilter = f.Or(
                f.Regex(u => u.UserName, rx),
                f.Regex(u => u.Gender, rx)
            );

            filter = f.And(filter, orFilter);
        }

        return await _user.CountDocumentsAsync(filter);
    }

    public async Task<List<UsersModel>> GetUsersByIdsAsync(IEnumerable<string> userIds, string? keyword, int page, int pageSize)
    {
        var f = Builders<UsersModel>.Filter;
        var filter = f.In(u => u.Id, userIds);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var safe = System.Text.RegularExpressions.Regex.Escape(keyword.Trim());
            var rx = new BsonRegularExpression(safe, "i");

            // Find by name or gender
            var orFilter = f.Or(
                f.Regex(u => u.UserName, rx),
                f.Regex(u => u.Gender, rx)
            );

            filter = f.And(filter, orFilter);
        }

        return await _user.Find(filter)
            .SortByDescending(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }
}
