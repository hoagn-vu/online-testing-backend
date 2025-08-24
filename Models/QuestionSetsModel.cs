namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class QuestionSetsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string QuestionId { get; set; } = string.Empty;

        [BsonElement("questionScore")]
        public double? QuestionScore { get; set; }
    }
}
