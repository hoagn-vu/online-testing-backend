namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class MatrixLogsModel
    {
        [BsonElement("matrixLogUserId")]
        public string MatrixLogUserId { get; set; } = string.Empty;

        [BsonElement("matrixLogUserId")]
        public string MatrixLogType { get; set; } = string.Empty;

        [BsonElement("matrixChangeAt")]
        public DateTime MatrixChangeAt { get; set; }
    }
}
