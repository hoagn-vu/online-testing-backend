using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class SessionRoomsModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SessionRoomId { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("supervisorId")] 
    public List<string> SupervisorIds { get; set; } = [];
    
    [BsonElement("candidates")]
    public List<CandidatesInRoomModel> Candidates { get; set; } = [];
}