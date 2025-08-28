namespace Backend_online_testing.Dtos
{
    public class QuestionBankPerSubjectDto
    {
        public string SubjectId { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public List<QuestionBanksDto> QuestionBanks { get; set; } = []; 
    }
    
    public class QuestionBanksDto
    {
        public string QuestionBankId { get; set; } = string.Empty;
        public string QuestionBankName { get; set; } = string.Empty;
        public long TotalQuestions { get; set; }
    }

    public class QuestionBankRequestDto
    {
        public string SubjectId { get; set; } = string.Empty;
        public string? QuestionBankId { get; set; }
        public string QuestionBankName { get; set; } = string.Empty;
        public string? QuestionBankStatus { get; set; }
    }
    
    public class QuestionBankOptionsDto
    {
        public string QuestionBankId { get; set; }
        public string QuestionBankName { get; set; } = string.Empty;
    }
    
    public class UpdateQuestionBankRequestDto
    {
        public string SubjectId { get; set; } = string.Empty;
        public string QuestionBankId { get; set; } = string.Empty;

        public string? QuestionBankName { get; set; }
        public string? QuestionBankStatus { get; set; }
        public List<string>? AllChapter { get; set; }
        public List<string>? AllLevel { get; set; }
    }
}
