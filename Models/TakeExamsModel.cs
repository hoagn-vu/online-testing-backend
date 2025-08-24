using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class TakeExamsModel
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
    
    [BsonElement("examId")]
    public string? ExamId { get; set; }
    
    [BsonElement("startAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? StartAt { get; set; }
    
    [BsonElement("answers")]
    public List<AnswersModel> Answers { get; set; } = [];
    
    [BsonElement("finishedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? FinishedAt { get; set; }
    
    [BsonElement("totalScore")]
    public double TotalScore { get; set; }

    [BsonElement("roomStatus")]
    public string RoomStatus { get; set; } = "closed";    
    
    [BsonElement("status")]
    public string Status { get; set; } = "not_started"; // active/closed/not_started/in_exam/rein_exam/out_exam/done/terminate
    
    [BsonElement("unrecognizedReason")]
    public string? UnrecognizedReason { get; set; }
    
    [BsonElement("progress")] 
    public int Progress { get; set; }
    
    [BsonElement("violationCount")]
    public int ViolationCount { get; set; }

    [BsonElement("takeTime")] public int TakeTime { get; set; } = 1;
}