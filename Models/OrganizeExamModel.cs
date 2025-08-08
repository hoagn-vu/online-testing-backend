using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class OrganizeExamModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("organizeExamName")]
    public string OrganizeExamName { get; set; } = string.Empty;
    
    [BsonElement("duration")]
    public int Duration { get; set; }
    
    [BsonElement("totalQuestions")]
    public int? TotalQuestions { get; set; }

    [BsonElement("maxScore")] 
    public int? MaxScore { get; set; } = 10;
    
    [BsonElement("subjectId")]
    public string SubjectId { get; set; } = string.Empty;

    [BsonElement("questionBankId")]
    public string? QuestionBankId { get; set; } = string.Empty;
    
    [BsonElement("examType")]
    public string ExamType { get; set; } = string.Empty;
    
    [BsonElement("examSet")]
    public List<string>? Exams { get; set; }
    
    [BsonElement("matrixId")]
    public string? MatrixId { get; set; }
    
    [BsonElement("sessions")]
    public List<SessionsModel> Sessions { get; set; } = [];
    
    [BsonElement("organizeExamStatus")]
    public string OrganizeExamStatus { get; set; } = "active";
}