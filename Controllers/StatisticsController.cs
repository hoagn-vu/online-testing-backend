using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
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

    [HttpGet("get-update-organize-exam-grade-stats")]
    public async Task<IActionResult> Get(string organizeExamId)
    {
        var doc = await _statisticsService.GetGradeStatisticSnapshotAsync(organizeExamId);
        if (doc is null) return NotFound(new { success = false, message = "Grade statistic not found" });
        return Ok(new { success = true, data = doc });
    }

    [HttpPost("update-organize-exam-grade-stats")]
    public async Task<IActionResult> Update(string organizeExamId)
    {
        if (string.IsNullOrWhiteSpace(organizeExamId))
            return BadRequest(new { success = false, message = "organizeExamId is required" });

        try
        {
            await _statisticsService.UpdateGradeStatisticAsync(organizeExamId);
            return Ok(new { success = true, message = "Grade statistic updated" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("get-participation-violation-by-id")]
    public async Task<IActionResult> GetViolation(string organizeExamId)
    {
        if (string.IsNullOrWhiteSpace(organizeExamId))
            return BadRequest(new { success = false, message = "organizeExamId is required" });

        try
        {
            var doc = await _statisticsService.GetParticipationViolationSnapshotAsync(organizeExamId);
            if (doc is null)
                return NotFound(new { success = false, message = "Violation snapshot not found" });

            return Ok(new { success = true, data = doc });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("update-participation-violation-by-id")]
    public async Task<IActionResult> UpdateViolation(string organizeExamId)
    {
        if (string.IsNullOrWhiteSpace(organizeExamId))
            return BadRequest(new { success = false, message = "organizeExamId is required" });

        try
        {
            await _statisticsService.UpdateParticipationViolationAsync(organizeExamId);
            return Ok(new { success = true, message = "Violation snapshot updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("exam-set")]
    public async Task<ActionResult<ExamSetStatisticDto>> GetExamSetStats(string organizeExamId)
    {
        try
        {
            var result = await _statisticsService.ExamSetSatisticAsync(organizeExamId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("organize-exams/exam-stats")]
    public async Task<ActionResult<ExamQuestionStatsResponse>> GetQuestionStats(string organizeExamId, string examId)
    {
        try
        {
            var data = await _statisticsService.GetExamQuestionStatsAsync(organizeExamId, examId);
            return Ok(data);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("organize-exam/ramdom-exam-status")]
    public async Task<IActionResult> UpdateStatistics(string organizeExamId)
    {
        if (string.IsNullOrWhiteSpace(organizeExamId))
            return BadRequest(new { success = false, message = "organizeExamId is required" });

        try
        {
            await _statisticsService.UpdateQuestionBankStatusAsync(organizeExamId);
            return Ok(new { success = true, message = "Statistic updated successfully" });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("OrganizeExam not found"))
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("organize-exam/ramdom-exam-status")]
    public async Task<IActionResult> GetStatistics(string organizeExamId)
    {
        if (string.IsNullOrWhiteSpace(organizeExamId))
            return BadRequest(new { success = false, message = "organizeExamId is required" });

        var doc = await _statisticsService.GetOrganizeExamStatisticAsync(organizeExamId);
        if (doc is null)
            return NotFound(new { success = false, message = "Statistic not found" });

        return Ok(new { success = true, data = doc });
    }


}
