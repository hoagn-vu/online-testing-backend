namespace Backend_online_testing.Dtos;

public class SubjectDto
{
    public string Id { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectStatus { get; set; } = string.Empty;
    public long TotalQuestionBanks { get; set; }
}

public class GetSubjectDto
{
    public string Id { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectStatus { get; set; } = string.Empty;
}

public class SubjectRequestDto
{
    public string SubjectName { get; set; } = string.Empty;
    public string? SubjectStatus { get; set; } = string.Empty;
}

public class SubjectOptionsDto
{
    public string Id { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
}