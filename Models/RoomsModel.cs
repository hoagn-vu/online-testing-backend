namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class RoomsModel
    {
        [BsonId]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("roomStatus")]
        public string? RoomStatus { get; set; } = string.Empty;

        [BsonElement("roomName")]
        public string? RoomName { get; set; } = string.Empty;

        [BsonElement("capacity")]
        public int? Capacity { get; set; }

        [BsonElement("roomLogs")]
        public List<RoomLogsModel> RoomLogs { get; set; } = new List<RoomLogsModel>();
    }
}
