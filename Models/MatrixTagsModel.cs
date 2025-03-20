namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class MatrixTagsModel
    {
        [BsonElement("tagName")]
        public string TagName { get; set; } = string.Empty;

        [BsonElement("questionCount")]
        public int QuestionCount { get; set; }

        [BsonElement("tagScore")]
        public int TagScore { get; set; }
    }
}
