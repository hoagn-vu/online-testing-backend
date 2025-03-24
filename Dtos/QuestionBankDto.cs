namespace Backend_online_testing.Dtos
{
    public class QuestionBankDto
    {
        public string QuestionBankId { get; set; } = string.Empty;
        public string QuestionBankName { get; set; } = string.Empty;
        public long TotalQuestions { get; set; }
    }
}
