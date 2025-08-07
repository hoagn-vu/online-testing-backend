using Backend_online_testing.DTO;
using Backend_online_testing.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories;

public class ExamRepository
{
    private readonly IMongoCollection<ExamsModel> _exams;
    private readonly IMongoCollection<SubjectsModel> _subjects;

    public ExamRepository(IMongoDatabase database)
    {
        _exams = database.GetCollection<ExamsModel>("exams");
        _subjects = database.GetCollection<SubjectsModel>("subjects");
    }

    //Filter definition
    private FilterDefinition<ExamsModel> FilterById(string examId) =>
        Builders<ExamsModel>.Filter.Eq(e => e.Id, examId);

    //Get subject information
    public async Task<SubjectsModel> GetSubjectByIdAsync(ExamsModel exam)
    {
        return await _subjects.Find(s => s.Id == exam.SubjectId).FirstOrDefaultAsync();
    }

    //Count document
    public async Task<long> CountAsync(string? keyword)
    {
        var filter = Builders<ExamsModel>.Filter.Ne(ex => ex.ExamStatus, "deleted");
        if (!string.IsNullOrEmpty(keyword))
        {
            filter = Builders<ExamsModel>.Filter.Or(
                Builders<ExamsModel>.Filter.Regex(ex => ex.ExamName, new BsonRegularExpression(keyword, "i")),
                Builders<ExamsModel>.Filter.Regex(ex => ex.ExamCode, new BsonRegularExpression(keyword, "i")));
        }

        return await _exams.CountDocumentsAsync(filter);
    }

    //Find all document
    public async Task<List<ExamsModel>> GetExamsAsync(string? keyword, int skip, int pageSize)
    {
        var filter = Builders<ExamsModel>.Filter.Ne(ex => ex.ExamStatus, "deleted");
        if (!string.IsNullOrEmpty(keyword))
        {
            filter = Builders<ExamsModel>.Filter.Or(
                Builders<ExamsModel>.Filter.Regex(ex => ex.ExamName, new BsonRegularExpression(keyword, "i")),
                Builders<ExamsModel>.Filter.Regex(ex => ex.ExamCode, new BsonRegularExpression(keyword, "i")));
        }

        return await _exams
            .Find(filter)
            .Skip((skip - 1) * pageSize)
            .Limit(pageSize)
            .SortByDescending(r => r.Id)
            .ToListAsync();
    }

    //Find by name
    public async Task<ExamsModel?> GetExamByNameAsync(string examName)
    {
        return await _exams.Find(e => e.ExamName == examName).FirstOrDefaultAsync();
    }

    //Get exam by id
    public async Task<ExamsModel?> GetByIdAsync(string examId)
    {
        return await _exams.Find(FilterById(examId)).FirstOrDefaultAsync();
    }

    //Create exam
    public async Task InsertExamAsync(ExamsModel exam)
    {
        await _exams.InsertOneAsync(exam);
    }

    //Update exam
    public async Task<UpdateResult> UpdateExamAsync(ExamDto updateExamData, string examId, string userLogId)
    {
        var update = Builders<ExamsModel>.Update
            .Set(e => e.ExamName, updateExamData.ExamName)
            .Set(e => e.ExamCode, updateExamData.ExamCode)
            .Set(e => e.SubjectId, updateExamData.SubjectId)
            .Set(e => e.ExamStatus, updateExamData.ExamStatus)
            .Set(e => e.QuestionBankId, updateExamData.QuestionBankId);

        return await _exams.UpdateOneAsync(FilterById(examId), update);
    }

    //Add question one/list
    public async Task<UpdateResult> AddQuestionAsync(string examId, IEnumerable<QuestionSetsModel> questions)
    {
        var updateQuestions = Builders<ExamsModel>.Update
            .PushEach<QuestionSetsModel>(nameof(ExamsModel.QuestionSet), questions);

        return await _exams.UpdateOneAsync(FilterById(examId), updateQuestions);
    }

    //Update question
    public async Task<UpdateResult> UpdateExamQuestionAsync(string examId, string questionId, double questionScore)
    {
        var filter = Builders<ExamsModel>.Filter.And(
            FilterById(examId),
            Builders<ExamsModel>.Filter.ElemMatch(e => e.QuestionSet, ps => ps.QuestionId == questionId)
        );

        var update = Builders<ExamsModel>.Update.Set("QuestionSet.$.QuestionScore", questionScore);

        return await _exams.UpdateOneAsync(filter, update);
    }

    //Delete question exam
    public async Task<UpdateResult> DeleteExamQuestionAsync(string examId, string questionId)
    {
        var filter = Builders<ExamsModel>.Filter.And(
            FilterById(examId),
            Builders<ExamsModel>.Filter.ElemMatch(e => e.QuestionSet, qs => qs.QuestionId == questionId));

        var delete = Builders<ExamsModel>.Update.PullFilter(e => e.QuestionSet, qs => qs.QuestionId == questionId);

        return await _exams.UpdateOneAsync(filter, delete);
    }
    
    public async Task<List<ExamOptionsDTO>> GetExamOptionsAsync(string? subjectId)
    {
        var filters = new List<FilterDefinition<ExamsModel>>
        {
            Builders<ExamsModel>.Filter.Ne(r => r.ExamStatus, "deleted")
        };

        if (!string.IsNullOrEmpty(subjectId))
        {
            filters.Add(Builders<ExamsModel>.Filter.Eq(r => r.SubjectId, subjectId));
        }

        var filter = Builders<ExamsModel>.Filter.And(filters);

        var projection = Builders<ExamsModel>.Projection
            .Expression(e => new ExamOptionsDTO
            {
                Id = e.Id,
                ExamCode = e.ExamCode,
                ExamName = e.ExamName,
            });

        return await _exams.Find(filter).Project(projection).ToListAsync();
    }
}