namespace Backend_online_testing.Models
{
    using Backend_online_testing.Dtos;

    public class SearchQuestionBankResult
    {
        public List<QuestionBankDto> QuestionBanks { get; set; } = new List<QuestionBankDto>();

        public int TotalCount { get; set; }
    }
}
