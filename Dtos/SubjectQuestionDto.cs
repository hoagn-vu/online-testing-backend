namespace Backend_online_testing.Dtos
{
    using Backend_online_testing.Models;

    public class SubjectQuestionDto
    {
        public List<OptionsModel> Options { get; set; } = new List<OptionsModel>();

        public string QuestionType { get; set; } = string.Empty;

        public string QuestionStatus { get; set; } = string.Empty;

        public string QuestionText { get; set; } = string.Empty;

        public bool IsRandomOrder { get; set; }

        public List<string> Tags { get; set; } = new List<string>();
    }
}
