using Backend_online_testing.Models;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories
{
    public interface ISubmitAnswerRepository
    {
        Task<UsersModel?> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(UsersModel user);
        Task<SubjectsModel?> GetSubjectByOrganizeExamIdAsync(string organizeExamId);
        Task<QuestionBanksModel?> GetQuestionBankByOrganizeExamIdAsync(string organizeExamId);
    }

    public class SubmitAnswerRepository : ISubmitAnswerRepository
    {
        private readonly IMongoCollection<UsersModel> _users;
        private readonly IMongoCollection<SubjectsModel> _subjects;
        private readonly IMongoCollection<OrganizeExamModel> _organizeExams;

        public SubmitAnswerRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<UsersModel>("users");
            _subjects = database.GetCollection<SubjectsModel>("subjects");
            _organizeExams = database.GetCollection<OrganizeExamModel>("organizeExams");
        }
        
        public async Task<UsersModel?> GetUserByIdAsync(string userId)
        {
            return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }

        public async Task UpdateUserAsync(UsersModel user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task<SubjectsModel?> GetSubjectByOrganizeExamIdAsync(string organizeExamId)
        {
            var organizeExam = await _organizeExams.Find(o => o.Id == organizeExamId).FirstOrDefaultAsync();
            if (organizeExam == null) return null;

            return await _subjects.Find(s => s.Id == organizeExam.SubjectId).FirstOrDefaultAsync();
        }
        
        public async Task<QuestionBanksModel?> GetQuestionBankByOrganizeExamIdAsync(string organizeExamId)
        {
            var organizeExam = await _organizeExams.Find(o => o.Id == organizeExamId).FirstOrDefaultAsync();
            if (organizeExam == null) return null;

            var subject = await _subjects.Find(s => s.Id == organizeExam.SubjectId).FirstOrDefaultAsync();
            if (subject == null) return null;
            
            return subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == organizeExam.QuestionBankId);
        }
    }
}