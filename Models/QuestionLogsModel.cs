using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class QuestionLogsModel
    {
        [BsonElement("questionLogType")]
        public string QuestionLogType { get; set; }

        [BsonElement("questionLogUserId")]
        public string QuestionLogUserId { get; set; }

        [BsonElement("questionLogAt")]
        public DateTime QuestionLogAt { get; set; }
    }
}
