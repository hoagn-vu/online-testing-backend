namespace Backend_online_testing.DTO
{
    using Backend_online_testing.Models;

    public class ExamDto
    {
        public string ExamCode { get; set; } = string.Empty;

        public string ExamName { get; set; } = string.Empty;

        public string SubjectId { get; set; } = string.Empty;

        public string ExamStatus { get; set; } = string.Empty;

        public string QuestionBankId { get; set; } = string.Empty;
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
}
