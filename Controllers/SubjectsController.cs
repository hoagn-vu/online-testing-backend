#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using System.Runtime.CompilerServices;
    using Backend_online_testing.Dtos;
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/subjects")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly SubjectsService _subjectsService;

        public SubjectsController(SubjectsService subjectsService)
        {
            this._subjectsService = subjectsService;
        }

        // Get all subject
        [HttpGet]
        public async Task<ActionResult<List<SubjectsModel>>> GetAllSubjects(string? keyword, int page, int pageSize)
        {
            var subjects = await this._subjectsService.GetAllSubjects(keyword ?? string.Empty, page, pageSize);

            return this.Ok(subjects);
        }

        // Search by question bank name
        [HttpGet("question-bank")]
        public async Task<ActionResult<List<SubjectsModel>>> SearchByQuestionBankName(string subjectId, string? questionBankName, int page, int pageSize)
        {
            var result = await this._subjectsService.SearchByQuestionBankName(subjectId, questionBankName, page, pageSize);
            return this.Ok(result);
        }

        // Search by question name
        [HttpGet("question-name")]
        public async Task<ActionResult<List<SubjectsModel>>> SearchByQuestionName(string subjectId, string questionBankId, string? questionName, int page, int pageSize)
        {
            var result = await this._subjectsService.SearchByQuestionName(subjectId, questionBankId, questionName, page, pageSize);
            return this.Ok(result);
        }

        // Add subject
        [HttpPost("add-subject")]
        public async Task<ActionResult> AddSubjectName(string subjectName)
        {
            var result = await this._subjectsService.AddSubject(subjectName);

            if (result == "Add subject successfully")
            {
                return this.Ok(new { message = result });
            }
            else
            {
                return this.BadRequest(new { error = result });
            }
        }

        // Add question bank
        [HttpPost("add-question-bank")]
        public async Task<ActionResult> AddQuestionBankName(string subjectId, string questionBankName)
        {
            var result = await this._subjectsService.AddQuestionBank(subjectId, questionBankName);

            if (result == "Add question bank successfully")
            {
                return this.Ok(new { message = result });
            }
            else
            {
                return this.BadRequest(new { error = result });
            }
        }

        // Add question list
        [HttpPost("add-question-list")]
        public async Task<ActionResult> AddQuestionList(string subjectId, string questionBankId, string questionLogUserId, List<SubjectQuestionDto> listQuestion)
        {
            var result = await this._subjectsService.AddQuestionsList(subjectId, questionBankId, questionLogUserId, listQuestion);

            if (result == "Add question list successfully")
            {
                return this.Ok(new { message = result });
            }
            else
            {
                return this.BadRequest(new { error = result });
            }
        }

        // Update subject
        [HttpPost("update-subject")]
        public async Task<ActionResult> UpdateSubjectName(string subjectId, string subjectName)
        {
            var result = await this._subjectsService.UpdateSubjectName(subjectId, subjectName);

            if (result == "Update subject name successfully")
            {
                return this.Ok(new { message = result });
            }
            else
            {
                return this.BadRequest(new { error = result });
            }
        }

        // Update question bank
        [HttpPost("update-question-bank")]
        public async Task<ActionResult> UpdateQuestionBankName(string subjectId, string questionBankId, string questionBankName)
        {
            var result = await this._subjectsService.UpdateQuestionBankName(subjectId, questionBankId, questionBankName);

            if (result == "Update subject name successfully")
            {
                return this.Ok(new { message = result });
            }
            else
            {
                return this.BadRequest(new { error = result });
            }
        }

        // Update question Id
        [HttpPost("update-question")]
        public async Task<ActionResult> UpdateQuestion(string subjectId, string questionBankId, string questionId, string userLogId, SubjectQuestionDto questionData)
        {
            var result = await this._subjectsService.UpdateQuestion(subjectId, questionBankId, questionId, userLogId, questionData);

            if (result == "Update question successfully")
            {
                return this.Ok(new { message = result });
            }
            else
            {
                return this.BadRequest(new { error = result });
            }
        }

        // Delete Subject
        [HttpDelete("delete-subject")]
        public async Task<ActionResult> DeleteSubject(string subjectId)
        {
            var result = await this._subjectsService.DeleteSubject(subjectId);

            if (result == "Delete subject successfully")
            {
                return this.Ok(new { message = result });
            }
            else
            {
                return this.BadRequest(new { message = result });
            }
        }

        // Delete question bank
        [HttpDelete("delete-question-bank")]
        public async Task<ActionResult> DeleteQuestionBank(string subjectId, string questionBankId)
        {
            var result = await this._subjectsService.DeleteQuestionBank(subjectId, questionBankId);

            if (result == "Delete question bank successfully")
            {
                return this.Ok(new { message = result });
            }
            else
            {
                return this.BadRequest(new { message = result });
            }
        }

        // Delete question
        [HttpDelete("delete/question")]
        public async Task<ActionResult> DeleteQuestion(string subjectId, string questionBankId, string questionId, string userLogId)
        {
            var result = await this._subjectsService.DeleteQuestion(subjectId, questionBankId, questionId, userLogId);

            if (result == "Question deleted successfully")
            {
                return this.Ok(new { message = result });
            }
            else
            {
                return this.BadRequest(new { message = result });
            }
        }

        [HttpPost("seed")]
        public async Task<IActionResult> InsertSampleData()
        {
            await this._subjectsService.InsertSampleDataAsync();
            return this.Ok("Seed data is inserted successfully");
        }
    }
}
