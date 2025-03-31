using Backend_online_testing.Dtos;
using Backend_online_testing.Models;

namespace Backend_online_testing.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Authenticate(string username, string password);
        Task<AuthResponseDto?> RefreshToken(string refreshToken);
        Task<UserDto?> GetUserProfile(string userId);
    }
}
