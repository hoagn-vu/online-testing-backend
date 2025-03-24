using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class SessionRoomsModel
{
    [BsonElement("roomId")]
    public required string RoomInSessionId { get; set; }

    [BsonElement("supervisorId")] 
    public List<string> SupervisorIds { get; set; } = [];
    
    [BsonElement("candidates")]
    public List<CandidatesInRoomModel> Candidates { get; set; } = [];
}