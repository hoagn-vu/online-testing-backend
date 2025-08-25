using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Backend_online_testing.Models;

public class ParticipationViolationModel
{

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("organizeExamId")]
    public string OrganizeExamId { get; set; } = default!;

    [BsonElement("organizeExamName")]
    public string OrganizeExamName { get; set; } = default!;

    [BsonElement("totalCandidates")]
    public int TotalCandidates { get; set; }

    [BsonElement("totalCandidateTerminated")]
    public int TotalCandidateTerminated { get; set; }

    [BsonElement("totalCandidateNotParticipated")]
    public int TotalCandidateNotParticipated { get; set; }
}
