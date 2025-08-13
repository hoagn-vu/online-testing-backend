namespace Backend_online_testing.Dtos;

public class StatisticsDto
{

}

public class OrganizeExamScoreStatistisDto
{
    public string OrganizeExamId { get; set; } = default!;
    public string OrganizeExamName { get; set; } = default!;
    public int TotalCandidates { get; set; }
    public int NoScoreCount { get; set; }
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