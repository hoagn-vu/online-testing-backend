using Backend_online_testing.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers
{
    public class UploadController : ControllerBase
    {
        private readonly S3Service _s3Service;

        public UploadController(S3Service s3Service)
        {
            _s3Service = s3Service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ");

            var url = await _s3Service.UploadFileAsync(file);
            return Ok(new { url });
        }
    }
}
