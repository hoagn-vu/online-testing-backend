using Backend_online_testing.Dtos;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers
{
    [ApiController]
    [Route("api/sumbit-answer")]
    public class SubmitAnswerController : ControllerBase
    {
        private readonly ISubmitAnswerService _service;

        public SubmitAnswerController(ISubmitAnswerService service)
        {
            _service = service;
        }

        [HttpPost("{userId}/{takeExamId}/{type}")]
        public async Task<IActionResult> SubmitAnswer(
            string userId,
            string takeExamId,
            string type,
            [FromBody] SubmitAnswerDto? request)
        {
            var (status, message) = await _service.HandleAnswerAsync(userId, takeExamId, type, request);

            if (status is "saved" or "submitted") return Ok(new { status, message });
            return BadRequest(new { status, message });
        }
    }
}