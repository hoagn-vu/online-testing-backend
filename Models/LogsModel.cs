using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class LogsModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("madeBy")]
    public string MadeBy { get; set; } = string.Empty;
    
    [BsonElement("logAction")]
    public string LogAction { get; set; } = string.Empty;

    [BsonElement("logAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LogAt { get; set; } = DateTime.UtcNow;

    [BsonElement("logDetails")]
    public string LogDetails { get; set; } = string.Empty;
}