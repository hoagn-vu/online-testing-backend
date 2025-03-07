using Backend_online_testing.Models;

namespace Backend_online_testing.Dtos
{
    public class SubjectQuestionDto
    {
        public List<OptionsModel> Options { get; set; }
        public string QuestionType { get; set; }
        public string QuestionStatus { get; set; }
        public string QuestionText { get; set; }
        public bool IsRandomOrder { get; set; }
        public List<string> Tags { get; set; }
    }
}
