namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class ExamMatricesModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("matrixName")]
        public required string MatrixName { get; set; }

        [BsonElement("MatrixStatus")]
        public required string MatrixStatus { get; set; }

        [BsonElement("totalGeneratedExams")]
        public int TotalGeneratedExams { get; set; }

        [BsonElement("subjectId")]
        public required string SubjectId { get; set; }

        [BsonElement("questionBankId")]
        public string? QuestionBankId { get; set; }

        [BsonElement("matrixTags")]
        public List<MatrixTagsModel> MatrixTags { get; set; } = new List<MatrixTagsModel>();

        [BsonElement("examId")]
        public List<string> ExamId { get; set; } = new List<string>();
    }
}
