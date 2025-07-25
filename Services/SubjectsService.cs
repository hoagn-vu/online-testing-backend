using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Services;

using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

public class SubjectsService
{
    //private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
    //private readonly IMongoCollection<UsersModel> _usersCollection;
    //private readonly IMongoCollection<LogsModel> _logsCollection;

    private readonly SubjectRepository _subjectRepository;

    public SubjectsService(SubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    // Find all
    public async Task<(List<SubjectDto>, long)> GetSubjects(string? keyword, int page, int pageSize)
    {
        var totalCount = await _subjectRepository.CountAsync(keyword);
        var subjects = await _subjectRepository.GetSubjectsAsync(keyword, page, pageSize);

        return (subjects, totalCount);
    }

    public async Task<List<SubjectOptionsDto>> GetAllSubjects()
    {
        return await _subjectRepository.GetSubjectsNameAsync();
    }

    // Search or Get All Question Bank Name
    public async Task<(string, string?, List<QuestionBankDto>, long)> GetQuestionBanks(string subjectId, string? keyword, int page, int pageSize)
    {
        // var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
        //var filter = Builders<SubjectsModel>.Filter.And(
        //    Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
        //    Builders<SubjectsModel>.Filter.Ne(s => s.SubjectStatus, "deleted")
        //);

        //if (!string.IsNullOrEmpty(keyword))
        //{
        //    filter = Builders<SubjectsModel>.Filter.And(
        //        filter,
        //        Builders<SubjectsModel>.Filter.ElemMatch(
        //            s => s.QuestionBanks,
        //            Builders<QuestionBanksModel>.Filter.Regex(q => q.QuestionBankName, new BsonRegularExpression(keyword, "i"))));
        //}

        //var subjects = await this._subjectsCollection
        //    .Find(filter)
        //    .ToListAsync();
        var subjects = await _subjectRepository.GetAllQuestionBanksAsync(subjectId, keyword, page, pageSize);

        var questionBanks = subjects
        .SelectMany(s => s.QuestionBanks
            .Where(qb => string.IsNullOrEmpty(keyword) || qb.QuestionBankName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) && !qb.QuestionBankStatus.Equals("deleted", StringComparison.CurrentCultureIgnoreCase))
            .Select(qb => new QuestionBankDto
            {
                QuestionBankId = qb.QuestionBankId,
                QuestionBankName = qb.QuestionBankName,
                TotalQuestions = qb.QuestionList.Count
            }))
        .ToList();

        var totalQuestionBanks = questionBanks.Count;
        var subjectName = subjects.FirstOrDefault()?.SubjectName;

        return (subjectId, subjectName, questionBanks, totalQuestionBanks);
    }

    public async Task<List<QuestionBankOptionsDto>> GetQuestionBanksPerSubject(string subjectId)
    {
        // var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
        //var filter = Builders<SubjectsModel>.Filter.And(
        //    Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
        //    Builders<SubjectsModel>.Filter.Ne(s => s.SubjectStatus, "deleted")
        //);

        //var subjects = await this._subjectsCollection
        //    .Find(filter)
        //    .ToListAsync();
        var subjects = await _subjectRepository.GetQuestionBankAsync(subjectId);

        var questionBanks = subjects
        .SelectMany(s => s.QuestionBanks
            .Where(qb => !qb.QuestionBankStatus.Equals("deleted", StringComparison.CurrentCultureIgnoreCase))
            .Select(qb => new QuestionBankOptionsDto
            {
                QuestionBankId = qb.QuestionBankId,
                QuestionBankName = qb.QuestionBankName,
            }))
        .ToList();

        return questionBanks;
    }

    // Get questions
    public async Task<(string, string, string, string, List<string>, List<string>, List<QuestionModel>, long)> GetQuestions(string subjectId, string questionBankId, string? keyWord, int page, int pageSize)
    {
        //var filter = Builders<SubjectsModel>.Filter.And(
        //    Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
        //    Builders<SubjectsModel>.Filter.Ne(s => s.SubjectStatus, "deleted")
        //);

        //var subject = await this._subjectsCollection
        //   .Find(filter)
        //   .FirstOrDefaultAsync();
        var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);

