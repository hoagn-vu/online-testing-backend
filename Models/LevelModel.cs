using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Backend_online_testing.Models;

public class LevelModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("levelName")]
    public string? LevelName { get; set; }
}
