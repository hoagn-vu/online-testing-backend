using backend_online_testing.Models;
using backend_online_testing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace backend_online_testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly SubjectsService _subjectsService;

        public SubjectsController(SubjectsService subjectsService)
        {
            _subjectsService = subjectsService;
        }

        [HttpGet("GetAllSubjects/")]
        public async Task<ActionResult<List<SubjectsModel>>> GetAllSubjects()
        {
            var subjects = await _subjectsService.GetAllSubjects();
            return Ok(subjects);
        }

        [HttpGet("/SearchBySubjectName/{subjectName}")]
        public async Task<ActionResult<List<SubjectsModel>>> SearchBySubjectName(string subjectName)
        {
            var result = await _subjectsService.SearchBySubjectName(subjectName);
            return Ok(result);
        }

        [HttpGet("/SearchByQuestionBank/{subjectName}/{questionBankName}")]
        public async Task<ActionResult<List<SubjectsModel>>> SearchByQuestionBankName(string subjectName, string questionBankName)
        {
            var result = await _subjectsService.SearchByQuestionBankName(subjectName, questionBankName);
            return Ok(result);
        }

        [HttpGet("/SearchByQuestionName/{subjectName}/{questionBankName}/{questionName}")]
        public async Task<ActionResult<List<SubjectsModel>>> SearchByQuestionName(string subjectName, string questionBankName, string questionName)
        {
            var result = await _subjectsService.SearchByQuestionName(subjectName, questionBankName, questionName);
            return Ok(result);
        }

        [HttpPost("SeedData/")]
        public async Task<IActionResult> InsertSampleData()
        {
            await _subjectsService.InsertSampleDataAsync();
            return Ok("Seed data is inserted successfully");
        }
    }
}
