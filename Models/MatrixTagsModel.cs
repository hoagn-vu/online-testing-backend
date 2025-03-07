using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models
{
    public class MatrixTagsModel
    {
        [BsonElement("tagName")]
        public string TagName { get; set; }

        [BsonElement("questionCount")]
        public int QuestionCount { get; set; }

        [BsonElement("tagScore")]
        public int TagScore { get; set; }
    }
}
