using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class ExamsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("examCode")]
        public string? ExamCode { get; set; }

        [BsonElement("examName")]
        public string? ExamName { get; set; }

        [BsonElement("subjectId")]
        public string? SubjectId { get; set; }

        [BsonElement("questionSet")]
        public List<QuestionSetsModel> QuestionSet { get; set; }

        [BsonElement("examStatus")]
        public string? ExamStatus { get; set; }

        [BsonElement("examLogs")]
        public List<ExamLogsModel> ExamLogs { get; set; }
    }
}
