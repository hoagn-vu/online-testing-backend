using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class ExamMatrixsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]

        public string Id { get; set; }

        [BsonElement]
        public string MatrixName { get; set; }

        [BsonElement]
        public string MatrixStatus { get; set; }

        [BsonElement]
        public int TotalGeneratedExams { get; set; }

        [BsonElement]
        public string SubjectId { get; set; }

        [BsonElement]
        public List<MatrixTagsModel>? MatrixTags { get; set; }

        [BsonElement]
        public List<MatrixLogsModel>? MatrixLogs { get; set; }

        [BsonElement]
        public List<string> ExamId { get; set; }
    }
}
