namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class ExamsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("examCode")]
        public string? ExamCode { get; set; }

        [BsonElement("examName")]
        public string? ExamName { get; set; }

        [BsonElement("subjectId")]
        public string? SubjectId { get; set; }

        [BsonElement("questionSet")]
        public List<QuestionSetsModel> QuestionSet { get; set; } = new List<QuestionSetsModel>();

        [BsonElement("examStatus")]
        public string? ExamStatus { get; set; }

        [BsonElement("questionBankId")]
        public string? QuestionBankId { get; set; }
    }
}
