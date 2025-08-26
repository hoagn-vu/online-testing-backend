using Backend_online_testing.Dtos;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend_online_testing.Models;

public class OrganizeExamStatisticModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("organizeExamId")]
    public string OrganizeExamId { get; set; } = string.Empty;

    [BsonElement("organizeExamName")]
    public string OrganizeExamName { get; set; } = string.Empty;

    [BsonElement("subjectId")]
    public string SubjecId { get; set; } = string.Empty;

    [BsonElement("subjectName")]
    public string SubjectName { get; set; } = string.Empty;

    [BsonElement("questionBankId")]
    public string QuestionBankId { get; set; } = string.Empty;

    [BsonElement("questionBankName")]
    public string QuestionBankName { get; set; } = string.Empty;

    [BsonElement("questions")]
    public List<QuestionItem> Questions { get; set; } = new();

    [BsonElement("participants")]
    public long Participants { get; set; }
}

public class OptionItem
{
    [BsonElement("optionId")]
    public string OptionId { get; set; } = string.Empty;

    [BsonElement("optionText")]
    public string OptionText { get; set; } = string.Empty;

    [BsonElement("isCorrect")]
    public bool IsCorrect { get; set; }

    [BsonElement("selectedCount")]
    public long SelectedCount { get; set; }
}

public class QuestionItem
{
    [BsonElement("questionId")]
    public string QuestionId { get; set; } = string.Empty;

    [BsonElement("questionType")]
    public string QuestionType { get; set; } = string.Empty;

    [BsonElement("questionText")]
    public string QuestionText { get; set; } = string.Empty;

    [BsonElement("tags")]
    public List<string> tags { get; set; } = new();

    [BsonElement("options")]
    public List<OptionItem> Options { get; set; } = new();

    [BsonElement("totalSelections")]
    public long TotalSelections { get; set; }

    [BsonElement("correctSelections")]
    public long CorrectSelections { get; set; }

    [BsonElement("incorrectSelections")]
    public long IncorrectSelections { get; set; }

    [BsonElement("noSelection")]
    public long NoSelection { get; set; }
}
