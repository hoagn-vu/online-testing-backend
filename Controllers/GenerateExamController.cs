using Backend_online_testing.Dtos;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers
{
    [ApiController]
    [Route("api/generate-exam")]
    public class GenerateExamController : ControllerBase
    {
        private readonly IGenerateExamService _examService;

        public GenerateExamController(IGenerateExamService examService)
        {
            _examService = examService;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateExam([FromBody] GenerateExamRequestDto request)
        {
            var (status, response) = await _examService.GenerateExamAsync(request);
                
            return status switch
            {
                "error-organize-exam" => BadRequest(new
                {
                    code = status, message = "Không tìm thấy dữ liệu kỳ thi phù hợp"
                }),
                "error-subject" => BadRequest(new { code = status, message = "Không tìm thấy dữ liệu môn thi" }),
                "error-question-bank" => BadRequest(new
                {
                    code = status, message = "Không tìm thấy dữ liệu ngân hàng câu hỏi"
                }),
                "error-exam-set" => BadRequest(new { code = status, message = "Không tìm thấy danh sách đề thi" }),
                "error-exam-matrix" => BadRequest(new { code = status, message = "Không tìm thấy ma trận đề thi" }),
                "error-get-exam" => BadRequest(new { code = status, message = "Lỗi lấy dữ liệu đề thi" }),
                "error-user" => BadRequest(new { code = status, message = "Không tìm thấy dữ liệu người dùng" }),
                "error-take-exam" => BadRequest(new { code = status, message = "Không tìm thấy dữ liệu làm bài thi" }),
                "error-exam-type" => BadRequest(new { code = status, message = "Lỗi lấy dữ liệu loại bài thi" }),
                "exam-done" => Ok(new { code = status, message = "Bài thi đã hoàn thành" }),
                "success" => Ok(new {code = status, data = response})
            };
        }
    }
}
