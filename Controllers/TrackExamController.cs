using Backend_online_testing.Dtos;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers;

[Route("api/trackexam")]

[ApiController]
public class TrackExamController : ControllerBase
{
    private readonly TrackExamService _trackExamService;

    public TrackExamController(TrackExamService trackExamService)
    {
        _trackExamService = trackExamService;
    }

    //Create track exam
    [HttpPost]
    public async Task<IActionResult> CreateTrackExams(string userId, CreateTrackExamDto trackExamDto)
    {
        var result = await _trackExamService.CreateTrackExamAsync(userId, trackExamDto);
        return Ok(result);
    }

    //Get all track exam id
    [HttpGet("all-track-exams")]
    public async Task<IActionResult> GetAllTrackExams(string userId)
    {
        var result = await _trackExamService.GetAllTrackExamAsync(userId);
        return Ok(result);
    }

    //Delete track exam by id
    [HttpDelete("delete-track-exam")]
    public async Task<IActionResult> DeleteTrackExamById(string userId, string trackExamId)
    {
        var success = await _trackExamService.DeleteTrackExamByIdAsync(userId, trackExamId);
        if (!success)
        {
            return NotFound(new { Message = "TrackExam not found" });
        }
        return Ok(new { Message = "TrackExam deleted successfully" });
    }

    //Get track exam infomation
    [HttpGet("get-candidate-details")]
    public async Task<IActionResult> GetCandidateDetailsWithStatus(string organizeExamId, string sessionId, string roomId)
    {
        var candidateDetails = await _trackExamService.GetCandidateDetailsWithStatus(organizeExamId, sessionId, roomId);

        return Ok(candidateDetails);
    }
}
