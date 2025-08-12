namespace Backend_online_testing.Dtos;
public class GroupUserDto
{
}

public class GroupUserCreateDto
{
    public required string GroupName { get; set;  }

    public List<string> ListUser { get; set; } = new();
}

public class GroupUserInfoDto
{
    public string UserId { get; set; } = string.Empty;

    public string UserCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? Gender { get; set; } = string.Empty;

    public string? DateOfBirth { get; set; }
}
