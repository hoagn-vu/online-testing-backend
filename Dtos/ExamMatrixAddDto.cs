namespace Backend_online_testing.Dtos
{
    using Backend_online_testing.Models;

    public class ExamMatrixAddDto
    {
        public string ExamMatrixId { get; set; } = string.Empty;

        public List<MatrixTagsModel> Tags { get; set; } = new List<MatrixTagsModel>();

        public string MatrixLogUserId { get; set; } = string.Empty;
    }
}
