namespace Backend_online_testing.Dtos
{
    using Backend_online_testing.Models;

    public class ExamMatrixUpdateDto
    {
        public string ExamMatrixId { get; set; } = string.Empty;

        public string ExamMatrixName { get; set; } = string.Empty;

        public string ExamMatrixStatus { get; set; } = string.Empty;

        public int TotalGenerateExam { get; set; }

        public string SubjectId { get; set; } = string.Empty;

        public List<string> ExamId { get; set; } = new List<string>();

        public List<MatrixTagsModel>? Tags { get; set; }

        public string MatrixLogUserId { get; set; } = string.Empty;
    }
}
