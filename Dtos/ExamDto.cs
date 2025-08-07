namespace Backend_online_testing.DTO
{
    using Backend_online_testing.Dtos;
    using Backend_online_testing.Models;

    public class ExamDto
    {
        public string ExamCode { get; set; } = string.Empty;

        public string ExamName { get; set; } = string.Empty;

        public string SubjectId { get; set; } = string.Empty;

        public string ExamStatus { get; set; } = "available";
        public string QuestionBankId { get; set; } = string.Empty;

        public List<QuestionSetsModel> QuestionSets { get; set; } = new List<QuestionSetsModel>();
    }
    
    public class ExamResponseDto {
        public string Id { get; set; } = string.Empty;
        public string? ExamCode { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
        public string SubjectId { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ExamStatus { get; set; } = string.Empty;
        public string QuestionBankId { get; set; } = string.Empty;
        public string? QuestionBankName { get; set; } = string.Empty;
    }

    public class ExamDetailResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string? ExamCode { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ExamStatus { get; set; } = string.Empty;
        public string? QuestionBankName { get; set; } = string.Empty;
        public List<ExamQuestionDetailDTO> ListQuestion { get; set; } = new();
    }

    public class ExamQuestionDetailDTO
    {
        public string QuestionId { get; set; } = string.Empty;
        public string QuestionText { get; set; } = string.Empty;
        public string Chapter { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public double QuestionScore { get; set; }
        public List<OptionDetailDTO> Options { get; set; } = new();
    }
    
    public class ExamOptionsDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? ExamCode { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
    }
}
