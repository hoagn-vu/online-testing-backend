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
    
    public class QuestionResponseDto
    {
        public string QuestionId { get; set; } = string.Empty;
        public string QuestionName { get; set; } = string.Empty;
        public required List<OptionsResponseDto> Options { get; set; }
    }

    public class OptionsResponseDto
    {
        public string OptionId { get; set; } = string.Empty;
        public string OptionText { get; set; } = string.Empty;
    }

    public class TagsClassification
    {
        public string Chapter { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int Total { get; set; }
    }
}
