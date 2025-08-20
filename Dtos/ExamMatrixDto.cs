using Backend_online_testing.Models;

namespace Backend_online_testing.Dtos
{
    public class ExamMatrixDto
    {
        public string Id { get; set; } = string.Empty;
        public string MatrixName { get; set; } = string.Empty;
        public string SubjectId { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string QuestionBankId { get; set; } = string.Empty;
        public string QuestionBankName { get; set; } = string.Empty;
        public List<MatrixTagsModel> MatrixTags { get; set; } = [];
        public int TotalGeneratedExams { get; set; }
        public List<string> ExamIds { get; set; } = [];
    }

    public class ExamMatrixRequestDto
    {
        public string MatrixName { get; set; } = string.Empty;
        public string SubjectId { get; set; } = string.Empty;
        public string QuestionBankId { get; set; } = string.Empty;
        public List<MatrixTagsModel> MatrixTags { get; set; } = [];
    }
    
    public class MatrixOptionsDto
    {
        public string Id { get; set; } = string.Empty;
        public string MatrixName { get; set; } = string.Empty;
        public string SubjectId { get; set; } = string.Empty;
    }
    
    public class ExamMatrixUpdateDto
    {
        public string MatrixName { get; set; } = string.Empty;

        public string MatrixStatus { get; set; } = string.Empty;

        public int TotalGenerateExam { get; set; }

        public string SubjectId { get; set; } = string.Empty;

        public List<string> ExamId { get; set; } = new List<string>();

        public List<MatrixTagsModel>? MatrixTags { get; set; }

        public string? QuestionBankId { get; internal set; }
    }
    
    public class ExamMatrixAddDto
    {
        public string ExamMatrixId { get; set; } = string.Empty;

        public List<MatrixTagsModel> Tags { get; set; } = new List<MatrixTagsModel>();

        public string MatrixLogUserId { get; set; } = string.Empty;
    }
    
    
    
}
