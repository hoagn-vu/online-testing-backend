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
        public string ExamCode { get; set; } = string.Empty;

        [BsonElement("examName")]
        public string ExamName { get; set; } = string.Empty;

        [BsonElement("subjectId")]
        public string SubjectId { get; set; } = string.Empty;

        [BsonElement("questionBankId")]
        public string QuestionBankId { get; set; } = string.Empty;
        
        [BsonElement("questionSet")]
        public List<QuestionSetsModel> QuestionSet { get; set; } = [];

        [BsonElement("examStatus")]
        public string ExamStatus { get; set; } = string.Empty;

        
        [BsonElement("organizeExamUsed")]
        public List<string>? OrganizeExamUsed { get; set; } = [];
    }
}
