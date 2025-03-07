using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models
{
    public class QuestionListModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string QuestionId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("options")]
        public List<OptionsModel>? Options { get; set; }

        [BsonElement("questionType")]
        public string QuestionType { get; set; }

        [BsonElement("questionStatus")]
        public string QuestionStatus { get; set; }

        [BsonElement("questionText")]
        public string QuestionText { get; set; }

        [BsonElement("isRandomOrder")]
        public bool? IsRandomOrder { get; set; }

        [BsonElement("questionLogs")]
        public List<QuestionLogsModel>? QuestionLogs { get; set; }

        [BsonElement("tags")]
        public List<string>? Tags { get; set; }
    }
}
