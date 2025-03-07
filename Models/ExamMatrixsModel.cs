using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models
{
    public class ExamMatrixsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string Id { get; set; }

        [BsonElement("matrixName")]
        public string MatrixName { get; set; }

        [BsonElement("MatrixStatus")]
        public string MatrixStatus { get; set; }

        [BsonElement("totalGeneratedExams")]
        public int TotalGeneratedExams { get; set; }

        [BsonElement("subjectId")]
        public string SubjectId { get; set; }

        [BsonElement("matrixTags")]
        public List<MatrixTagsModel>? MatrixTags { get; set; }

        [BsonElement("matrixLogs")]
        public List<MatrixLogsModel>? MatrixLogs { get; set; }

        [BsonElement("examId")]
        public List<string> ExamId { get; set; }
    }
}
