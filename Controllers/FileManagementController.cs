#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using System.Security.Cryptography.X509Certificates;
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Mvc;

    public class FileManagementController : ControllerBase
    {
        private readonly FileManagementService _fileService;

        public FileManagementController(FileManagementService fileService)
        {
            this._fileService = fileService;
        }

        [HttpPost("upload-file-question")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string subjectId, [FromQuery] string userLogId)
        {
            if (file == null || file.Length == 0)
            {
                return this.BadRequest("Vui lòng chọn file hợp lệ.");
            }

            string result;

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    if (file.FileName.EndsWith(".txt"))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            result = await this._fileService.ProcessFileTxt(reader, subjectId, userLogId);
                        }
                    }
                    else if (file.FileName.EndsWith(".docx"))
                    {
                        result = await this._fileService.ProcessFileDocx(stream, subjectId, userLogId);
                    }
                    else
                    {
                        return this.BadRequest("Chỉ hỗ trợ file .txt và .docx.");
                    }
                }

                if
                (result == "Insert question bank successfully")
                {
                    return this.Ok(new { message = "Insert question bank successfully" });
                }
                else
                {
                    return this.BadRequest(new { message = result });
                }
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("upload-file-user")]
        public async Task<IActionResult> UploadFileUser(IFormFile file, [FromQuery] string userLogId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var users = await this._fileService.UsersFileExcel(stream, userLogId);

                if (users == null)
                {
                    return this.BadRequest("File is uploaded");
                }

                return this.Ok(users);
            }
        }

        [HttpPost("upload-file-user-group")]
        public async Task<IActionResult> UploadFileUserGroup(IFormFile file, [FromQuery] string userLogId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty or null");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var result = await this._fileService.GroupUser(stream, userLogId);
                return this.Ok(result);
            }
        }
    }
}
