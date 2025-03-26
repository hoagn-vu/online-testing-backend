namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class SubjectsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("subjects")]
        public string SubjectName { get; set; } = string.Empty;

        [BsonElement("subjectStatus")]
        public string SubjectStatus { get; set; } = "available";

        [BsonElement("questionBanks")]
        public List<QuestionBanksModel> QuestionBanks { get; set; } = [];
    }
}
