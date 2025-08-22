namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class MatrixTagsModel
    {
        [BsonElement("chapter")]
        public string? Chapter { get; set; } = string.Empty;        
        
        [BsonElement("level")]
        public string? Level { get; set; } = string.Empty;

        [BsonElement("questionCount")]
        public int QuestionCount { get; set; }

        [BsonElement("score")]
        public double Score { get; set; }
    }
}
