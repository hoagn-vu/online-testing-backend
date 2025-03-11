using backend_online_testing.Models;
using backend_online_testing.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;

namespace backend_online_testing.Controllers
{
    public class FileManagementController : ControllerBase
    {
        private readonly FileManagementService _fileService;

        public FileManagementController(FileManagementService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload-file-question")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string subjectId, [FromQuery] string userLogId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file hợp lệ.");

            List<QuestionBanksModel> questionBanks;
            string result;

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    if (file.FileName.EndsWith(".txt"))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            //questionBanks = await _fileService.ProcessFileTxt(reader, subjectId, userLogId);
                            result = await _fileService.ProcessFileTxt(reader, subjectId, userLogId);
                        }
                    }
                    else if (file.FileName.EndsWith(".docx"))
                    {
                        //questionBanks = await _fileService.ProcessFileDocx(stream, subjectId, userLogId);
                        result = await _fileService.ProcessFileDocx(stream, subjectId, userLogId);
                    }
                    else
                    {
                        return BadRequest("Chỉ hỗ trợ file .txt và .docx.");
                    }
                }

                //return Ok(new { Message = "Tải lên thành công!", Data = questionBanks });
                if(result == "Insert question bank successfully")
                {
                    return Ok(new { message = "Insert question bank successfully" });
                }
                else
                {
                    return BadRequest(new {message = result});
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý file.");
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

                var users = await _fileService.UsersFileExcel(stream, userLogId);

                if (users == null)
                {
                    return BadRequest("File is uploaded");
                }

                return Ok(users);
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

                var result = await _fileService.GroupUser(stream, userLogId);
                return Ok(result);
            }
        }
    }
}
