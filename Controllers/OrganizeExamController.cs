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
}