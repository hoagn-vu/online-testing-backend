using Backend_online_testing.Models;

namespace Backend_online_testing.DTO
{
    public class ExamQuestionDTO
    {
        public string? Id { get; set; }
        public string? ExamLogUserId { get; set; }
        public List<QuestionSetsModel> QuestionSets { get; set; }
    }
}
