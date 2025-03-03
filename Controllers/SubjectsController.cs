using backend_online_testing.Dtos;
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

        [HttpPost("/AddSubjectName/")]
        public async Task<ActionResult> AddSubjectName(string subjectName)
        {
            var result = await _subjectsService.AddSubject(subjectName);

            if (result == "Add subject successfully")
            {
                return Ok(new { message = result });
            }
            else
            {
                return BadRequest(new { error = result });
            }
        }

        [HttpPost("/{subjectId}/AddQuestionBank/")]
        public async Task<ActionResult> AddQuestionBankName(string subjectId, string questionBankName)
        {
            var result = await _subjectsService.AddQuestionBank(subjectId, questionBankName);

            if (result == "Add question bank successfully")
            {
                return Ok(new { message = result });
            }
            else
            {
                return BadRequest(new { error = result });
            }
        }

        [HttpPost("/{subjectId}/{questionBankId}/AddQuestionList")]
        public async Task<ActionResult> AddQuestionList(string subjectId, string questionBankId, string questionLogUserId, List<SubjectQuestionDto> listQuestion)
        {
            var result = await _subjectsService.AddQuestionsList(subjectId, questionBankId, questionLogUserId, listQuestion);

            if (result == "Add question list successfully")
            {
                return Ok(new { message = result });
            }
            else
            {
                return BadRequest(new { error = result });
            }
        }

        [HttpPost("Update/{subjectId}")]
        public async Task<ActionResult> UpdateSubjectName(string subjectId, string subjectName)
        {
            var result = await _subjectsService.UpdateSubjectName(subjectId, subjectName);

            if (result == "Update subject name successfully")
            {
                return Ok(new { message = result });
            }
            else
            {
                return BadRequest(new { error = result });
            }
        }

        [HttpPost("Update/{subjectId}/{questionBankId}")]
        public async Task<ActionResult> UpdateQuestionBankName(string subjectId,string questionBankId, string questionBankName)
        {
            var result = await _subjectsService.UpdateQuestionBankName(subjectId, questionBankId, questionBankName);

            if (result == "Update subject name successfully")
            {
                return Ok(new { message = result });
            }
            else
            {
                return BadRequest(new { error = result });
            }
        }

        [HttpPost("Update/{subjectId}/{questionBankId}/{questionId}")]
        public async Task<ActionResult> UpdateQuestion(string subjectId, string questionBankId, string questionId, string userLogId, SubjectQuestionDto questionData)
        {
            var result = await _subjectsService.UpdateQuestion(subjectId, questionBankId, questionId, userLogId, questionData);

            if (result == "Update question successfully")
            {
                return Ok(new { message = result });
            }
            else
            {
                return BadRequest(new { error = result });
            }
        }

        [HttpPost("DeleteSubject/{subjectId}")]
        public async Task<ActionResult> DeleteSubject(string subjectId)
        {
            var result = await _subjectsService.DeleteSubject(subjectId);

            if(result == "Delete subject successfully")
            {
                return Ok(new { message = result });
            }else
            {
                return BadRequest(new { message = result});
            }
        }

        [HttpPost("DeleteQuestionBank/{questionBankId}")]
        public async Task<ActionResult> DeleteQuestionBank(string subjectId, string questionBankId)
        {
            var result = await _subjectsService.DeleteQuestionBank(subjectId, questionBankId);

            if(result == "Delete question bank successfully")
            {
                return Ok(new { message = result });
            }else
            {
                return BadRequest(new { message = result });
            }
        }

        [HttpPost("DeleteQuestion/{questionId}")]
        public async Task<ActionResult> DeleteQuestion(string subjectId, string questionBankId, string questionId, string userLogId)
        {
            var result = await _subjectsService.DeleteQuestion(subjectId, questionBankId, questionId, userLogId);

            if(result == "Question deleted successfully")
            {
                return Ok(new { message = result });
            }else
            {
                return BadRequest(new { message = result });
            }
        }

        [HttpPost("SeedData/")]
        public async Task<IActionResult> InsertSampleData()
        {
            await _subjectsService.InsertSampleDataAsync();
            return Ok("Seed data is inserted successfully");
        }
    }
}
