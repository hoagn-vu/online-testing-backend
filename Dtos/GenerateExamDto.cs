using Backend_online_testing.Models;

namespace Backend_online_testing.Dtos
{
    public class GenerateExamDto
    {
        
    }
    
    public class GenerateExamRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string OrganizeExamId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
    }

    public class GenerateExamResponseDto
    {
        public string ExamName { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int TotalQuestions { get; set; }
        public int MaxScore { get; set; }
        public List<GenerateExamQuestionResponseDto> Questions { get; set; } = [];
    }
    
    public class GenerateExamQuestionResponseDto
    {
        public string QuestionId { get; set; } = string.Empty;
        public List<GenerateExamOptionResponseDto> Options { get; set; } = [];
        public string QuestionType { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public List<string> ImgLinks { get; set; } = [];
        public bool? IsRandomOrder { get; set; }
    }

    public class GenerateExamOptionResponseDto
    {
        public string OptionId { get; set; } = string.Empty;
        public string OptionText { get; set; } = string.Empty;
    }

}