using System.Security.Claims;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers;

[Route("api/organize-exams")]
[ApiController]
public class OrganizeExamController : ControllerBase
{
    private readonly OrganizeExamService _organizeExamService;
    private readonly ILogsService  _logService;

    public OrganizeExamController(OrganizeExamService organizeExamService,  ILogsService  logService)
    {
        _organizeExamService = organizeExamService;
        _logService = logService;
    }

    // Get all Exam
    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetOrganizeExam([FromQuery] string organizeExamId)
    {
        var organizeExams = await _organizeExamService.GetOrganizeExamById(organizeExamId);

        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "get-organize_exam",
            LogDetails = $"Truy cập kỳ thi \"{organizeExams?.OrganizeExamName}\"",
        });
        return Ok(organizeExams);
    }    
    
    [HttpGet]
    public async Task<IActionResult> GetOrganizeExams([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (organizeExams, totalCount) = await _organizeExamService.GetOrganizeExams(keyword, page, pageSize);
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "get-organize_exam",
            LogDetails = $"Truy cập danh sách kỳ thi",
        });
        return Ok(new { organizeExams, totalCount });
    }
    
    [HttpGet("sessions")]
    public async Task<ActionResult<List<OrganizeExamModel>>> GetSessions(string orgExamId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (organizeExamId, organizeExamName, sessions, totalCount)  = await _organizeExamService.GetSessions(orgExamId, keyword, page, pageSize);
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "get-organize_exam_session",
            LogDetails = $"Truy cập danh sách ca thi trong kỳ thi \"{organizeExamName}\"",
        });
        return Ok(new { organizeExamId, organizeExamName, sessions, totalCount });
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRoomsInSession(string orgExamId, string ssId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (organizeExamId, organizeExamName, sessionId, sessionName, rooms, totalCount) = await _organizeExamService.GetRoomsInSession(orgExamId, ssId, keyword, page, pageSize);
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "get-organize_exam_session_room",
            LogDetails = $"Truy cập danh sách phòng thi trong ca thi của \"{sessionName}\" kỳ thi \"{organizeExamName}\"",
        });
        return Ok(new { organizeExamId, organizeExamName, sessionId, sessionName, rooms, totalCount });
    }

    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidatesInSessionRoom(string orgExamId, string ssId, string rId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (organizeExamId, organizeExamName, sessionId, sessionName, roomId, roomName, candidates, totalCount) = await _organizeExamService.GetCandidatesInSessionRoom(orgExamId, ssId, rId, keyword, page, pageSize);
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "get-organize_exam_session_room_candidate",
            LogDetails = $"Truy cập danh sách thí sinh phòng thi \"{roomName}\" trong ca thi \"{sessionName}\" của kỳ thi \"{organizeExamName}\"",
        });
        return Ok(new { organizeExamId, organizeExamName, sessionId, sessionName, roomId, roomName, candidates, totalCount });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateOrganizeExam([FromBody] OrganizeExamRequestDto dto)
    {
        // var result = await _organizeExamService.CreateOrganizeExam(dto);
        // return CreatedAtAction(nameof(CreateOrganizeExam), new { id = result.Id }, result);
        // var result = await _organizeExamService.CreateOrganizeExamWithSessions(dto);
        // return Ok(result);
        var (status, exam) = await _organizeExamService.CreateOrganizeExamWithSessions(dto);

        if (status == "success")
        {
            await _logService.WriteLogAsync(new CreateLogDto
            {
                MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                LogAction = "post-organize_exam",
                LogDetails = $"Tạo kỳ thi \"{exam?.OrganizeExamName}\"",
            });
        }
        
        return status switch
        {
            "session-overlap" => BadRequest(new { message = "Ca thi bị chồng chéo thời gian." }),
            "duplicate-session-time" => BadRequest(new { status, message = "Trong kỳ thi đã tồn tại ca thi có cùng StartAt và FinishAt." }),
            "success" => Ok(new { status, data = exam }),
            _ => StatusCode(500, new { message = "Unexpected error" })
        };
    }

    [HttpPut("{organizeExamId}")]
    public async Task<ActionResult> UpdateOrganizeExam(string organizeExamId, [FromBody] OrganizeExamRequestDto dto)
    {
        var result = await _organizeExamService.UpdateOrganizeExam(organizeExamId, dto);
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "put-organize_exam",
            LogDetails = $" Cập nhật kỳ thi (id: {organizeExamId})",
        });
        return Ok(result);
    }
    
    [HttpPost("{examId}/sessions")]
    public async Task<IActionResult> AddSession(string examId, [FromBody] SessionRequestDto dto)
    {
        var (status, exam) = await _organizeExamService.AddSession(examId, dto);

        if (status == "success")
        {
            await _logService.WriteLogAsync(new CreateLogDto
            {
                MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                LogAction = "put-organize_exam",
                LogDetails = $"Thêm ca thi \"{dto.SessionName}\" cho kỳ thi \"{exam?.OrganizeExamName}\"",
            });    
        }
        
        return status switch
        {
            "exam-not-found" => NotFound(new { status, message = "Không tìm thấy kỳ thi." }),
            "session-overlap" => BadRequest(new { status, message = "Ca thi bị chồng chéo thời gian." }),
            "success" => Ok(new{ status, exam }),
            _ => StatusCode(500, new { message = "Unexpected error" })
        };
    }
    
    [HttpPut("{examId}/sessions/{sessionId}")]
    public async Task<IActionResult> UpdateSession(string examId, string sessionId, [FromBody] SessionRequestDto dto)
    {
        var result = await _organizeExamService.UpdateSession(examId, sessionId, dto);
        if (result != "Cập nhật ca thi thành công")
        {
            return NotFound(result);
        }

        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "put-organize_exam_session",
            LogDetails = $" Cập nhật ca thi \"{dto.SessionName}\" (id kỳ thi: {examId})",
        });
            
        return Ok(result);

    }

    //[HttpPost("{examId}/sessions/{sessionId}/rooms")]
    //public async Task<IActionResult> AddRoomToSession(string examId, string sessionId, [FromBody] RoomsInSessionRequestDto dto)
    //{
    //    var result = await _organizeExamService.AddRoomToSession(examId, sessionId, dto);
    //    if (result == null) return NotFound();
    //    return Ok(result);
    //    // var success = await _organizeExamService.AddRoomToSession(examId, sessionId, dto);
    //    // if (!success)
    //    // {
    //    //     return BadRequest("Failed to add room to session.");
    //    // }
    //    // return Ok("Room added successfully.");
    //    //
    //}

    [HttpPost("{examId}/sessions/{sessionId}/rooms/{roomId}/candidates")]
    public async Task<IActionResult> AddCandidateToRoom(string examId, string sessionId, string roomId, [FromBody] CandidatesInSessionRoomRequestDto dto)
    {
        var result = await _organizeExamService.AddCandidateToRoom(examId, sessionId, roomId, dto);
        
        await _logService.WriteLogAsync(new CreateLogDto
        {
            MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
            LogAction = "post-organize_exam_session_candidate",
            LogDetails = $"Thêm {dto.CandidateIds?.Count} vào phòng thi",
        });
        if (result == null) return NotFound();
        return Ok(result);
    }
    
    [HttpGet("candidate-get-list")]
    public async Task<IActionResult> GetExamsByCandidateId([FromQuery] string candidateId)
    {
        var exams = await _organizeExamService.GetExamsByCandidateId(candidateId);
        if (exams == null || exams.Count == 0)
        {
            return BadRequest("No exams found for the given candidate.");
        }
        return Ok(exams);
    }
    
    [HttpGet("questions/{organizeExamId}")]
    public async Task<IActionResult> GetQuestionsByExamId(string organizeExamId)
    {
        var (questions, duration, sessionId) = await _organizeExamService.GetQuestionsByExamId(organizeExamId);
        // if (questions == null || questions.Count == 0)
        // {
        //     return NotFound("No questions found for the given exam.");
        // }
        return Ok(new { Questions = questions, Duration = duration, SessionId = sessionId });
    }
    
    [HttpPost("{organizeExamId}/sessions/{sessionId}/rooms")]
    // public async Task<IActionResult> AddRoomToSession(
    //     string organizeExamId, string sessionId, [FromBody] AddRoomToSessionRequest request)
    // {
    //     const string DupMsg = "One or more rooms already exist in this session. Operation aborted.";
    //     try
    //     {
    //         await _organizeExamService.AddRoomsToSession_StrictAsync(organizeExamId, sessionId, request);
    //         return Ok(new { message = "Added rooms & allocated candidates successfully." });
    //     }
    //     catch (InvalidOperationException ex) when (string.Equals(ex.Message, DupMsg, StringComparison.Ordinal))
    //     {
    //         // Trả đúng 1 dòng text, status 409 (không stack trace)
    //         return new ContentResult
    //         {
    //             StatusCode = StatusCodes.Status409Conflict,
    //             Content = DupMsg,
    //             ContentType = "text/plain; charset=utf-8"
    //         };
    //     }
    // }
    public async Task<IActionResult> AddRoomsToSession(
        string organizeExamId,
        string sessionId,
        [FromBody] AddRoomToSessionRequest request)
    {
        var (status, message) = await _organizeExamService.AddRoomsToSession_StrictAsync(
            organizeExamId, sessionId, request);

        if (status == "success")
        {
            await _logService.WriteLogAsync(new CreateLogDto
            {
                MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                LogAction = "post-organize_exam_session_room_candidate",
                LogDetails = $"Chia thí sinh từ {request.GroupUserIds.Count} vào {request.RoomIds.Count} phòng thi",
            });
        }
        
        return status switch
        {
            // Missing / invalid args
            "missing-exam-id"       => BadRequest(new { status, message }),
            "missing-session-id"    => BadRequest(new { status, message }),
            "missing-request-body"  => BadRequest(new { status, message }),
            "empty-room-ids"        => BadRequest(new { status, message }),
            "invalid-room-quantity" => BadRequest(new { status, message }),
            "insufficient-quantity" => BadRequest(new { status, message }),

            // Not found
            "room-not-found"        => NotFound(new { status, message }),
            "candidates-not-found"  => NotFound(new { status, message }),

            // Validation / conflicts
            "invalid-supervisor"        => Conflict(new { status, message }),
            "room-already-in-session"   => Conflict(new { status, message }),
            "capacity-exceeded"         => Conflict(new { status, message }),
            "room-capacity-exceeded"    => Conflict(new { status, message }),
            "candidate-conflict"        => Conflict(new { status, message }),
            "supervisor-conflict"       => Conflict(new { status, message }),
            "session-time-duplicate"    => Conflict(new { status, message }),

            // Repo / system errors
            "repository-failure"    => StatusCode(StatusCodes.Status500InternalServerError, new { status, message }),

            // Success
            "success"               => Ok(new { status, message }),

            // Unknown / fallback
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { status = "unknown", message })
        };
    }

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions([FromQuery] string? subjectId, [FromQuery] string? status)
    {
        var result = await _organizeExamService.GetOrganizeExamOptions(subjectId, status);
        return Ok(result);
    }
    
    [HttpPut("{organizeExamId}/update-status")]
    public async Task<IActionResult> UpdateStatus([FromRoute] string organizeExamId, [FromQuery] string newStatus)
    {
        var (status, result) = await _organizeExamService.UpdateStatusAsync(organizeExamId, newStatus);

        return status switch
        {
            "organize-exam-not-found" => NotFound(new { code = status, message = "Organize exam not found" }),
            "update-failed" => BadRequest(new { code = status, message = "Update status failed" }),
            "success" => Ok(new { code = status, new_status = result }),
            _ => StatusCode(500, new { code = "unexpected-error", message = "Unexpected error occurred" })
        };
    }
    
    [HttpPut("{organizeExamId}/done")]
    public async Task<IActionResult> UpdateStatus([FromRoute] string organizeExamId)
    {
        var (status, result) = await _organizeExamService.UpdateStatusAsync(organizeExamId, "done");

        if (status == "success")
        {
            await _logService.WriteLogAsync(new CreateLogDto
            {
                MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                LogAction = "put-organize_exam_session",
                LogDetails = $"Đánh dấu hoàn thành kỳ thi (id: {organizeExamId})",
            });
        }
        
        return status switch
        {
            "organize-exam-not-found" => NotFound(new { code = status, message = "Organize exam not found" }),
            "update-failed" => BadRequest(new { code = status, message = "Update status failed" }),
            "success" => Ok(new { code = status, new_status = result }),
            _ => StatusCode(500, new { code = "unexpected-error", message = "Unexpected error occurred" })
        };
    }
   
}