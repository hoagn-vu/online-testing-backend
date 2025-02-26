using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class MatrixLogsModel
    {
        [BsonElement]
        public string MatrixLogUserId { get; set; }

        [BsonElement]
        public string MatrixLogType { get; set; }

        [BsonElement]
        public DateTime MatrixChangeAt { get; set; }
    }
}
