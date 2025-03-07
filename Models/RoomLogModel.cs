using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models
{
    public class RoomLogModel
    {
        [BsonElement("roomLogUserId")]
        public ObjectId UserId { get; set; }
        [BsonElement("RoomLogType")]
        public required string Type { get; set; }
        [BsonElement("RoomLogAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LogAt { get; set; } = DateTime.UtcNow;
    }
}
