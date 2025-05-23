﻿namespace Backend_online_testing.Controllers
{
    using Services;
    using Microsoft.AspNetCore.Mvc;
    [Route("api/file")]
    [ApiController]
    public class FileManagementController : ControllerBase
    {
        private readonly IFileManagementService _fileService;

        public FileManagementController(IFileManagementService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload-file-question")]
        public async Task<IActionResult> UploadFile(IFormFile? file, [FromQuery] string subjectId, [FromQuery] string questionBankId)
        {
            if (file == null || file.Length == 0)
            {
                return this.BadRequest("Vui lòng chọn file hợp lệ.");
            }

            // Kiểm tra kích thước file (3MB = 3 * 1024 * 1024 bytes)
            const int maxFileSize = 2 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest("Kích thước file không được vượt quá 2MB.");
            }

            try
            {
                string result;
                await using (var stream = file.OpenReadStream())
                {
                    if (file.FileName.EndsWith(".txt"))
                    {
                        using var reader = new StreamReader(stream);
                        result = await this._fileService.ProcessFileTxt(reader, subjectId, questionBankId);
                    }
                    else if (file.FileName.EndsWith(".docx"))
                    {
                        result = await this._fileService.ProcessFileDocx(stream, subjectId, questionBankId);
                    }
                    else
                    {
                        return this.BadRequest("Chỉ hỗ trợ file .txt và .docx.");
                    }
                }

                if (result == "Tải tệp câu hỏi thành công")
                {
                    return this.Ok(new { message = "Tải tệp câu hỏi thành công" });
                }
                else
                {
                    return this.BadRequest(new {result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = 500,
                    message = $"Error: {ex.Message}",
                    error = "FileProcessingError"
                });
            }
        }

        [HttpPost("upload-file-user")]
        public async Task<IActionResult> UploadFileUser(IFormFile file, [FromQuery] string userLogId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File rỗng không hợp lệ");
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            var users = await _fileService.UsersFileExcel(stream, userLogId);

            return Ok(users);
        }

        [HttpPost("upload-file-user-group")]
        public async Task<IActionResult> UploadFileUserGroup(IFormFile file, [FromQuery] string userLogId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File rỗng không hợp lệ");
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            var result = await _fileService.GroupUser(stream, userLogId);
            return Ok(result);
        }
    }
}
