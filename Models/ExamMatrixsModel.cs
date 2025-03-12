namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class ExamMatrixsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("matrixName")]
        required public string MatrixName { get; set; }

        [BsonElement("MatrixStatus")]
        required public string MatrixStatus { get; set; }

        [BsonElement("totalGeneratedExams")]
        public int TotalGeneratedExams { get; set; }

        [BsonElement("subjectId")]
        required public string SubjectId { get; set; }

        [BsonElement("questionBankId")]
        public string? QuestionBankId { get; set; }

        [BsonElement("matrixTags")]
        public List<MatrixTagsModel> MatrixTags { get; set; } = new List<MatrixTagsModel>();

        [BsonElement("examId")]
        public List<string> ExamId { get; set; } = new List<string>();
    }
}
