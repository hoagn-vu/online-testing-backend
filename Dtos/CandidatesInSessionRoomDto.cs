namespace Backend_online_testing.Dtos;

public class CandidatesInSessionRoomDto
{
    public string CandidateId { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string? Dob { get; set; } = string.Empty;
    public string? Gender { get; set; } = string.Empty;
    public string? ProgressStatus { get; set; } = string.Empty;
    public bool? RecognizedResult { get; set; }
    public double? Score { get; set; }
}

public class CandidatesInSessionRoomRequestDto
{
    public List<string> CandidateIds { get; set; } = [];
}