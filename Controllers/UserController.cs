using backend_online_testing.Models;
using backend_online_testing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace backend_online_testing.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        // Hiện chưa có tài khoản nên dùng Id sau để test tạm
        public ObjectId tempId = ObjectId.Parse("67b75fc3e54f92629f3d7378");

        public UserController(UserService roomService)
        {
            _userService = roomService;
        }


        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _userService.GetUsersAsync());

        [HttpPost]
        public async Task<IActionResult> Create(UserModel user)
        {
            await _userService.CreateAsync(user, tempId);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }
    }
}
