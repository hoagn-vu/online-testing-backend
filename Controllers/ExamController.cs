using System.Security.Claims;

#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using Backend_online_testing.DTO;
    using Backend_online_testing.Dtos;
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/exams")]
    [ApiController]
    public class ExamController : ControllerBase
    {
        private readonly ExamsService _examsService;
        private readonly ILogsService _logService;

        public ExamController(ExamsService examsService, ILogsService logService)
        {
            this._examsService = examsService;
            _logService = logService;
        }

        // Get all Exam
        [HttpGet]
        public async Task<IActionResult> GetExams([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (exams, totalCount) = await _examsService.GetExams(keyword, page, pageSize);
            
            await _logService.WriteLogAsync(new CreateLogDto
            {
                MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                LogAction = "get-exam",
                LogDetails = $"Truy cập danh sách đề thi",
            });

            return Ok(new { exams, totalCount });
        }

        [HttpGet("questions")]
        public async Task<IActionResult> GetExamQuestionsByCode([FromQuery] string examId)
        {
            var (status, detailedQuestions) = await _examsService.GetExamQuestionsWithDetailsAsync(examId);

            if (status == "error-exam")
                return NotFound(new { message = "Không tìm thấy đề thi" });
            if (status == "error-subject")
                return NotFound(new { message = "Không tìm thấy phân môn" });
            if (status == "error-questionBank")
                return NotFound(new { message = "Không tìm thấy bộ đề thi" });
            
            await _logService.WriteLogAsync(new CreateLogDto
            {
                MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                LogAction = "get-exam_detail",
                LogDetails = $"Truy cập chi tiết đề thi \"{detailedQuestions?.ExamName}\" - \"{detailedQuestions?.ExamCode}\"",
            });
            return Ok(detailedQuestions);
        }

        // Create Exam //Add multi question when add an exam
        [HttpPost]
        public async Task<IActionResult> CreateExam([FromBody] ExamDto createExamData)
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
                await _logService.WriteLogAsync(new CreateLogDto
                {
                    MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                    LogAction = "post-exam",
                    LogDetails = $"Tạo đề thi \"{createExamData.ExamName}\" - \"{createExamData.ExamCode}\"",
                });
                return this.Ok(new { Message = result });
            }
            else
            {
                return this.StatusCode(500, new { Message = result });
            }
        }

        // Add questions to Exam
        [HttpPost("{examId}/questions")]    
        public async Task<IActionResult> AddQuestionExam(string examId, [FromBody] ExamQuestionDTO addQuestionData, string userLogId)
        {
            var (examCode, examName, addStatus) = await this._examsService.AddExamQuestion(addQuestionData, examId, userLogId);

            if (addStatus == "Question added successfully")
            {
                await _logService.WriteLogAsync(new CreateLogDto
                {
                    MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                    LogAction = "put-exam_question",
                    LogDetails = $"Thêm câu hỏi vào đề thi \"{examCode}\" - \"{examName}\"",
                });
                
                return this.Ok(new { status = "Success", message = "Added question successfully." });
            }

            return this.BadRequest(new { status = "Failed", message = addStatus });
        }

        // Update exam
        [HttpPut("{examId}")]
        public async Task<IActionResult> UpdateExam(string examId, [FromBody] ExamDto updateExamData)
        {
            if (updateExamData == null || string.IsNullOrEmpty(examId))
            {
                return this.BadRequest(new { status = "Error", message = "Invalid exam data provided." });
            }

            bool updateSuccess = await this._examsService.UpdateExam(updateExamData, examId);

            if (updateSuccess)
            {
                await _logService.WriteLogAsync(new CreateLogDto
                {
                    MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                    LogAction = "put-exam",
                    LogDetails = $"Cập nhật đề thi \"{updateExamData.ExamName}\" - \"{updateExamData.ExamCode}\"",
                });
                
                return this.Ok(new { status = "Success", message = "Exam updated successfully." });
            }

            return this.NotFound(new { status = "Error", message = "Exam update failed." });
        }

        // Update a question to exam
        [HttpPost("questions/{questionId}")]
        public async Task<IActionResult> UpdateQuestionExam(string examId, string questionId, string userLogId, double questionScore)
        {
            string updateStatus = await this._examsService.UpdateExamQuestion(examId, questionId, userLogId, questionScore);

            if (updateStatus == "Question updated successfully")
            {
                return this.Ok(new { status = "Success", message = "Update question successfully." });
            }

            return this.BadRequest(new { status = "Failed", message = updateStatus });
        }

        // Delete question in Exam
        [HttpDelete("questions/{questionId}")]
        public async Task<IActionResult> DeleteQuestionExam(string examId, string questionId, string userLogId)
        {
            string deleteStatus = await this._examsService.DeleteExamQuestion(examId, questionId, userLogId);
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

        [HttpGet("options")]
        public async Task<IActionResult> GetOptions([FromQuery] string? subjectId, [FromQuery] string? questionBankId)
        {
            var result = await _examsService.GetExamOptionsAsync(subjectId, questionBankId);
            return new OkObjectResult(new { status = "Success", data = result });
        }
    }
}
