using Backend_online_testing.Models;

namespace Backend_online_testing.Dtos
{
    public class RoomDto
    {
        public string? RoomName { get; set; } = string.Empty;
        public string? RoomStatus { get; set; } = string.Empty;
        public int? RoomCapacity { get; set; }
        public string? RoomLocation { get; set; } = string.Empty;
        public List<RoomScheduleModel>? RoomSchedule { get; set; } = [];
    }

    public class GetRoomsDto
    {
        public string RoomId { get; set; } = string.Empty;
        public string? RoomName { get; set; } = string.Empty;
        public string? RoomStatus { get; set; } = string.Empty;
        public string? RoomLocation { get; set; } = string.Empty;
        public int? RoomCapacity { get; set; }
    } 
    public class RoomOptionsDto
    {
        public string RoomId { get; set; } = string.Empty;
        public string? RoomName { get; set; } = string.Empty;
        public string? RoomLocation { get; set; } = string.Empty;
        public int? RoomCapacity { get; set; }
    }
    
    public class GetRoomSchedulesRequestDto
    {
        public string RoomId { get; set; } = string.Empty;
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    public class RoomScheduleDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime FinishAt { get; set; }
        public int? TotalCandidates { get; set; }
        public string OrganizeExamId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
    }

    public class RoomWithSchedulesDto
    {
        public string RoomId { get; set; } = string.Empty;
        public string? RoomName { get; set; }
        public List<RoomScheduleDto> Schedules { get; set; } = new();
    }

}
