using MongoDB.Bson;

namespace Backend_online_testing.Dtos;

public class SessionsDto
{
    public string SessionId { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime FinishAt { get; set; }
    public long TotalRooms { get; set; }
    public string SessionStatus { get; set; } = string.Empty;
}

public class SessionRequestDto
{
    public string SessionName { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime FinishAt { get; set; }
    public DateTime? ForceEndAt { get; set; }
    public string SessionStatus { get; set; } = "active";
}