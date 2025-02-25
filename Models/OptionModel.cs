using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class OptionModel
    {
        [BsonElement("optionId")]
        public ObjectId OptionId { get; set; }
        [BsonElement("optionText")]
        public string? OptionText { get; set; }
        [BsonElement("isCorrect")]
        public bool? IsCorrect { get; set; }
    }
}
