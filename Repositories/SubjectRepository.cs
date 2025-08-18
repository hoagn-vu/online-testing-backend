using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Reflection.Metadata.Ecma335;

namespace Backend_online_testing.Repositories;

public class SubjectRepository
{
    private readonly IMongoCollection<SubjectsModel> _subjects;
    private readonly IMongoCollection<UsersModel> _users;
    //private readonly IMongoCollection<LogsModel> _logs;

    public SubjectRepository(IMongoDatabase database)
    {
        _subjects = database.GetCollection<SubjectsModel>("subjects");
        _users = database.GetCollection<UsersModel>("users");
        //_logs = database.GetCollection<LogsModel>("logs");
    }

    //Filter definition
    //Filter subject base
    private static readonly FilterDefinition<SubjectsModel> SubjectBaseFilter =
        Builders<SubjectsModel>.Filter.Ne(s => s.SubjectStatus, "deleted");

    private static readonly SortDefinition<SubjectsModel> SubjectBaseSort =
        Builders<SubjectsModel>.Sort.Descending("_id");

    private static readonly FilterDefinition<QuestionBanksModel> QuestionBankBaseFilter =
        Builders<QuestionBanksModel>.Filter.Ne(qb => qb.QuestionBankStatus, "deleted");

    private static readonly SortDefinition<QuestionBanksModel> QuestionBankBaseSort =
        Builders<QuestionBanksModel>.Sort.Descending("_id");

    //Filter by subject name
    private FilterDefinition<SubjectsModel> SubjectFilterByName(string? keyword)
    {
        var builder = Builders<SubjectsModel>.Filter;

        if (string.IsNullOrEmpty(keyword))
        {
            return SubjectBaseFilter;
        }

        return builder.And(
            SubjectBaseFilter,
            builder.Regex(s => s.SubjectName, new BsonRegularExpression(keyword, "i"))
        );
    }

    //Filter by subject id
    private FilterDefinition<SubjectsModel> SubjectFilterById(string? subjectId)
    {
        var builder = Builders<SubjectsModel>.Filter;
        return builder.And(
            builder.Eq(s => s.Id, subjectId),
            SubjectBaseFilter
        );
    }

    //Count document
    public async Task<long> CountAsync(string? keyword)
    {
        var filter = SubjectFilterByName(keyword);

        return await _subjects.CountDocumentsAsync(filter);
    }

