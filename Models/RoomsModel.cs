using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class RoomsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = string.Empty;

        [BsonElement("roomStatus")]
        public string? RoomStatus { get; set; } = string.Empty;

        [BsonElement("roomName")]
        public string? RoomName { get; set; } = string.Empty;

        [BsonElement("capacity")]
        public int? Capacity { get; set; }

        [BsonElement("roomLogs")]
        //public List<RoomLogsModel> RoomLogs { get; set; } = new List<RoomLogsModel>();
        public List<RoomLogsModel>? RoomLogs { get; set; }
    }
}
