using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Services;

namespace Backend_online_testing.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    public class ErrorResponseDto
    {
        public string Message { get; set; }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var authResponse = await _authService.Authenticate(dto.UserName, dto.Password);
        if (authResponse == null)
            return Unauthorized(new ErrorResponseDto { Message = "Tài khoản hoặc mật khẩu không chính xác" });

        return Ok(authResponse);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var token = Request.Headers.Authorization.ToString();
        Console.WriteLine($"Token nhận được: {token}");
        
        Console.WriteLine("Claims của user:");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"UserId: {userId}");
        if (userId == null)
            return Unauthorized();

        var user = await _authService.GetUserProfile(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(user);
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        var authResponse = await _authService.RefreshToken(request.RefreshToken);
        if (authResponse == null)
            return Unauthorized(new { message = "Invalid refresh token" });

        return Ok(authResponse);
    }
}