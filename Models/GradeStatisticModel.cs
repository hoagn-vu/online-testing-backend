using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Backend_online_testing.Models;

public class GradeStatisticModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("organizeExamId")]
    public string OrganizeExamId { get; set; } = default!;

    [BsonElement("organizeExamName")]
    public string OrganizeExamName { get; set; } = default!;

    [BsonElement("subjectName")]
    public string SubjectName { get; set; } = string.Empty;

    [BsonElement("totalCandidates")]
    public int TotalCandidates { get; set; }

    [BsonElement("noScoreCount")]
    public int NoScoreCount { get; set; }

    [BsonElement("minScore")]
    public double? MinScore { get; set; }

    [BsonElement("maxScore")]
    public double? MaxScore { get; set; }

    [BsonElement("averageScore")]
    public double? AverageScore { get; set; }

    [BsonElement("scoreDistribution")]
    public ScoreDistribution ScoreDistribution { get; set; } = new();
}

public class ScoreDistribution
{
    [BsonElement("bin0_1")]
    public int Bin0_1 { get; set; }

    [BsonElement("bin1_2")]
    public int Bin1_2 { get; set; }

    [BsonElement("bin2_3")]
    public int Bin2_3 { get; set; }

    [BsonElement("bin3_4")]
    public int Bin3_4 { get; set; }

    [BsonElement("bin4_5")]
    public int Bin4_5 { get; set; }

    [BsonElement("bin5_6")]
    public int Bin5_6 { get; set; }

    [BsonElement("bin6_7")]
    public int Bin6_7 { get; set; }

    [BsonElement("bin7_8")]
    public int Bin7_8 { get; set; }

    [BsonElement("bin8_9")]
    public int Bin8_9 { get; set; }

    [BsonElement("bin9_10")]
    public int Bin9_10 { get; set; }
}

