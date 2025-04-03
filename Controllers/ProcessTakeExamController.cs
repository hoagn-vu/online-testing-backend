using System.Security.Claims;
using Backend_online_testing.Dtos;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers;

[Route("api/process-take-exams")]
[ApiController]
public class ProcessTakeExamController : ControllerBase
{
    private readonly ProcessTakeExamService _processTakeExamService;

    public ProcessTakeExamController(ProcessTakeExamService processTakeExamService)
    {
        _processTakeExamService = processTakeExamService;
    }
    
    [Authorize]
    [HttpGet("test")]
    public async Task<IActionResult> GetTest()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);
        if (userId == null || userRole is not "admin") 
            return Unauthorized();

        return Ok();
    }
    
    [HttpPost("toggle-session-status")]
    public async Task<IActionResult> ToggleSessionStatus([FromBody] ToggleSessionStatusRequest request)
    {
        var result = await _processTakeExamService.ToggleSessionStatus(request.OrganizeExamId, request.SessionId);
        if (result is not null)
            return Ok(new { message = result == "active" ? "Kích hoạt ca thi thành công" : "Đóng ca thi thành công" });
        return BadRequest(new { message = "Kích hoạt ca thi thất bại" });
    }

    [HttpPost("toggle-room-status")]
    public async Task<IActionResult> ToggleRoomStatus([FromBody] ToggleRoomStatusRequest request)
    {
        var result = await _processTakeExamService.ToggleRoomStatus(request.OrganizeExamId, request.SessionId, request.RoomId);
        if (result is not null)
            return Ok(new { message = result == "active" ? "Kích hoạt phòng thi thành công" : "Đóng phòng thi thành công" });
        return BadRequest(new { message = "Kích hoạt phòng thi thất bại" });
    }

    // [Authorize]
    [HttpGet("activate-exam")]
    public async Task<IActionResult> GetActiveExam([FromQuery] string userId)
    {
        var result = await _processTakeExamService.GetActiveExam(userId);
        return Ok(result);
    }
}