        var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);

        if (questionBank == null)
        {
            throw new Exception("Question bank not found.");
        }

        var filteredQuestions = string.IsNullOrEmpty(keyWord)
            ? questionBank.QuestionList
            : questionBank.QuestionList.Where(q => q.QuestionText.Contains(keyWord, StringComparison.OrdinalIgnoreCase))
                               .ToList();


        var totalCount = filteredQuestions?.Count ?? 0;

        var paginatedQuestions = (filteredQuestions ?? [])
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (subject.Id, subject.SubjectName, questionBank.QuestionBankId, questionBank.QuestionBankName, questionBank.AllChapter, questionBank.AllLevel, paginatedQuestions, totalCount);
    }

    // Add subject
    public async Task<string> AddSubject(string subjectName)
    {
        try
        {
            var subject = new SubjectsModel
            {
                SubjectName = subjectName,
                QuestionBanks = [],
            };

            await _subjectRepository.AddSubjectAsync(subject);
            return "Add subject successfully";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Update subject
    public async Task<string> UpdateSubject(string subjectId, string subjectName)
    {
        try
        {
            await _subjectRepository.UpdateSubjectAsync(subjectId, subjectName);
            return "Update subject successfully";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Add question bank
    public async Task<string> AddQuestionBank(string subjectId, string questionBankName)
    {
        try
        {
            var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null)
            {
                return "Not found subject";
            }

            var newQuestionBank = new QuestionBanksModel
            {
                QuestionBankName = questionBankName,
                QuestionList = [],
            };

            subject.QuestionBanks.Add(newQuestionBank);

            await _subjectRepository.AddQuestionBankAsync(subject, subjectId);
            return $"Add question bank successfully";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Add Question
    public async Task<string> AddQuestion(string subjectId, string questionBankId, string userId, SubjectQuestionDto question)
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
        if (subject == null)
        {
            return "Not found subject";
        }

        var questionBank = subject.QuestionBanks.Find(qb => qb.QuestionBankId == questionBankId);
        if (questionBank == null)
        {
            return "Not found question bank";
        }

        var newQuestion = new QuestionModel
        {
            Options = question.Options,
            QuestionType = question.QuestionType,
            QuestionText = question.QuestionText,
            QuestionStatus = question.QuestionStatus,
            IsRandomOrder = question.IsRandomOrder,
            Tags = question.Tags,
        };

        questionBank.QuestionList.Add(newQuestion);
        if (!questionBank.AllChapter.Contains(newQuestion.Tags[0]))
        {
            questionBank.AllChapter.Add(newQuestion.Tags[0]);
        }
        if (!questionBank.AllLevel.Contains(newQuestion.Tags[1]))
        {
            questionBank.AllLevel.Add(newQuestion.Tags[1]);
        }

        await _subjectRepository.AddQuestionAsync(subjectId, subject);
        //var logInsert = new LogsModel
        //{
        //    MadeBy = userId,
        //    LogAction = "create",
        //    LogDetails = "Tạo câu hỏi: " + question.QuestionText
        //};
        //await _logsCollection.InsertOneAsync(logInsert);

        // var user = _usersCollection.Find(u => u.Id == userId).FirstOrDefault();
        // if (user == null) return $"Add question list successfully";
        // user.UserLog ??= [];
        //
        // user.UserLog.Add(new UserLogsModel
        // {
        //     LogAction = "create",
        //     LogDetails = "Tạo câu hỏi: " + question.QuestionText
        // });
        //
        // var updateLogUser = Builders<UsersModel>.Update.Set(u => u.UserLog, user.UserLog);
        // await _usersCollection.UpdateOneAsync(u => u.Id == userId, updateLogUser);

        return $"Thêm câu hỏi thành công";
    }

    // Update Subject Name
    public async Task<string> UpdateSubjectName(string subjectId, string subjectName)
    {
        try
        {
            var result = await _subjectRepository.UpdateSubjectNameAsync(subjectId, subjectName);

            return result.ModifiedCount > 0 ? "Update subject name successfully" : "Subject not found or no changes made";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Update Question Bank Name
    public async Task<string> UpdateQuestionBankName(string subjectId, string questionBankId, string questionBankName)
    {
        try
        {
            var result = await _subjectRepository.UpdateQuestionBankNameAsync(subjectId, questionBankId, questionBankName);

            if (result.ModifiedCount > 0)
            {
                return "Update question bank name successfully";
            }

            return "Question bank not found or no changes made";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Update Question List
    public async Task<string> UpdateQuestion(string subjectId, string questionBankId, string questionId, string userId, SubjectQuestionDto questionData)
    {
        try
        {
            //var subject = await this._subjectsCollection.Find(s => s.Id == id).FirstOrDefaultAsync();
            var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
            if (subject == null)
            {
                return "Subject not found";
            }

            // Find question bank
            var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
            if (questionBank == null)
            {
                return "QuestionBank not found";
            }

            // Find question
            var questionIndex = questionBank.QuestionList.FindIndex(q => q.QuestionId == questionId);
            if (questionIndex == -1)
            {
                return "Question not found";
            }

            // Update question data
            questionBank.QuestionList[questionIndex].Options = questionData.Options;
            questionBank.QuestionList[questionIndex].QuestionType = questionData.QuestionType;
            questionBank.QuestionList[questionIndex].QuestionStatus = questionData.QuestionStatus;
            questionBank.QuestionList[questionIndex].QuestionText = questionData.QuestionText;
            questionBank.QuestionList[questionIndex].IsRandomOrder = questionData.IsRandomOrder;
            questionBank.QuestionList[questionIndex].Tags = questionData.Tags;

            // Update data
            //var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
            //var result = await _subjectsCollection.UpdateOneAsync(s => s.Id == id, update);
            var result = await _subjectRepository.UpdateQuestionsAsync(subjectId, subject);

            //var logInsert = new LogsModel
            //{
            //    MadeBy = userId,
            //    LogAction = "create",
            //    LogDetails = "Cập nhật câu hỏi có id  " + questionId
            //};
            //await _logsCollection.InsertOneAsync(logInsert);

            return result.ModifiedCount > 0 ? "Update question successfully" : "Update failed";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Delete subject
    public async Task<string> DeleteSubject(string subjectId)
    {
        try
        {
            //var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);

            //var update = Builders<SubjectsModel>.Update.Set(s => s.SubjectStatus, "Deleted/Disable");

            //var result = await this._subjectsCollection.UpdateOneAsync(filter, update);
            var result = await _subjectRepository.DeleteSubject(subjectId);

            if (result.ModifiedCount > 0)
            {
                return "Delete subject successfully";
            }
            else
            {
                return "Subject not found";
            }
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Delete question bank
    public async Task<string> DeleteQuestionBank(string subjectId, string questionBankId)
    {
        try
        {
            //var filter = Builders<SubjectsModel>.Filter.And(
            //    Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId),
            //    Builders<SubjectsModel>.Filter.ElemMatch(s => s.QuestionBanks, qb => qb.QuestionBankId == questionBankId));

            //var update = Builders<SubjectsModel>.Update
            //    .Set("questionBanks.$.questionBankStatus", "Deleted/Disable"); // Hoặc "Disabled"

            //var result = await this._subjectsCollection.UpdateOneAsync(filter, update);
            var result = await _subjectRepository.DeleteQuestionBank(subjectId, questionBankId);
            if (result.ModifiedCount > 0)
            {
                return "Delete question bank successfully";
            }

            return "Not found question bank";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    // Delete question in question list
    public async Task<string> DeleteQuestion(string subjectId, string questionBankId, string questionId, string userLogId)
    {
        //var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
        //var subject = await this._subjectsCollection.Find(filter).FirstOrDefaultAsync();
        var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
        if (subject == null)
        {
            return "Subject not found";
        }

        var questionBank = subject.QuestionBanks?.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
        if (questionBank == null)
        {
            return "Question bank not found";
        }

        var question = questionBank.QuestionList.FirstOrDefault(q => q.QuestionId == questionId);
        if (question == null)
        {
            return "Question not found";
        }

        question.QuestionStatus = "deleted";

        //var update = Builders<SubjectsModel>.Update.Set(s => s.QuestionBanks, subject.QuestionBanks);
        //var result = await this._subjectsCollection.UpdateOneAsync(filter, update);
        var result = await _subjectRepository.DeleteQuestion(subjectId, subject);

        if (result.ModifiedCount > 0)
        {
            return "Question disabled successfully";
        }
        else
        {
            return "Update failed or no changes were made";
        }
    }

    // Classification tag for create matrix
    public async Task<List<TagsClassification>> GetTagsClassificationAsync(string subjectId, string questionBankId)
    {
        //var subject = await _subjectsCollection.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
        var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
        if (subject == null) return [];

        var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
        if (questionBank == null) return [];

        var tagsDictionary = new Dictionary<(string Chapter, string Level), int>();

        foreach (var question in questionBank.QuestionList)
        {
            if (question.Tags is { Count: >= 2 })
            {
                var key = (question.Tags[0], question.Tags[1]);
                if (!tagsDictionary.TryAdd(key, 1))
                {
                    tagsDictionary[key]++;
                }
            }
        }

        return tagsDictionary.Select(kvp => new TagsClassification
        {
            Chapter = kvp.Key.Chapter,
            Level = kvp.Key.Level,
            Total = kvp.Value
        }).ToList();
    }
}