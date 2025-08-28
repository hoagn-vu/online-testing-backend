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
    public async Task<(List<GroupUserModel>, long)> GetAllGroupUserAsync(string? keyword, int page, int pageSize)
    {
        var totalCount = await _groupUserRepository.CountGroupUserAsync(keyword);
        var groupUsers = await _groupUserRepository.GetAllAsync(keyword, page, pageSize);

        return (groupUsers,  totalCount);
    }

    //Get list user info in a group
    public async Task<(string, string, string, int, List<GroupUserInfoDto>)> GetUserInfoInGroup(string groupId, string ? keyword, int page, int pageSize)
    {
        var group = await _groupUserRepository.GetByIdAsync(groupId);
        var groupName = group.GroupName;

        if (group == null)
            return ("", "", "Group not found", 0, new List<GroupUserInfoDto>());

        var userIds = group.ListUser ?? new List<string>();
        if (userIds.Count == 0)
            return ("", "", "Empty group", 0, new List<GroupUserInfoDto>());

        var totalLong = await _groupUserRepository.CountUsersByIdsAsync(userIds, keyword);
        var total = (int)totalLong;

        if (total == 0)
            return (groupName, groupId, "Not found user!", 0, new List<GroupUserInfoDto>());

        var users = await _groupUserRepository.GetUsersByIdsAsync(userIds, keyword, page, pageSize);

        var items = users.Select(u => new GroupUserInfoDto
        {
            UserId = u.Id,
            UserCode = u.UserCode,
            UserName = u.UserName,
            FullName = u.FullName,
            Gender = u.Gender,
            DateOfBirth = u.DateOfBirth
        }).ToList();

        return (groupName, groupId,"success", total, items);
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

    //Add users to group
    public async Task<bool> AddUsersToGroupAsync(string groupId, List<string> userCodes)
    {
        if (string.IsNullOrEmpty(groupId) || userCodes == null || userCodes.Count == 0)
            return false;

        var userIds = await _groupUserRepository.GetUserIdsByUserCodesAsync(userCodes);

        if (userIds == null || userIds.Count == 0)
            throw new Exception("No matching users found for given user codes.");

        return await _groupUserRepository.AddUserIdsToGroupAsync(groupId, userIds);
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
    
    public async Task<List<UsersFromGroupsDto>> GetUsersFromGroupsAsync(List<string> groupUserIds)
    {
        if (groupUserIds == null || !groupUserIds.Any())
            return new List<UsersFromGroupsDto>();

        // Lấy group
        var groups = await _groupUserRepository.GetGroupsByIdsAsync(groupUserIds);

        // Lấy toàn bộ userId từ các group
        var userIds = groups.SelectMany(g => g.ListUser).Distinct().ToList();

        if (!userIds.Any())
            return new List<UsersFromGroupsDto>();

        // Lấy users
        var users = await _groupUserRepository.GetUsersByIdsAsync(userIds);

        // Map sang DTO + Sort theo FirstName, LastName
        return users
            .Select(u => new UsersFromGroupsDto
            {
                Id = u.Id,
                UserName = u.UserName,
                UserCode = u.UserCode,
                FullName = u.FullName,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                Gender = u.Gender ?? string.Empty,
                DateOfBirth = u.DateOfBirth ?? string.Empty,
            })
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToList();
    }

    
    
}
