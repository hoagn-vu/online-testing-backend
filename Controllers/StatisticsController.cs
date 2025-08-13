using Backend_online_testing.Services;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers;

[Route("api/statistics")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly StatisticsService _statisticsService;

    public StatisticsController(StatisticsService statisticService)
    {
        _statisticsService = statisticService;
    }

    [HttpGet("organize-exam-by-id")]
    public async Task<IActionResult> StatisticsOrganizeExam(string organizeExamId)
    {
        if (string.IsNullOrWhiteSpace(organizeExamId))
            return BadRequest("organizeExamId is required.");

        var result = await _statisticsService.GetScoreHistogram10Async(organizeExamId);
        return Ok(result);
    }

    [HttpGet("participation-violation-by-id")]
    public async Task<IActionResult> GetParticipationViolation(string organizeExamId)
    {
        if (string.IsNullOrWhiteSpace(organizeExamId))
            return BadRequest("organizeExamId is required.");

        var result = await _statisticsService.GetParticipationViolationAsync(organizeExamId);
        return Ok(result);
    }
}
