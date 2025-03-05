using backend_online_testing.Models;

namespace backend_online_testing.Dtos
{
    public class ExamMatrixAddDto
    {
        public string ExamMatrixId { get; set; }
        public List<MatrixTagsModel> Tags { get; set; }
        public string MatrixLogUserId { get; set; }
    }
}
