namespace Backend_online_testing.Dtos
{
    public class QuestionBankDto
    {
        public string QuestionBankId { get; set; } = string.Empty;
        public string QuestionBankName { get; set; } = string.Empty;
        public long TotalQuestions { get; set; }
    }

    public class QuestionBankRequestDto
    {
        public string SubjectId { get; set; } = string.Empty;
        public string? QuestionBankId { get; set; }
        public string QuestionBankName { get; set; } = string.Empty;
        public string? QuestionBankStatus { get; set; }
    }
}
