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
}
