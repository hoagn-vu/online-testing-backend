namespace Backend_online_testing.Dtos
{
    using Backend_online_testing.Models;

    public class SubjectQuestionDto
    {
        public string? QuestionType { get; set; } = "single-choice";
        public string? QuestionText { get; set; } = string.Empty;
        public string? QuestionStatus { get; set; } = "available";
        public List<OptionsModel>? Options { get; set; } = [];
        public bool? IsRandomOrder { get; set; }
        public List<string>? Tags { get; set; } = [];
        public List<string>? ImgLinks { get; set; }
    }
}
