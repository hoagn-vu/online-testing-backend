namespace Backend_online_testing.Dtos;

public class RoomsInSessionDto
{
    public string RoomInSessionId { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string RoomLocation { get; set; } = string.Empty;
    public List<SupervisorsInRoomModel> Supervisors { get; set; } = [];
    public long TotalCandidates { get; set; }
    public string RoomStatus { get; set; } = "inactive";
}

public class SupervisorsInRoomModel
{
    public string SupervisorId { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
}

public class RoomsInSessionRequestDto
{
    public string RoomId { get; set; } = string.Empty;
    public List<string> SupervisorIds { get; set; } = [];
}