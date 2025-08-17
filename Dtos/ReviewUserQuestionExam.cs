using Backend_online_testing.Models;
using System.Runtime.CompilerServices;

namespace Backend_online_testing.Dtos;

public class ReviewUserQuestionExam
{
}

public class ExamReviewDto
{
    public string FullName { get; set; } = string.Empty;

    public string OrganizeExamName {  get; set; } = string.Empty;

    public string SessionName { get; set; } = string.Empty;

    public int Duration { get; set; }

    public double TotalScore { get; set; }

    public string RoomName { get; set; } = string.Empty;

    public string SubjectName { get; set; } = string.Empty;

    public string QuestionBankName { get; set; } = string.Empty;

    public List<QuestionReviewDto> Questions { get; set; } = new();
}

public class QuestionReviewDto
{
    public string QuestionId { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new();
    public List<OptionsModel> Options { get; set; } = new();

    public List<string> AnswerChosen { get; set; } = new();

    public bool IsUserChosenCorrect { get; set; }
}