namespace Backend_online_testing.Dtos
{
    using Backend_online_testing.Models;

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
}
