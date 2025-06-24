namespace Backend_online_testing.Dtos
{
    public class RoomDto
    {
        public string RoomName { get; set; } = string.Empty;

        public string RoomStatus { get; set; } = string.Empty;

        public int RoomCapacity { get; set; }

        public string RoomLocation { get; set; } = string.Empty;
    }

    public class RoomOptionsDto
    {
        public string RoomId { get; set; } = string.Empty;
        public string? RoomName { get; set; } = string.Empty;
        public string? RoomLocation { get; set; } = string.Empty;
        public int? RoomCapacity { get; set; }
    }
}
