using backend_online_testing.Models;

namespace backend_online_testing.Dtos
{
    public class ExamMatrixUpdateDto
    {
        public string ExamMatrixId { get; set; }

        public string ExamMatrixName { get; set; }

        public string ExamMatrixStatus { get; set; }

        public int TotalGenerateExam { get; set; }

        public string SubjectId { get; set; }

        public List<string> ExamId { get; set; }

        public List<MatrixTagsModel>? Tags { get; set; }

        public string MatrixLogUserId { get; set; }
    }
}
