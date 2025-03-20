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
        public async Task<IActionResult> GetSubjects([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (subjects, totalCount) = await this._subjectsService.GetSubjects(keyword ?? string.Empty, page, pageSize);

            return Ok(new { subjects, totalCount });
        }

        // Search by question bank name
        [HttpGet("question-banks")]
        public async Task<ActionResult<List<SubjectsModel>>> GetQuestionBanks(string subId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (subjectId, subjectName, questionBanks, totalCount)  = await this._subjectsService.GetQuestionBanks(subId, keyword, page, pageSize);
            return this.Ok(new { subjectId, subjectName, questionBanks, totalCount });
        }

        // Search by question name
        [HttpGet("questions")]
        public async Task<ActionResult<List<SubjectsModel>>> GetQuestions(string subId, string qbId, [FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (subjectId, subjectName, questionBankId, questionBankName, questions, totalCount) = await this._subjectsService.GetQuestions(subId, qbId, keyword, page, pageSize);
            return this.Ok( new { subjectId, subjectName, questionBankId, questionBankName, questions, totalCount });
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
        [HttpPost("add-question")]
        public async Task<ActionResult> AddQuestion([FromQuery] string subjectId, [FromQuery] string questionBankId, [FromQuery] string userId, [FromBody] SubjectQuestionDto question)
        {
            var result = await this._subjectsService.AddQuestion(subjectId, questionBankId, userId, question);

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
