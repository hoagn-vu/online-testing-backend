using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class SubjectsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("subjects")]
        public string SubjectName { get; set; }

        [BsonElement("questionBanks")]
        public List<QuestionBanksModel>? QuestionBanks { get; set; }
    }
}
