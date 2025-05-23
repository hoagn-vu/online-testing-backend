using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class SessionsModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SessionId { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("sessionName")]
    public string SessionName { get; set; } = string.Empty;
    
    [BsonElement("activeAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ActiveAt { get; set; }
    
    [BsonElement("forceEndAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ForceEndAt { get; set; }
    
    [BsonElement("rooms")]
    public List<SessionRoomsModel> RoomsInSession { get; set; } = [];
    
    [BsonElement("sessionStatus")]
    public string SessionStatus { get; set; } = "active";
}