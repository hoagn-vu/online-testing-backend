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
    public async Task<IActionResult> Create([FromBody] OrganizeExamRequestDto dto)
    {
        var result = await _organizeExamService.CreateOrganizeExam(dto);
        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }

    // [HttpPut]
    // public async Task<IActionResult> Update([FromBody] OrganizeExamRequestDto dto)
    // {
    //     
    // }
    
    [HttpPost("{examId}/sessions")]
    public async Task<IActionResult> AddSession(string examId, [FromBody] SessionRequestDto dto)
    {
        var result = await _organizeExamService.AddSession(examId, dto);
        if (result == null) return NotFound();
        return Ok(result);
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
        if (questions == null || questions.Count == 0)
        {
            return NotFound("No questions found for the given exam.");
        }
        return Ok(new { Questions = questions, Duration = duration, SessionId = sessionId });
    }
    
    
}