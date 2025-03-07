using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models
{
    public class MatrixLogsModel
    {
        [BsonElement("matrixLogUserId")]
        public string MatrixLogUserId { get; set; }

        [BsonElement("matrixLogUserId")]
        public string MatrixLogType { get; set; }

        [BsonElement("matrixChangeAt")]
        public DateTime MatrixChangeAt { get; set; }
    }
}
