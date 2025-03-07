using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models
{
    public class OptionsModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string OptionId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("optionText")]
        public string OptionText { get; set; }

        [BsonElement("isCorrect")]
        public bool? IsCorrect { get; set; }
    }
}
