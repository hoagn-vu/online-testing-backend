#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using Backend_online_testing.DTO;
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/exams")]
    public class ExamController : ControllerBase
    {
        private readonly ExamsService _examsService;

        public ExamController(ExamsService examsService)
        {
            this._examsService = examsService;
        }

        // Get all Exam
        [HttpGet]
        public async Task<IActionResult> GetAllExam()
        {
            var exams_list = await this._examsService.FindExam();
            if (exams_list == null || !exams_list.Any())
            {
                return this.NotFound(new { message = "No exam founds" });
            }

            return this.Ok(new { status = "Success", data = exams_list });
        }

        // Search by name
        [HttpPost("search")]
        public async Task<IActionResult> SearchByName(string name, [FromBody] string examName)
        {
            var exams_list = await this._examsService.FindExamByName(examName);
            if (exams_list == null || !exams_list.Any())
            {
                return this.NotFound(new { message = "No exam founds" });
            }

            return this.Ok(new { message = "Success", exams_list });
        }

        // Create Exam
        [HttpPost]
        public async Task<IActionResult> CreateExam([FromBody] ExamDTO createExamData)
        {
            if (createExamData == null)
            {
                return this.BadRequest(new { Message = "Invalid request data." });
            }

            string result = await this._examsService.CreateExam(createExamData);

            if (result == "Exam name already exists.")
            {
                return this.Conflict(new { Message = result });
            }
            else if (result == "Exam created successfully.")
            {
                return this.Ok(new { Message = result });
            }
            else
            {
                return this.StatusCode(500, new { Message = result });
            }
        }

        // Update exam
        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateExam(string id, [FromBody] ExamDTO updateExamData)
        {
            if (updateExamData == null || string.IsNullOrEmpty(updateExamData.Id))
            {
                return this.BadRequest(new { status = "Error", message = "Invalid exam data provided." });
            }

            bool updateSuccess = await this._examsService.UpdateExam(updateExamData);

            if (updateSuccess)
            {
                return this.Ok(new { status = "Success", message = "Exam updated successfully." });
            }

            return this.NotFound(new { status = "Error", message = "Exam update failed." });
        }

        // Add questions to Exam
        [HttpPost("{examId}/questions)")]
        public async Task<IActionResult> AddQuestionExam([FromBody] ExamQuestionDTO addQuestionData)
        {
            string addStatus = await this._examsService.AddExamQuestion(addQuestionData);

            if (addStatus == "Question added successfully")
            {
                return this.Ok(new { status = "Success", message = "Added question successfully." });
            }

            return this.BadRequest(new { status = "Failed", message = addStatus });
        }

        // Update a question to exam
        [HttpPost("questions/{questionId}")]
        public async Task<IActionResult> UpdateQuestionExam(string id, [FromBody] ExamQuestionDTO addQuestionData)
        {
            string updateStatus = await this._examsService.UpdateExamQuestion(addQuestionData);

            if (updateStatus == "Question updated successfully")
            {
                return this.Ok(new { status = "Success", message = "Update question successfully." });
            }

            return this.BadRequest(new { status = "Failed", message = updateStatus });
        }

        // Delete question in Exam
        [HttpDelete("questions/{questionId}")]
        public async Task<IActionResult> DeleteQuestionExam(string id, [FromBody] ExamQuestionDTO deleteQuestionData)
        {
            string deleteStatus = await this._examsService.DeleteExamQuestion(deleteQuestionData);
            if (deleteStatus == "Question deleted successfully")
            {
                return this.Ok(new { status = "Success", message = "Delete question successfully." });
            }

            return this.BadRequest(new { status = "Failed", message = deleteStatus });
        }

        // Insert data to database
        [HttpGet("seed")]
        public async Task<IActionResult> SeedData()
        {
            await this._examsService.SeedData();
            return new OkObjectResult(new { status = "Success", message = "Example exam data seeded successfully." });
        }
    }
}
