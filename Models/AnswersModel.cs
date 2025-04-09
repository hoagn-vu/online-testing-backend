using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class AnswersModel
{
    [BsonElement("questionId")]
    public string QuestionId { get; set; } = string.Empty;
    
    [BsonElement("score")]
    public double? Score { get; set; }

    [BsonElement("answerChose")] 
    public List<string> AnswerChosen { get; set; } = [];
    
    [BsonElement("isCorrect")]
    public bool IsCorrect { get; set; }
    
    // [BsonElement("answerText")]
    // public List<string>? AnswerText { get; set; }
}