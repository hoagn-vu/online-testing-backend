#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Bson;

    [Route("api/auth")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        // Hiện chưa có tài khoản nên dùng Id sau để test tạm
        private ObjectId tempId = ObjectId.Parse("67b75fc3e54f92629f3d7378");

        public UserController(UserService roomService)
        {
            this._userService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => this.Ok(await this._userService.GetUsersAsync());

        [HttpPost]
        public async Task<IActionResult> Create(UserModel user)
        {
            await this._userService.CreateAsync(user, this.tempId);
            return this.CreatedAtAction(nameof(this.Get), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await this._userService.AuthenticateAsync(request.Username, request.Password);
            if (user == null)
            {
                return this.Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });
            }

            return this.Ok(new { message = "Đăng nhập thành công", userId = user.Id, userFullname = user.FullName });
        }
    }
}
