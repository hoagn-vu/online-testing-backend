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

    public OrganizeExamController(OrganizeExamService organizeExamService)
    {
        _organizeExamService = organizeExamService;
    }

    // Get all Exam
    [HttpGet("get-by-id")]
    public async Task<IActionResult> GetOrganizeExam([FromQuery] string organizeExamId)
    {
        var organizeExams = await _organizeExamService.GetOrganizeExamById(organizeExamId);

        return Ok(organizeExams);
    }    
    
    [HttpGet]
    public async Task<IActionResult> GetOrganizeExams([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (organizeExams, totalCount) = await _organizeExamService.GetOrganizeExams(keyword, page, pageSize);

        return Ok(new { organizeExams, totalCount });
    }
    
    [HttpGet("sessions")]
    public async Task<ActionResult<List<OrganizeExamModel>>> GetSessions(string orgExamId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (organizeExamId, organizeExamName, sessions, totalCount)  = await _organizeExamService.GetSessions(orgExamId, keyword, page, pageSize);
        return Ok(new { organizeExamId, organizeExamName, sessions, totalCount });
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRoomsInSession(string orgExamId, string ssId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (organizeExamId, organizeExamName, sessionId, sessionName, rooms, totalCount) = await _organizeExamService.GetRoomsInSession(orgExamId, ssId, keyword, page, pageSize);
        return Ok(new { organizeExamId, organizeExamName, sessionId, sessionName, rooms, totalCount });
    }

    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidatesInSessionRoom(string orgExamId, string ssId, string rId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (organizeExamId, organizeExamName, sessionId, sessionName, roomId, roomName, candidates, totalCount) = await _organizeExamService.GetCandidatesInSessionRoom(orgExamId, ssId, rId, keyword, page, pageSize);
        return Ok(new { organizeExamId, organizeExamName, sessionId, sessionName, roomId, roomName, candidates, totalCount });
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateOrganizeExam([FromBody] OrganizeExamRequestDto dto)
    {
        // var result = await _organizeExamService.CreateOrganizeExam(dto);
        // return CreatedAtAction(nameof(CreateOrganizeExam), new { id = result.Id }, result);
        var result = await _organizeExamService.CreateOrganizeExamWithSessions(dto);
        return Ok(result);
    }

    [HttpPut("{organizeExamId}")]
    public async Task<ActionResult> UpdateOrganizeExam(string organizeExamId, [FromBody] OrganizeExamRequestDto dto)
    {
        var result = await _organizeExamService.UpdateOrganizeExam(organizeExamId, dto);
        return Ok(result);
    }
    
    [HttpPost("{examId}/sessions")]
    public async Task<IActionResult> AddSession(string examId, [FromBody] SessionRequestDto dto)
    {
        var result = await _organizeExamService.AddSession(examId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }
    
    [HttpPut("{examId}/sessions/{sessionId}")]
    public async Task<IActionResult> UpdateSession(string examId, string sessionId, [FromBody] SessionRequestDto dto)
    {
        var result = await _organizeExamService.UpdateSession(examId, sessionId, dto);
        return result == "Cập nhật ca thi thành công" ? Ok(result) : NotFound(result);
    }

    [HttpPost("{examId}/sessions/{sessionId}/rooms")]
    public async Task<IActionResult> AddRoomToSession(string examId, string sessionId, [FromBody] RoomsInSessionRequestDto dto)
    {
        var result = await _organizeExamService.AddRoomToSession(examId, sessionId, dto);
        if (result == null) return NotFound();
        return Ok(result);
        // var success = await _organizeExamService.AddRoomToSession(examId, sessionId, dto);
        // if (!success)
        // {
        //     return BadRequest("Failed to add room to session.");
        // }
        // return Ok("Room added successfully.");
        //
    }

    [HttpPost("{examId}/sessions/{sessionId}/rooms/{roomId}/candidates")]
    public async Task<IActionResult> AddCandidateToRoom(string examId, string sessionId, string roomId, [FromBody] CandidatesInSessionRoomRequestDto dto)
    {
        var result = await _organizeExamService.AddCandidateToRoom(examId, sessionId, roomId, dto);
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

    [HttpGet("options")]
    public async Task<IActionResult> GetOptions([FromQuery] string? subjectId)
    {
        var result = await _organizeExamService.GetOrganizeExamOptions(subjectId);
        return Ok(result);
    }
    
}