namespace Backend_online_testing.Dtos;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string DateOfBirth { get; set; } = string.Empty;
    public List<string> GroupName { get; set; } = [];
    public string AccountStatus { get; set; } = string.Empty;
    public List<string> Authenticate { get; set; } = [];
}