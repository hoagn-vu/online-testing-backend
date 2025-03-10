using backend_online_testing.Models;
using backend_online_testing.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
namespace backend_online_testing.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _userService;

        public UsersController(UsersService userService)
        {
            _userService = userService;
        }

        //Get all user
        [HttpGet]
        public async Task<IEnumerable<UsersModel>> Get()
        {
            return await _userService.GetAllUsers();
        }

        //Get method with ID
        [HttpGet("{id}")]
        public async Task<ActionResult<UsersModel?>> GetById(string id)
        {
            return await _userService.GetUserById(id);
        }

        //Add on User
        [HttpPost]
        public async Task<ActionResult<UsersModel>> AddUser(UsersModel userData, string userLogId)
        {
            if (userData == null)
            {
                return BadRequest(new { message = "Invalid data" });
            }

            var result = await _userService.AddUser(userData, userLogId);

            if(result == "User is added successfully")
            {
                return Ok("User is add successfully");
            }

            return BadRequest(new { message = "Error: " + (result ?? "Unknown error") });
        }

        //Update Method
        [HttpPost("update/{id}")]
        public async Task<ActionResult> UpdateUserById(string id, [FromBody] UsersModel updateUser)
        {
            if (updateUser == null)
            {
                return BadRequest(new { message = "Invalid data" });
            }

            bool isUpdated = await _userService.UpdateUserbByID(id, updateUser);

            if (!isUpdated)
            {
                return NotFound(new { message = "User not found or not changes" });
            }
            return Ok(new { message = "User updated sucessfully" });
        }

        //Insert sample data
        [HttpPost("seed")]
        public async Task<IActionResult> SeedData()
        {
            await _userService.InsertSampleData();
            return Ok("Sample data inserted successfully");
        }
    }
}
