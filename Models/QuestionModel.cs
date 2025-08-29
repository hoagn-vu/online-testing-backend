using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Backend_online_testing.Models
{
    public class QuestionModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string QuestionId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("options")]
        public List<OptionsModel> Options { get; set; } = [];

        [BsonElement("questionType")]
        public string QuestionType { get; set; } = string.Empty;

        [BsonElement("questionStatus")] 
        public string QuestionStatus { get; set; } = "available";

        [BsonElement("questionText")]
        public string QuestionText { get; set; } = string.Empty;

        [BsonElement("imgSources")]
        public List<string> ImgLinks { get; set; } = [];

        [BsonElement("isRandomOrder")]
        public bool? IsRandomOrder { get; set; } = false;

        [BsonElement("tags")]
        public List<string>? Tags { get; set; } = [string.Empty, string.Empty];
    }
}
