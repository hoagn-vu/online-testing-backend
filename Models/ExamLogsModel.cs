using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class ExamLogsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string? ExamLogUserId { get; set; }

        [BsonElement]
        public string? ExamLogType { get; set; }

        [BsonElement]
        public DateTime ExamChangeAt { get; set; }
    }
}
