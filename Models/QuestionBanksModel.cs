using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class QuestionBanksModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string QuestionBankId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("questionBankName")]
        public string QuestionBankName { get; set; }

        [BsonElement("questionBankStatus")]
        public string QuestionBankStatus { get; set; }

        [BsonElement("questionList")]
        public List<QuestionListModel> List { get; set; }
    }
}
