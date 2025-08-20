using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class TrackExamsModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("organizeExamId")]
    public string OrganizeExamId { get; set; } = string.Empty;
    
    [BsonElement("sessionId")]
    public string SessionId { get; set; } = string.Empty;
    
    [BsonElement("roomId")]
    public string RoomId { get; set; } = string.Empty;
    
    [BsonElement("roomSessionStatus")]
    public string? RoomSessionStatus { get; set; } = string.Empty;
}