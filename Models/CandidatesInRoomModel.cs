using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class CandidatesInRoomModel
{
    [BsonElement("SessionId")]
    public string CandidateId { get; set; } = string.Empty;
    
    [BsonElement("examId")]
    public string? ExamId { get; set; }
    
    [BsonElement("startAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime StartAt { get; set; }
    
    [BsonElement("answers")]
    public List<AnswersModel> Answers { get; set; } = [];

    [BsonElement("progress")] 
    public int Progress { get; set; } = 0;

    [BsonElement("examProgressStatus")]
    public string ProgressStatus { get; set; } = "notin";      // NotIn/InExam/OutExam
    
    [BsonElement("finishedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FinishedAt { get; set; }
    
    [BsonElement("totalScore")]
    public double TotalScore { get; set; }
    
    [BsonElement("violationCount")]
    public int ViolationCount { get; set; } = 0;
    
    [BsonElement("recognizedResult")]
    public bool RecognizedResult { get; set; }
    
    [BsonElement("unrecognizedReason")]
    public string? UnrecognizedReason { get; set; }
}