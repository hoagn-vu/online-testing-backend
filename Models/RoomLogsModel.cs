namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class RoomLogsModel
    {
        [BsonId]
        public string LogId { get; set; } = string.Empty;

        [BsonElement("roomLogUserId")]
        public string RoomLogUserId { get; set; } = string.Empty;

        [BsonElement("roomChangeAt")]
        public DateTime RoomChangeAt { get; set; }

        [BsonElement("roomLogsType")]
        public string RoomLogType { get; set; } = string.Empty;
    }
}
