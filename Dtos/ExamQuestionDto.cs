using backend_online_testing.Models;

namespace backend_online_testing.DTO
{
    public class ExamQuestionDTO
    {   
        public string? Id { get; set; }
        public string? ExamLogUserId { get; set; }
        public List<QuestionSetsModel> QuestionSets { get; set; }
    }
}
