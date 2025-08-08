using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Backend_online_testing.Models;

public class GroupUserModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public required string GroupName { get; set; }

    public List<string> ListUser { get; set; } = new List<string>();

    public string GroupStatus { get; set; } = "active";
}
