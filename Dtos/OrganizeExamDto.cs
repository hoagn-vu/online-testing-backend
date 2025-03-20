using Backend_online_testing.Models;

namespace Backend_online_testing.Dtos;

public class OrganizeExamDto
{
    public string Id { get; set; } = string.Empty;
    public string OrganizeExamName { get; set; } = string.Empty;
    public int Duration { get; set; }
    public int? TotalQuestions { get; set; }
    public int MaxScore { get; set; }
    public string SubjectId { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public List<ExamInOrganizeExamDto>? Exams { get; set; }
    public string? MatrixId { get; set; }
    public string? MatrixName { get; set; }
    public string OrganizeExamStatus { get; set; } = "available";
}

public class ExamInOrganizeExamDto
{
    public string Id { get; set; } = string.Empty;
    public string ExamCode { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
}