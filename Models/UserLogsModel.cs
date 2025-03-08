using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class UserLogsModel
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string LogId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("logAction")]
        public required string LogAction { get; set; }
        
        [BsonElement("logAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LogAt { get; set; } = DateTime.UtcNow;


        [BsonElement("logDetails")]
        public string? LogDetails { get; set; }

        [BsonElement("affectedObject")]
        public string? AffectedObject { get; set; }

        [BsonElement("affectedObjectId")]
        public string? AffectedObjectId { get; set; }
    }
}
