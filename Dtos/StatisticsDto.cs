namespace Backend_online_testing.Dtos;

public class StatisticsDto
{

}

public class OrganizeExamScoreStatistisDto
{
    public string OrganizeExamId { get; set; } = default!;

    public string OrganizeExamName { get; set; } = default!;

    public string SubjecName { get; set; } = string.Empty;

    public int TotalCandidates { get; set; }

    public int NoScoreCount { get; set; }

    public double? MinScore { get; set; }

    public double? MaxScore { get; set; }

    public double? AverageScore { get; set; }

    public ScoreDistributionDto ScoreDistribution { get; set; } = new();
}

public class ScoreDistributionDto
{
    public int Bin0_1 { get; set; }
    public int Bin1_2 { get; set; }
    public int Bin2_3 { get; set; }
    public int Bin3_4 { get; set; }
    public int Bin4_5 { get; set; }
    public int Bin5_6 { get; set; }
    public int Bin6_7 { get; set; }
    public int Bin7_8 { get; set; }
    public int Bin8_9 { get; set; }
    public int Bin9_10 { get; set; }
}

public class ParticipationViolationDto
{
    public string OrganizeExamId { get; set; } = default!;
    public string OrganizeExamName { get; set; } = default!;
    public int TotalCandidates { get; set; }
    public int TotalCandidateTerminated { get; set; }
}

public class ExamSetStatisticDto
{
    public string OrganizeExamId { get; set; } = string.Empty;

    public string OrganizeExamName { get; set; } = string.Empty;

    public int TotalCandidates { get; set; }

    public List<ExamCountItem> ExamCounts { get; set; } = new();
}

//Exam set statistic
public class ExamCountItem
{
    public string ExamId { get; set; } = string.Empty;

    public string ExamName { get; set; } = string.Empty;

    public int Count { get; set; }
}

//Exam question statistic
public sealed class QuestionStatDto
{
    public string QuestionId { get; set; } = string.Empty;
    public long Correct { get; set; }
    public long Incorrect { get; set; }
}

public sealed class ExamQuestionStatsResponse
{
    public string OrganizeExamId { get; set; } = string.Empty;
    public string ExamId { get; set; } = string.Empty;
    public List<QuestionStatDto> Questions { get; set; } = new();
}
