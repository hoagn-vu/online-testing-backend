#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using DocumentFormat.OpenXml.Spreadsheet;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Backend_online_testing.Dtos;

    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _userService;

        public UsersController(UsersService userService)
        {
            this._userService = userService;
        }

        // Get all user
        [HttpGet]
        public async Task<IActionResult> Get(string? keyword, int page, int pageSize, string? role)
        {
            var (users, total) = await this._userService.GetAllUsers(keyword, page, pageSize, role);
            return this.Ok(new { users, total });
        }   
        
        [HttpGet("get-by-role")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            var users = await _userService.GetUsersByRole(role);
            return this.Ok(users);
        } 

        // Get method with ID
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto?>> GetById(string id)
        {
            return await _userService.GetUserByIdAsync(id);
        }
        
        // Add on User
        [HttpPost]
        public async Task<ActionResult<UsersModel>> AddUser(CreateOrUpdateUserDto userData)
        {
            if (userData == null)
            {
                return this.BadRequest(new { message = "Invalid data" });
            }

            var result = await _userService.AddUser(userData);

            if (result == "User is added successfully")
            {
                return this.Ok("User is add successfully");
            }

            return this.BadRequest(new { message = "Error: " + (result ?? "Unknown error") });
        }

        // Update Method
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserById([FromRoute] string id, [FromBody] CreateOrUpdateUserDto updateUser)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { status = "Error", message = "UserId is required" });

            var result = await _userService.UpdateUserById(id, updateUser);

            if (result == "Success")
                return Ok(new { status = "Success", message = "User updated successfully" });

            if (result.StartsWith("Update user error"))
                return NotFound(new { status = "Error", message = result });

            return StatusCode(500, new { status = "Error", message = result });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUserById(string id)
        {

            var result = await _userService.DeleteUserById(id, "");

            if (result == "Success")
            {
                return Ok(new { message = "User deleted successfully" });
            }

            return NotFound(new { message = result });
        }

        [HttpGet("get-review-exam-user")]
        public async Task<ActionResult<ExamReviewDto>> GetReviewExam( string userId, string organizeExamId, string sessionId, string roomId)
        {
            try
            {
                var dto = await _userService.GetExamReviewAsync(
                    userId, organizeExamId, sessionId, roomId);
                return Ok(dto); 
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound
                }); 
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Server Error",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        [HttpGet("resume-exam")]
        public async Task<ActionResult<ResumeExamResponse>> Resume(string userId, string organizeExamId, string sessionId, string roomId)
        {
            var result = await _userService.ResumeAsync(userId, organizeExamId, roomId, sessionId);
            return Ok(result);
        }
    }
}
