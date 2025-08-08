using Amazon.S3.Model;
using Backend_online_testing.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Dtos;

public class TakeExamDto
{
    public string TakeExamId { get; set; } = string.Empty;
    public string OrganizeExamId { get; set; } = string.Empty;
    public string OrganizeExamName { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<TakeExamQuestionDto> Questions { get; set; } = [];
}

public class TakeExamQuestionDto
{
    public string QuestionId { get; set; } = string.Empty;
    public bool? IsRandomOrder { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<TakeExamOptionDto> Options { get; set; } = [];
}

public class TakeExamOptionDto
{
    public string OptionId { get; set; } = string.Empty;
    public string OptionText { get; set; } = string.Empty;
    public bool IsChosen { get; set; } = false;
}

public class CreateTrackExamDto
{
    public string OrganizeExamId { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string RoomId { get; set; } = string.Empty;
}

public class TrackExamDto
{
    public string OrganizeExamId { get; set; } = string.Empty;
    public string OrganizeExamName { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class TrackExamDetailDto
{
    public string TakeExamId { get; set; } = string.Empty;
    public string OrganizeExamId { get; set; } = string.Empty;
    public string OrganizeExamName { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public int Duration { get; set; }
    public int TotalQuestion { get; set; }
    public List<TrackCandidateDto> Candidates { get; set; } = [];
    public string Status { get; set; } = string.Empty;
}

public class TrackCandidateDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public int Progress { get; set; }
    public DateTime? FinishedAt { get; set; }
    public double? TotalScore { get; set; }
    public int ViolationCount { get; set; }
    public string TakeExamId { get; set; } = string.Empty;
}

public class CandidateDetailsDto
{
    public string OrganizeExamId { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string RoomId { get; set; } = string.Empty;

    public List<CandidateInfoDto> Candidates { get; set; } = new List<CandidateInfoDto>();
}

public class CandidateInfoDto
{
    public string FullName { get; set; } = string.Empty;

    public DateTime? StartAt { get; set; }

    public DateTime? FinishAt { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int Progress { get; set; }

    public int TotalQuestions { get; set; }
}

public class UserAllTrackExamResponseDto
{
    public string UserId { get; set; } = string.Empty;

    public List<TrackExamsInfo> TrackExams { get; set; } = new List<TrackExamsInfo>();
}

public class TrackExamsInfo
{
    public string TrackExamId { get; set; } = string.Empty;

    public string OrganizeExamId { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;

    public string RoomId { get; set; } = string.Empty;

    public string OrganizeExamName { get; set; } = string.Empty;
}