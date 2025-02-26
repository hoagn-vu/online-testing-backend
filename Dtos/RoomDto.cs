namespace backend_online_testing.DTO
{
    public class RoomDTO
    {
        public string RoomName { get; set; } = string.Empty;

        public string RoomStatus { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public string UserId { get; set; } = string.Empty;
    }
}
