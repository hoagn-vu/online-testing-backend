namespace Backend_online_testing.Dtos;

public class SubjectDto
{
    public string Id { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectStatus { get; set; } = string.Empty;
    public long TotalQuestionBanks { get; set; }
}

public class SubjectRequestDto
{
    public string SubjectName { get; set; } = string.Empty;
    public string? SubjectStatus { get; set; }
}

public class SubjectOptionsDto
{
    public string Id { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
}