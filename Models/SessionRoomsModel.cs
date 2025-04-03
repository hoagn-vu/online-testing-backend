using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class SessionRoomsModel
{
    [BsonElement("roomId")]
    public required string RoomInSessionId { get; set; }

    [BsonElement("supervisorIds")] 
    public List<string> SupervisorIds { get; set; } = [];
    
    [BsonElement("candidateIds")]
    public List<string> CandidateIds { get; set; } = [];
    
    [BsonElement("roomStatus")]
    public string RoomStatus { get; set; } = "closed";
}