namespace Backend_online_testing.Dtos;

public class ProcessTakeExamDto
{
    public string OrganizeExamId { get; set; } = string.Empty;
    public string OrganizeExamName { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public string? MatrixId { get; set; }
    public string? ExamId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ToggleSessionStatusRequest
{
    public string OrganizeExamId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

public class ToggleRoomStatusRequest
{
    public string OrganizeExamId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
}
