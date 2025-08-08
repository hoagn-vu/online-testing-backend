using Backend_online_testing.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using MongoDB.Driver;

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

    //Get all group user
    public async Task<List<GroupUserModel>> GetAllAsync()
    {
        return await _groupUser.Find(_ => true).ToListAsync();
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
    //Create group user
    public async Task CreateGroupUserAsync(GroupUserModel group)
    {
        await _groupUser.InsertOneAsync(group);
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
}