    //Get all subject with subject name, subject status, question bank
    public async Task<List<SubjectDto>> GetSubjectsAsync(string? keyword, int page, int pageSize)
    {
        //Filter
        var filter = SubjectFilterByName(keyword);

        //Get necessary filed
        var projection = Builders<SubjectsModel>.Projection
            .Expression(sub => new SubjectDto
            {
                Id = sub.Id,
                SubjectName = sub.SubjectName,
                SubjectStatus = sub.SubjectStatus,
                TotalQuestionBanks = sub.QuestionBanks.Count(qb => qb.QuestionBankStatus != "deleted")
            });

        return await _subjects
            .Find(filter)
            .Sort(SubjectBaseSort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .Project(projection)
            .ToListAsync();
    }

    //Get all subject with subject name
    public async Task<List<SubjectOptionsDto>> GetSubjectsNameAsync()
    {
        //Filter
        var filter = SubjectFilterByName("");

        //Get necessary filed
        var projection = Builders<SubjectsModel>.Projection
            .Expression(sub => new SubjectOptionsDto
            {
                Id = sub.Id,
                SubjectName = sub.SubjectName,
            });

        return await _subjects
            .Find(filter)
            .Sort(SubjectBaseSort)
            .Project(projection)
            .ToListAsync();
    }

    //Search or get all question bank name
    public async Task<List<SubjectsModel>> GetAllQuestionBanksAsync(string subjectId, string? keyword, int page, int pageSize)
    {
        var filter = SubjectFilterById(subjectId);

        var qbFilter = Builders<QuestionBanksModel>.Filter.Ne(q => q.QuestionBankStatus, "deleted");

        if (!string.IsNullOrEmpty(keyword))
        {
            qbFilter = Builders<QuestionBanksModel>.Filter.And(
                qbFilter,
                Builders<QuestionBanksModel>.Filter.Regex(
                    q => q.QuestionBankName,
                    new BsonRegularExpression(keyword, "i"))
            );
        }

        filter = Builders<SubjectsModel>.Filter.And(
            filter,
            Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qbFilter)
        );

        return await _subjects
            .Find(filter)
            .Sort(SubjectBaseSort)
            // .Skip((page - 1) * pageSize)
            // .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<QuestionBankPerSubjectDto?> GetQuestionBanksAsync(string subjectId, string? keyword, int page, int pageSize)
    {
        var filter = SubjectFilterById(subjectId);

        var subject = await _subjects
            .Find(filter)
            .FirstOrDefaultAsync();

        if (subject == null)
            return null;

        var questionBanks = subject.QuestionBanks
            .Where(qb => qb.QuestionBankStatus != "deleted");

        if (!string.IsNullOrEmpty(keyword))
        {
            questionBanks = questionBanks
                .Where(qb => qb.QuestionBankName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        questionBanks = questionBanks
            .OrderByDescending(qb => qb.QuestionBankId);

        // Phân trang
        // questionBanks = questionBanks
        //     .Skip((page - 1) * pageSize)
        //     .Take(pageSize);

        var dto = new QuestionBankPerSubjectDto
        {
            SubjectId = subject.Id,
            SubjectName = subject.SubjectName,
            QuestionBanks = questionBanks.Select(qb => new QuestionBanksDto
            {
                QuestionBankId = qb.QuestionBankId,
                QuestionBankName = qb.QuestionBankName,
                TotalQuestions = qb.QuestionList?.Count ?? 0
            }).ToList()
        };

        return dto;
    }

    //Get question bank in subject (using for GetQuetionBanksPerSubject)
    public async Task<List<SubjectsModel>> GetQuestionBankAsync(string subjectId)
    {
        var filter = Builders<SubjectsModel>.Filter.And(
            SubjectFilterById(subjectId),
            Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb => qb.QuestionBankStatus != "deleted")
        );

        return await _subjects
            .Find(filter)
            .Sort(SubjectBaseSort)
            .ToListAsync();
    }

    //Get subject by id (using for GetQuestion)
    public async Task<SubjectsModel> GetSubjectByIdAsync(string subjectId)
    {
        var filter = SubjectFilterById(subjectId);

        return await _subjects
            .Find(filter)
            .FirstOrDefaultAsync();
    }

    //Add subject
    public async Task AddSubjectAsync(SubjectsModel subject)
    {
        await _subjects.InsertOneAsync(subject);
    }

    //Update subject name
    public async Task<UpdateResult> UpdateSubjectAsync(string? subjectId, string subjectName)
    {
        var filter = SubjectFilterById(subjectId);
        var update = Builders<SubjectsModel>.Update.Set(s => s.SubjectName, subjectName);

        return await _subjects.UpdateOneAsync(filter, update);
    }

    //Add question bank
    public async Task<UpdateResult> AddQuestionBankAsync(SubjectsModel subject, string subjectId)
    {
        var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
        return await _subjects.UpdateOneAsync(s => s.Id == subjectId, update);
    }

    //Add question
    public async Task<UpdateResult> AddQuestionAsync(string subjectId, SubjectsModel subject)
    {
        var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);

        return await _subjects.UpdateOneAsync(s => s.Id == subjectId, update);
    }

    //Update subject name
    public async Task<UpdateResult> UpdateSubjectNameAsync(string subjectId, string subjectName)
    {
        var filter = SubjectFilterById(subjectId);
        var update = Builders<SubjectsModel>.Update.Set(s => s.SubjectName, subjectName);

        return await _subjects.UpdateOneAsync(filter, update);
    }

    //Update question bank name
    public async Task<UpdateResult> UpdateQuestionBankNameAsync(string subjectId, string questionBankId, string questionBankName)
    {
        var filter = SubjectFilterById(subjectId);
        filter = Builders<SubjectsModel>.Filter.And(
            filter,
            Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb => qb.QuestionBankId == questionBankId)
        );

        var update = Builders<SubjectsModel>.Update.Set("QuestionBanks.$.QuestionBankName", questionBankName);

        return await _subjects.UpdateOneAsync(filter, update);
    }

    //Update question list
    public async Task<UpdateResult> UpdateQuestionsAsync(string subjectId, SubjectsModel subject)
    {
        var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
        return await _subjects.UpdateOneAsync(s => s.Id == subjectId, update);
    }

    //Delete subject
    public async Task<UpdateResult> DeleteSubject(string subjectId)
    {
        var filter = SubjectFilterById(subjectId);
        var update = Builders<SubjectsModel>.Update.Set(s => s.SubjectStatus, "deleted");

        return await _subjects.UpdateOneAsync(filter, update);
    }

    //Delete question bank
    public async Task<UpdateResult> DeleteQuestionBank(string subjectId, string questionBankId)
    {
        var filter = SubjectFilterById(subjectId);
        filter = Builders<SubjectsModel>.Filter.And(
            filter,
            Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb => qb.QuestionBankId == questionBankId)
        );

        var update = Builders<SubjectsModel>.Update
                .Set("questionBanks.$.questionBankStatus", "deleted");

        return await _subjects.UpdateOneAsync(filter, update);
    }

    //Delete question in question list
    public async Task<UpdateResult> DeleteQuestion(string subjectId, SubjectsModel subject)
    {
        var filter = SubjectFilterById(subjectId);
        var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);

        return await _subjects.UpdateOneAsync(filter, update);
    }
}