namespace Backend_online_testing.DTO
{
    using Backend_online_testing.Models;

    public class ExamQuestionDTO
    {
        public string? Id { get; set; }

        public string? ExamLogUserId { get; set; }

        public List<QuestionSetsModel> QuestionSets { get; set; } = new List<QuestionSetsModel>();
    }
}
