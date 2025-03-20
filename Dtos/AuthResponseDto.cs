namespace Backend_online_testing.Dtos;

public class AuthResponseDto
{
    public string? Role { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
