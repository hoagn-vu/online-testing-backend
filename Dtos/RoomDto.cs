﻿namespace Backend_online_testing.DTO
{
    public class RoomDto
    {
        public string RoomName { get; set; } = string.Empty;

        public string RoomStatus { get; set; } = string.Empty;

        public int RoomCapacity { get; set; }

        public string RoomLocation { get; set; } = string.Empty;
    }
}
