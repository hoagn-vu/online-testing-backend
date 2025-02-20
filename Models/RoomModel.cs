using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace backend_online_testing.Models
{
    public class RoomModel
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("roomName")]
        public string? Name { get; set; }
        [BsonElement("roomLocation")]
        public string? Location { get; set; }
        [BsonElement("roomCapacity")]
        public int? Capacity { get; set; }
        [BsonElement("roomStatus")]
        public string? Status { get; set; } = "available";
        [BsonElement("roomLogs")]
        public List<RoomLogModel>? Logs { get; set; }
    }
}
