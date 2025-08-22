using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories
{
    public interface IGenerateExamRepository
    {
        Task<OrganizeExamModel?> GetOrganizeExamByIdAsync(string organizeExamId);
        Task<SubjectsModel?> GetSubjectByIdAsync(string subjectId);
        Task<UsersModel?> GetUserByIdAsync(string userId);
        Task UpdateUserTakeExamAsync(string userId, TakeExamsModel updatedTakeExam);
        Task<ExamsModel?> GetExamByIdAsync(string examId);
    }

    public class GenerateExamRepository : IGenerateExamRepository
    {
        private readonly IMongoCollection<OrganizeExamModel> _organizeExamCollection;
        private readonly IMongoCollection<SubjectsModel> _subjectCollection;
        private readonly IMongoCollection<UsersModel> _userCollection;
        private readonly IMongoCollection<ExamsModel> _examCollection;

        public GenerateExamRepository(IMongoDatabase database)
        {
            _organizeExamCollection = database.GetCollection<OrganizeExamModel>("organizeExams");
            _subjectCollection = database.GetCollection<SubjectsModel>("subjects");
            _userCollection = database.GetCollection<UsersModel>("users");
            _examCollection = database.GetCollection<ExamsModel>("exams");
        }

        public async Task<OrganizeExamModel?> GetOrganizeExamByIdAsync(string organizeExamId)
        {
            return await _organizeExamCollection
                .Find(o => o.Id == organizeExamId)
                .FirstOrDefaultAsync();
            // var filter = Builders<OrganizeExamModel>.Filter.And(
            //     Builders<OrganizeExamModel>.Filter.Eq(oe => oe.Id, organizeExamId),
            //     Builders<OrganizeExamModel>.Filter.Ne(s => s.OrganizeExamStatus, "deleted")
            // );
            //
            // var exam = await _organizeExamCollection.Find(filter).FirstOrDefaultAsync();
            // return exam;
        }

        public async Task<SubjectsModel?> GetSubjectByIdAsync(string subjectId)
        {
            return await _subjectCollection
                .Find(s => s.Id == subjectId)
                .FirstOrDefaultAsync();
        }
        
        public async Task<UsersModel?> GetUserByIdAsync(string userId)
        {
            return await _userCollection
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateUserTakeExamAsync(string userId, TakeExamsModel updatedTakeExam)
        {
            var filter = Builders<UsersModel>.Filter.And(
                Builders<UsersModel>.Filter.Eq(u => u.Id, userId),
                Builders<UsersModel>.Filter.ElemMatch(u => u.TakeExam,
                    t => t.OrganizeExamId == updatedTakeExam.OrganizeExamId
                         && t.SessionId == updatedTakeExam.SessionId
                         && t.RoomId == updatedTakeExam.RoomId
                )
            );

            var update = Builders<UsersModel>.Update
                .Set("takeExams.$.startAt", updatedTakeExam.StartAt)
                .Set("takeExams.$.status", updatedTakeExam.Status)
                .Set("takeExams.$.answers", updatedTakeExam.Answers);

            await _userCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task<ExamsModel?> GetExamByIdAsync(string examId)
            => await _examCollection.Find(e => e.Id == examId).FirstOrDefaultAsync();
        
    }
}
