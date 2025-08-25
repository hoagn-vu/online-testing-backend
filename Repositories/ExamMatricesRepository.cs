using Backend_online_testing.Models;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories
{
    public interface IExamMatricesRepository
    {
        Task<ExamMatricesModel?> GetExamMatrixByIdAsync(string id);
        Task<SubjectsModel?> GetSubjectByIdAsync(string subjectId);
        Task InsertExamAsync(ExamsModel exam);
        Task UpdateExamMatrixAsync(ExamMatricesModel examMatrix);
    }
    
    public class ExamMatricesRepository : IExamMatricesRepository
    {
        private readonly IMongoCollection<ExamMatricesModel> _examMatrices;
        private readonly IMongoCollection<SubjectsModel> _subjects;
        private readonly IMongoCollection<ExamsModel> _exams;
        
        public ExamMatricesRepository(IMongoDatabase database)
        {
            _examMatrices = database.GetCollection<ExamMatricesModel>("examMatrices");
            _subjects = database.GetCollection<SubjectsModel>("subjects");
            _exams = database.GetCollection<ExamsModel>("exams");
        }
        
        public async Task<ExamMatricesModel?> GetExamMatrixByIdAsync(string id)
        {
            return await _examMatrices.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<SubjectsModel?> GetSubjectByIdAsync(string subjectId)
        {
            return await _subjects.Find(x => x.Id == subjectId).FirstOrDefaultAsync();
        }

        public async Task InsertExamAsync(ExamsModel exam)
        {
            await _exams.InsertOneAsync(exam);
        }

        public async Task UpdateExamMatrixAsync(ExamMatricesModel examMatrix)
        {
            await _examMatrices.ReplaceOneAsync(x => x.Id == examMatrix.Id, examMatrix);
        }
        
    }
}