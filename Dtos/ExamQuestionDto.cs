namespace Backend_online_testing.DTO
{
    using Backend_online_testing.Models;

    public class ExamQuestionDTO
    {
        public List<QuestionSetsModel> QuestionSets { get; set; } = new List<QuestionSetsModel>();
    }
}
