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
        if (userId == null || userRole is not "admin") return Unauthorized();

        return Ok();
    }
    
    [HttpPost("toggle-session-status")]
    public async Task<IActionResult> ToggleSessionStatus([FromBody] ToggleSessionStatusRequest request)
    {
        var (status, newStatus) = await _processTakeExamService.ToggleSessionStatus(request.OrganizeExamId, request.SessionId);
        // if (result is not null)
        //     return Ok(new { message = result == "active" ? "Kích hoạt ca thi thành công" : "Đóng ca thi thành công" });
        // return BadRequest(new { message = "Kích hoạt ca thi thất bại" });
        if (status != "success")
            return BadRequest(new { code = status, message = "Toggle session failed" });

        return Ok(new { code = status, newStatus });
    }

    [HttpPost("toggle-room-status")]
    public async Task<IActionResult> ToggleRoomStatus([FromBody] ToggleRoomStatusRequest request)
    {
        // var result = await _processTakeExamService.ToggleRoomStatus(request.OrganizeExamId, request.SessionId, request.RoomId);
        // if (result is not null)
        //     return Ok(new { message = result == "active" ? "Kích hoạt phòng thi thành công" : "Đóng phòng thi thành công" });
        // return BadRequest(new { message = "Kích hoạt phòng thi thất bại" });
        var (status, message) = await _processTakeExamService
            .ToggleRoomStatus(request.OrganizeExamId, request.SessionId, request.RoomId);

        return status switch
        {
            "active" or "closed" => Ok(new
            {
                code = "success",
                newStatus = status
            }),
            _ => BadRequest(new
            {
                code = status,
                message = message ?? "Failed to toggle room status"
            })
        };
    }

    // [Authorize]
    [HttpGet("activate-exam")]
    public async Task<IActionResult> GetActiveExam([FromQuery] string userId)
    {
        var result = await _processTakeExamService.GetActiveExam(userId);
        return Ok(result);
    }
    
    [HttpGet("take-exam")]
    public async Task<IActionResult> TakeExam(
        [FromQuery] string organizeExamId,
        [FromQuery] string sessionId,
        [FromQuery] string roomId,
        [FromQuery] string userId)
    {
        var result = await _processTakeExamService.TakeExam(organizeExamId, sessionId, roomId, userId);

        if (result == null)
        {
            return NotFound(new { message = "Không tìm thấy bài thi phù hợp hoặc không có câu hỏi khả dụng." });
        }

        return Ok(result);
    }
    
    [HttpPost("submit-answers")]
    public async Task<IActionResult> SubmitAnswers(
        [FromQuery] string userId,
        [FromQuery] string takeExamId,
        [FromQuery] string type,
        [FromBody] List<SubmitAnswersRequestDto> request)
    {
        var (description, status) = await _processTakeExamService.SubmitAnswers(userId, takeExamId, type, request);
        if (!status)
            return BadRequest(new { code = description, description = "Không thể lưu/nộp bài."});

        return Ok(new { message = type == "submit" ? "Nộp bài thành công." : "Lưu bài thành công." });
    }

    [HttpGet("track-exam")]
    public async Task<IActionResult> GetTrackExam([FromQuery] string userId)
    {
        var result = await _processTakeExamService.TrackActiveExam(userId);
        return Ok(result);
    }
    
    [HttpGet("track-exam-detail")]
    public async Task<IActionResult> GetTrackExamDetail([FromQuery] string organizeExamId, [FromQuery] string sessionId, [FromQuery] string roomId)
    {
        var result = await _processTakeExamService.TrackExamDetail(organizeExamId, sessionId, roomId);
        if (result == null)
            return NotFound("Không tìm thấy dữ liệu phù hợp");

        return Ok(result);
    }
    
    [HttpPost("violation")]
    public async Task<IActionResult> IncreaseViolation([FromQuery] string userId, [FromQuery] string takeExamId)
    {
        var success = await _processTakeExamService.IncreaseViolationCount(userId, takeExamId);
        if (!success) return NotFound("User or TakeExam not found.");

        return Ok("Violation count increased.");
    }
    
    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateStatus(
        [FromQuery] string userId,
        [FromQuery] string takeExamId,
        [FromBody] UpdateTakeExamStatusRequest request)
    {
        var success = await _processTakeExamService.UpdateStatusAndReason(userId, takeExamId, request.Type, request.UnrecognizedReason);
        if (!success) return NotFound("User or TakeExam not found.");

        return Ok("Status and reason updated.");
    }
    
    [HttpGet("check-can-continue")]
    public async Task<IActionResult> CheckCanContinue([FromQuery] string userId, [FromQuery] string takeExamId)
    {
        var (status, reason) = await _processTakeExamService.CheckCanContinue(userId, takeExamId);

        if (string.IsNullOrEmpty(status))
            return NotFound(new { message = "Không tìm thấy user hoặc takeExam." });

        bool canContinue = !status.Equals("terminate", StringComparison.OrdinalIgnoreCase);

        return Ok(new
        {
            canContinue,
            status,
            unrecognizedReason = reason
        });
    }
    
    [HttpGet("exam-result")]
    public async Task<IActionResult> GetExamResult(string userId, string takeExamId)
    {
        var (status, result) = await _processTakeExamService.GetExamResult(userId, takeExamId);
        return status switch
        {
            "error-user" => BadRequest(new { status, message = "Không tìm thấy dữ liệu kỳ thi phù hợp" }),
            "error-texam" => BadRequest(new { status, message = "Không tìm thấy dữ liệu làm bài" }),
            "error-status" => BadRequest(new { status, message = "Bài thi chưa được hoàn thành" }),
            "terminated" => Ok(new { status, data = result }),
            "done" => Ok(new { status, data = result }),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    [HttpGet("{userId}/exam-results")]
    public async Task<IActionResult> GetUserExamResults(string userId)
    {
        var result = await _processTakeExamService.GetUserExamHistory(userId);
        return Ok(result);
    }


}