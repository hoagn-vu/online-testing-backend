namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class RoomLogModel
    {
        [BsonElement("roomLogUserId")]
        public ObjectId UserId { get; set; }

        [BsonElement("RoomLogType")]
        required public string Type { get; set; }

        [BsonElement("RoomLogAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LogAt { get; set; } = DateTime.UtcNow;
    }
}
