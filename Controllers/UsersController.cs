using Backend_online_testing.Models;
using Backend_online_testing.Services;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
namespace Backend_online_testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _userService;

        public UsersController(UsersService userService)
        {
            _userService = userService;
        }

        //Get method
        [HttpGet]
        public async Task<IEnumerable<UsersModel>> Get()
        {
            return await _userService.GetAllUsers();
        }

        //Get method with ID
        [HttpGet("getUserById/{id}")]
        public async Task<ActionResult<UsersModel?>> GetById(string id)
        {
            return await _userService.GetUserById(id);
        }

        //Update Method
        [HttpPost("updateUserById/{id}")]
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
