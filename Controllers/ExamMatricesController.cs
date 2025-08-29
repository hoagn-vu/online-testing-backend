using System.Security.Claims;

namespace Backend_online_testing.Controllers
{
    using Backend_online_testing.Dtos;
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/exam-matrices")]
    [ApiController]
    public class ExamMatricesController : ControllerBase
    {
        private readonly IExamMatricesService _examMatricesService;
        private readonly ILogsService _logService;

        public ExamMatricesController(IExamMatricesService examMatricesService,  ILogsService logService)
        {
            this._examMatricesService = examMatricesService;
            _logService = logService;
        }

        // Get all exam matrix
        [HttpGet]
        public async Task<IActionResult> GetExamMatrices([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (examMatrices, totalCount) = await _examMatricesService.GetExamMatrices(keyword, page, pageSize);
            
            await _logService.WriteLogAsync(new CreateLogDto
            {
                MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                LogAction = "get-matrix",
                LogDetails = $"Truy cập danh sách ma trận đề thi",
            });

            return Ok(new { examMatrices, totalCount });
        }

        // Get by Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdExamMatrix(string id)
        {
            var result = await this._examMatricesService.GetByIdExamMatrix(id);

            if (result == null)
            {
                return NotFound(new { message = "Exam matrix not found" });
            }

            await _logService.WriteLogAsync(new CreateLogDto
            {
                MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                LogAction = "get-matrix_detail",
                LogDetails = $"Truy cập chi tiết ma trận đề thi \"{result.MatrixName}\"",
            });
            return this.Ok(new { status = "Success", data = result });
        }

        // Add exam matrix
        [HttpPost]
        public async Task<IActionResult> AddExamMatrix([FromBody] ExamMatrixRequestDto examMatrixData)
        {
            var result = await _examMatricesService.AddExamMatrix(examMatrixData);

            if (result == "Tạo ma trận thành công")
            {
                await _logService.WriteLogAsync(new CreateLogDto
                {
                    MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                    LogAction = "post-matrix",
                    LogDetails = $"Tạo ma trận đề thi \"{examMatrixData.MatrixName}\"",
                });
                return Ok(new { status = "Success", message = result });
            }

            return BadRequest(new { status = "Failure", message = result });
        }

        // // Add tag
        // [HttpPost("tags/")]
        // public async Task<IActionResult> AddTag([FromBody] ExamMatrixAddDto tagsData)
        // {
        //     var result = await this._examMatricesService.AddTag(tagsData);
        //     if (result == "Tags added successfully")
        //     {
        //         return this.Ok(new { status = "Success", message = result });
        //     }
        //
        //     return this.BadRequest(new { status = "Failure", message = result });
        // }

        // Update exam matrix
        [HttpPut("{examMatrixId}")]
        public async Task<IActionResult> UpdateMatrixExam(string examMatrixId, [FromBody] ExamMatrixUpdateDto examMatrixData)
        {
            var (matrixName, result) = await this._examMatricesService.UpdateExamMatrix(examMatrixId, examMatrixData);
            if (result == "Exam matrix updated successfully")
            {
                await _logService.WriteLogAsync(new CreateLogDto
                {
                    MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                    LogAction = "post-matrix",
                    LogDetails = $"Tạo ma trận đề thi \"{matrixName}\"",
                });
                return this.Ok(new { status = "Success", message = result });
            }

            return this.BadRequest(new { status = "Failure", message = result });
        }

        // // Update tag
        // [HttpPost("{examMatrixId}/tags/{tagName}")]
        // public async Task<IActionResult> UpdateTags(string examMatrixId, string tagName, int questionCount, double tagScore)
        // {
        //     var result = await this._examMatricesService.UpdateTag(examMatrixId, tagName, questionCount, tagScore);
        //     if (result == "Tags updated successfully")
        //     {
        //         return this.Ok(new { status = "Success", message = result });
        //     }
        //
        //     return this.BadRequest(new { status = "Failure", message = result });
        // }
        //
        // [HttpDelete("delete-exam-matrix/{examMatrixId}")]
        // public async Task<IActionResult> DeleteMatrixExam(string examMatrixId, string matrixLogUserId)
        // {
        //     var result = await this._examMatricesService.DeleteExamMatrix(examMatrixId, matrixLogUserId);
        //     if (result == "Exam matrix marked as unavailable")
        //     {
        //         return this.Ok(new { status = "Success", message = result });
        //     }
        //
        //     return this.BadRequest(new { status = "Failure", message = result });
        // }
        //
        // [HttpDelete("delete-tag/{examMatrixId}/{tagName}")]
        // public async Task<IActionResult> DeleteTagExam(string examMatrixId, string tagName, string matrixLogUserId)
        // {
        //     var result = await this._examMatricesService.DeleteTag(examMatrixId, tagName, matrixLogUserId);
        //     if (result == "Tag deleted successfully")
        //     {
        //         return this.Ok(new { status = "Success", message = result });
        //     }
        //
        //     return this.BadRequest(new { status = "Failure", message = result });
        // }
        //
        // // Insert form data
        // [HttpGet("seed")]
        // public async Task<IActionResult> SeedData()
        // {
        //     await this._examMatricesService.SeedData();
        //     return new OkObjectResult(new { status = "Success", message = "Example exam matrix data seeded successfully." });
        // }
        
        [HttpGet("options")]
        public async Task<IActionResult> GetMatrixOptions([FromQuery] string subjectId, [FromQuery] string? questionBankId)
        {
            var (status, result) = await _examMatricesService.GetMatrixOptions(subjectId, questionBankId);

            if (status != "ok")
            {
                return BadRequest(new { message = "Không thể lấy dữ liệu ma trận." });
            }

            return Ok(result);
        }
        
        [HttpPost("generate-exam")]
        public async Task<IActionResult> Generate([FromBody] GenerateExamByMatrixRequestDto request)
        {
            var (status, matrixName, result) = await _examMatricesService.GenerateExamAsync(request);

            if (status == "success" && result != null)
            {
                foreach (var ex in result)
                {
                    await _logService.WriteLogAsync(new CreateLogDto
                    {
                        MadeBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous",
                        LogAction = "post-matrix_generate_exam",
                        LogDetails = $"Tạo đề thi \"{ex.ExamName}\" - \"{ex.ExamCode}\" ma trận đề thi \"{matrixName}\"",
                    });
                }
            }

            return status switch
            {
                "success" => Ok(result),

                "exam-matrix-not-found" => NotFound(new
                {
                    code = status,
                    message = "Exam matrix not found"
                }),

                "subject-not-found" => NotFound(new
                {
                    code = status,
                    message = "Subject not found"
                }),

                "question-bank-not-found" => NotFound(new
                {
                    code = status,
                    message = "Question bank not found"
                }),

                _ when status.StartsWith("not-enough-questions") => BadRequest(new
                {
                    code = "not-enough-questions",
                    message = "Not enough questions available in the question bank to satisfy matrix requirement",
                    details = status
                }),

                _ => BadRequest(new
                {
                    code = "error",
                    message = "Unknown error occurred"
                })
            };
        }
    }
}
