namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class ExamLogsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string? ExamLogUserId { get; set; }

        [BsonElement("examLogType")]
        public string? ExamLogType { get; set; }

        [BsonElement("examChangeAt")]
        public DateTime ExamChangeAt { get; set; }
    }
}
