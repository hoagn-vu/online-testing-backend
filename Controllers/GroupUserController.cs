using System.Security.Claims;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers;

[ApiController]
[Route("api/groupUser")]
public class GroupUserController : ControllerBase
{
    private readonly GroupUserService _groupUserService;
    private readonly ILogsService _logService;

    public GroupUserController(GroupUserService groupUserService,  ILogsService logsService)
    {
        _groupUserService = groupUserService;
        _logService = logsService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GroupUserModel>>> GetAll(string? keyword, int page, int pageSize)
    {
        var (groups, totalCount) = await _groupUserService.GetAllGroupUserAsync(keyword, page, pageSize);
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "get-group_user",
            LogDetails = $"Truy cập danh sách nhóm người dùng",
        });
        
        return Ok( new { groups, totalCount });
    }

    [HttpGet("group/users")]
    public async Task<IActionResult> GetUsersInGroup(string groupId, string? keyword, int page, int pageSize)
    {
        var (groupName, group_id, status, total, items) = await _groupUserService.GetUserInfoInGroup(groupId, keyword, page, pageSize);

        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "get-group_user_detail",
            LogDetails = $"Truy cập chi tiết nhóm người dùng \"{groupName}\"",
        });
        
        return Ok(new { groupName, group_id, status, total, items });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GroupUserModel>> GetGroupUserById(string id)
    {
        var group = await _groupUserService.GetGroupUserByIdAsync(id);
        if (group == null) return NotFound();
        return Ok(group);
    }

    [HttpPost("create-group-users")]
    public async Task<ActionResult> CreateGroupUser([FromBody] GroupUserCreateDto groupDto)
    {
        var (groupName, result) = await _groupUserService.CreateGroupUserAsync(groupDto);
        if (!result) return BadRequest("Cannot create group");
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "post-group_user",
            LogDetails = $"Tạo nhóm người dùng \"{groupName}\"",
        });
        return Ok("Group created successfully");
    }

    [HttpPost("add-users")]
    public async Task<IActionResult> AddUsersToGroup(string groupId, [FromBody] List<string> userCodes)
    {
        var success = await _groupUserService.AddUsersToGroupAsync(groupId, userCodes);
        if (!success)
            return BadRequest("Failed to add users to group.");
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "post-group_user_member",
            LogDetails = $"Thêm {userCodes.Count} người dùng vào nhóm",
        });
        
        return Ok("Users added to group successfully.");
    }


    [HttpPut("update/{groupId}")]
    public async Task<IActionResult> UpdateGroupName(string groupId, [FromBody] string newName)
    {
        var success = await _groupUserService.UpdateGroupNameAsync(groupId, newName);
        if (!success) return NotFound("Group not found or update failed");
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "put-group_user",
            LogDetails = $"Cập nhật tên nhóm người dùng \"{newName}\"",
        });
        return Ok("Group name updated");
    }

    [HttpDelete("delete/{groupId}/user/{userCode}")]
    public async Task<ActionResult> RemoveUserFromGroup(string groupId, string userCode)
    {
        var result = await _groupUserService.RemoveUserFromGroupUserAsync(groupId, userCode);
        if (!result) return NotFound("User not found in group");
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "put-group_user",
            LogDetails = $"Xóa người dùng với mã \"{userCode}\" khỏi nhóm",
        });
        return Ok("User removed from group");
    }

    [HttpDelete("delete/{id}")]
    public async Task<ActionResult> DeleteById(string id)
    {
        var (roomName, result) = await _groupUserService.DeleteGroupUserAsync(id);
        if (result == "not-found") return NotFound("Group not found");

        if (result == "Success")
        {
            await _logService.WriteLogAsync(new CreateLogDto
            {
                MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                LogAction = "delete-group_user",
                LogDetails = $"Xóa nhóm người dùng \"{roomName}\"",
            });
            return Ok("Deleted successfully");
        }
        
        return BadRequest(result);
    }
    
    [HttpPost("get-users-from-groups")]
    public async Task<IActionResult> GetUsersFromGroups([FromBody] GetUsersFromGroupsRequestDto request)
    {
        var result = await _groupUserService.GetUsersFromGroupsAsync(request.GroupUserIds);
        return Ok(result);
    }
    
    
}
