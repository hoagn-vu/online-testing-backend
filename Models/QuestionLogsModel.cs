namespace Backend_online_testing.Models
{
    using MongoDB.Bson.Serialization.Attributes;

    public class QuestionLogsModel
    {
        [BsonElement("questionLogType")]
        public string QuestionLogType { get; set; } = string.Empty;

        [BsonElement("questionLogUserId")]
        public string QuestionLogUserId { get; set; } = string.Empty;

        [BsonElement("questionLogAt")]
        public DateTime QuestionLogAt { get; set; }
    }
}
