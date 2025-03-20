namespace Backend_online_testing.Dtos
{
    using Backend_online_testing.Models;

    public class QuestionDto
    {
        public string SubjectId { get; set; } = string.Empty;

        public string SubjectName { get; set; } = string.Empty;

        public string QuestionBankId { get; set; } = string.Empty;

        public string QuestionBankName { get; set; } = string.Empty;
        
        public List<QuestionModel>? Questions { get; set; }
    }
}
