namespace Backend_online_testing.Dtos;
public class GroupUserDto
{
}

public class GroupUserCreateDto
{
    public required string GroupName { get; set;  }

    public List<string> ListUser { get; set; } = new();
} 


