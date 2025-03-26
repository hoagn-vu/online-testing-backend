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
        public async Task<ActionResult<UsersModel?>> GetById(string id)
        {
            return await this._userService.GetUserById(id);
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
        public async Task<ActionResult> UpdateUserById(string id, [FromBody] UsersModel updateUser)
        {
            if (updateUser == null)
            {
                return this.BadRequest(new { message = "Invalid data" });
            }

            bool isUpdated = await this._userService.UpdateUserbByID(id, updateUser);

            if (!isUpdated)
            {
                return this.NotFound(new { message = "User not found or not changes" });
            }

            return this.Ok(new { message = "User updated sucessfully" });
        }

        // Insert sample data
        [HttpPost("seed")]
        public async Task<IActionResult> SeedData()
        {
            await this._userService.InsertSampleData();
            return this.Ok("Sample data inserted successfully");
        }
    }
}
