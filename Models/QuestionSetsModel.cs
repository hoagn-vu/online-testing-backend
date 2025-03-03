using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class QuestionSetsModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string QuestionId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("questionScore")]
        public double? QuestionScore { get; set; }
    }
}
