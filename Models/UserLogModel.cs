using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class UserLogModel
    {
        [BsonElement("logUserId")]
        public ObjectId UserId { get; set; }
        [BsonElement("userLogType")]
        public required string Type { get; set; }
        [BsonElement("userLogAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LogAt { get; set; } = DateTime.UtcNow;
    }
}
