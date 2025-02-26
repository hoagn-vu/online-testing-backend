using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend_online_testing.Models
{
    public class MatrixTagsModel
    {
        [BsonElement]
        public string TagName { get; set; }

        [BsonElement]
        public int QuestionCount { get; set; }

        [BsonElement]
        public int TagScore { get; set; }
    }
}
