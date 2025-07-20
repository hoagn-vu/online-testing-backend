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
        public async Task<IActionResult> Get(string? keyword, int page, int pageSize)
        {
            var (users, total) = await this._userService.GetAllUsers(keyword, page, pageSize);
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
        public async Task<ActionResult<UsersModel>> AddUser(UsersModel userData, string userLogId)
        {
            if (userData == null)
            {
                return this.BadRequest(new { message = "Invalid data" });
            }

            var result = await this._userService.AddUser(userData, userLogId);

            if (result == "User is added successfully")
            {
                return this.Ok("User is add successfully");
            }

            return this.BadRequest(new { message = "Error: " + (result ?? "Unknown error") });
        }

        // Update Method
        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateUserById(string id, [FromBody] UsersModel updateUser)
        {
            if (updateUser == null)
            {
                return BadRequest(new { message = "Invalid user data" });
            }

            string result = await _userService.UpdateUserById(id, updateUser, "");

            return result switch
            {
                "Success" => Ok(new { message = "User updated successfully" }),
                "Update user error" => NotFound(new { message = "User not found or no changes were made" }),
                _ => StatusCode(500, new { message = "Something went wrong" }) 
            };
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

    }
}
