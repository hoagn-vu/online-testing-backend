using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class ExamsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement]
        public string? ExamCode { get; set; }
        
        [BsonElement]
        public string? ExamName { get; set; }
        
        [BsonElement]
        public string? SubjectId { get; set; }  

        [BsonElement]
        public List<QuestionSetsModel> QuestionSet { get; set; }

        [BsonElement]
        public string? ExamStatus { get; set; }

        [BsonElement]
        public List<ExamLogsModel> ExamLogs { get; set; }
    }
}
