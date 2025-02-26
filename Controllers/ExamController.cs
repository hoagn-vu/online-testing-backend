using backend_online_testing.DTO;
using backend_online_testing.Models;
using backend_online_testing.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace backend_online_testing.Controllers
{
    [Route("api/[controller]")]
    public class ExamController : ControllerBase
    {
        private readonly ExamsService _examsService;

        public ExamController(ExamsService examsService)
        {
            _examsService = examsService;
        }

        [HttpGet("GetAllExam")]
        public async Task<IActionResult> GetAllExam()
        {
            var exams_list = await _examsService.FindExam();
            if(exams_list == null || !exams_list.Any()){
                return NotFound(new { message = "No exam founds" });
            }
            return Ok(new { status ="Success" , data = exams_list });
        }

        [HttpPost("SearchByName")]
        public async Task<IActionResult> SearchByName([FromBody] string examName)
        {
            var exams_list = await _examsService.FindExamByName(examName);
            if (exams_list == null || !exams_list.Any())
            {
                return NotFound(new { message = "No exam founds" });
            }
            return Ok(new { message = "Success", exams_list });
        }

        [HttpPost("CreatExam")]
        public async Task<IActionResult> CreateExam([FromBody] ExamDTO createExamData)
        {
            if (createExamData == null)
            {
                return BadRequest(new { Message = "Invalid request data." });
            }

            string result = await _examsService.CreateExam(createExamData);

            if (result == "Exam name already exists.")
            {
                return Conflict(new { Message = result }); 
            }
            else if (result == "Exam created successfully.")
            {
                return Ok(new { Message = result });
            }
            else
            {
                return StatusCode(500, new { Message = result }); 
            }
        }

        [HttpPost("UpdateExam")]
        public async Task<IActionResult> UpdateExam([FromBody] ExamDTO updateExamData)
        {
            if (updateExamData == null || string.IsNullOrEmpty(updateExamData.Id))
            {
                return BadRequest(new { status = "Error", message = "Invalid exam data provided." });
            }

            //var existingExam = await _examsService.FindExamById(updateExamData.Id);
            //if (existingExam == null)
            //{
            //    return NotFound(new { status = "Error", message = "Exam not found." });
            //}

            bool updateSuccess = await _examsService.UpdateExam(updateExamData);

            if (updateSuccess)
            {
                return Ok(new { status = "Success", message = "Exam updated successfully." });
            }

            return NotFound(new { status = "Error", message = "Exam update failed." });
        }

        [HttpPost("AddQuestion")]
        public async Task<IActionResult> AddQuestionExam([FromBody] ExamQuestionDTO addQuestionData)
        {
            string addStatus = await _examsService.AddExamQuestion(addQuestionData);

            if(addStatus == "Question added successfully")
            {
                return Ok(new { status = "Success", message = "Added question successfully." });
            }
            return BadRequest(new { status = "Failed", message = addStatus });
        }

        [HttpPut("UpdateQuestion/{Id}")]
        public async Task<IActionResult> UpdateQuestionExam(string Id, [FromBody] ExamQuestionDTO addQuestionData)
        {
            string updateStatus = await _examsService.UpdateExamQuestion(addQuestionData);

            if (updateStatus == "Question updated successfully")
            {
                return Ok(new { status = "Success", message = "Update question successfully." });
            }
            return BadRequest(new { status = "Failed", message = updateStatus });
        }
        [HttpPost("DeleteQuestion")]
        public async Task<IActionResult> DeleteQuestionExam([FromBody] ExamQuestionDTO deleteQuestionData)
        {
            string deleteStatus = await _examsService.DeleteExamQuestion(deleteQuestionData);
            if(deleteStatus == "Question deleted successfully")
            {
                return Ok(new { status = "Success", message = "Delete question successfully." });
            }
            return BadRequest(new { status = "Failed", message = deleteStatus });
        }

        [HttpGet("SeedData")]
        public async Task<IActionResult> SeedData()
        {
            await _examsService.SeedData();
            return new OkObjectResult(new { status = "Success", message = "Example exam data seeded successfully." });
        }
    }
}
