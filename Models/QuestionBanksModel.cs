namespace Backend_online_testing.Models
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public class QuestionBanksModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string QuestionBankId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("questionBankName")]
        public string QuestionBankName { get; set; } = string.Empty;

        [BsonElement("questionBankStatus")]
        public string QuestionBankStatus { get; set; } = "available";

        [BsonElement("allChapter")]
        public List<string> AllChapter { get; set; } = [];

        [BsonElement("allLevel")]
        public List<string> AllLevel { get; set; } = [];

        [BsonElement("questionList")]
        public List<QuestionSetModel> QuestionList { get; set; } = [];
    }
}
