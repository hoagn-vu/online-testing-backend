using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Backend_online_testing.Services;

public class GroupUserService
{
    private readonly GroupUserRepository _groupUserRepository;

    public GroupUserService(GroupUserRepository groupUserRepository)
    {
        _groupUserRepository = groupUserRepository;
    }

    //Get all group user
    public async Task<List<GroupUserModel>> GetAllGroupUserAsync()
    {
        return await _groupUserRepository.GetAllAsync();
    }

    //Get group user by id
    public async Task<GroupUserModel?> GetGroupUserByIdAsync(string groupUserId)
    {
        return await _groupUserRepository.GetByIdAsync(groupUserId);
    }

    //Update group name
    public async Task<bool> UpdateGroupNameAsync(string groupId, string newGroupName)
    {
        return await _groupUserRepository.UpdateGroupNameAsync(groupId, newGroupName);
    }

    //Create group user
    public async Task<bool> CreateGroupUserAsync(GroupUserCreateDto groupDto)
    {
        var userIds = await _groupUserRepository.GetUserIdsByUserCodesAsync(groupDto.ListUser);

        var group = new GroupUserModel
        {
            GroupName = groupDto.GroupName,
            ListUser = userIds,
            GroupStatus = "active"
        };

        await _groupUserRepository.CreateGroupUserAsync(group);
        return true;
    }

    //Delete group user by id
    public async Task<bool> DeleteGroupUserAsync(string userGroupId)
    {
        return await _groupUserRepository.DeleteByIdAsync(userGroupId);
    }

    //Delete user code in group
    public async Task<bool> RemoveUserFromGroupUserAsync(string groupId, string userCode)
    {
        return await _groupUserRepository.RemoveUserCodeFromGroupAsync(groupId, userCode);
    }
}
