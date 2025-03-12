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
        public string QuestionBankStatus { get; set; } = string.Empty;

        [BsonElement("questionList")]
        public List<QuestionListModel> List { get; set; } = new List<QuestionListModel>();
    }
}
