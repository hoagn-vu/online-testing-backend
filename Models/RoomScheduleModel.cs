using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models
{
    public class RoomScheduleModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        
        [BsonElement("startAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartAt { get; set; }
    
        [BsonElement("finishAt")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime FinishAt { get; set; }
        
        [BsonElement("totalCandidates")]
        public int? TotalCandidates { get; set; }
        
        [BsonElement("organizeExamId")]
        public string OrganizeExamId { get; set; } = string.Empty;
        
        [BsonElement("sessionId")]
        public string SessionId { get; set; } = string.Empty;
    }
}