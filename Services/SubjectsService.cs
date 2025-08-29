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
    private readonly IS3Service _s3Service;
    
    public SubjectsService(SubjectRepository subjectRepository, IS3Service s3Service)
    {
        _subjectRepository = subjectRepository;
        _s3Service = s3Service;
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
    public async Task<QuestionBankPerSubjectDto?> GetQuestionBanks(string subjectId, string? keyword, int page, int pageSize)
    {
        var questionBankPerSubject = await _subjectRepository.GetQuestionBanksAsync(subjectId, keyword, page, pageSize);
        return questionBankPerSubject ?? null;
    }

    public async Task<List<QuestionBankOptionsDto>> GetQuestionBanksPerSubject(string subjectId)
    {
        // var filter = Builders<SubjectsModel>.Filter.Eq(s => s.Id, subjectId);
        //var filter = Builders<SubjectsModel>. Filter.And(
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
            ? questionBank.QuestionList.OrderByDescending(q => q.QuestionId).ToList()
            : questionBank.QuestionList.Where(q => q.QuestionText.Contains(keyWord, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var totalCount = filteredQuestions?.Count ?? 0;

        var paginatedQuestions = (filteredQuestions ?? [])
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (subject.Id, subject.SubjectName, questionBank.QuestionBankId, questionBank.QuestionBankName, questionBank.AllChapter, questionBank.AllLevel, paginatedQuestions, totalCount);
    }
    
    // Get question by id
    public async Task<QuestionModel?> GetQuestion(string subjectId, string questionBankId, string questionId)
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);

        var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);

        if (questionBank == null)
        {
            throw new Exception("Question bank not found.");
        }

        var filteredQuestions = questionBank.QuestionList.Where(q => q.QuestionId == questionId).ToList();
        
        return filteredQuestions.FirstOrDefault();
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
    public async Task<(string?, string?, string?)> UpdateSubject(string subjectId, SubjectRequestDto? request)
    {
        // var subject = await _subjectRepository.GetById(subjectId);
        // if (subject == null)
        // {
        //     return ("error", "Subject not found", null);
        // }

        var updated = await _subjectRepository.UpdateSubjectAsync(subjectId, request);

        if (updated == null)
        {
            return ("error", "Update failed", null);
        }

        return ("success", "Subject updated successfully", updated.SubjectName);
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
    public async Task<string> AddQuestion(string subjectId, string questionBankId, SubjectQuestionDto question)
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
            ImgLinks = question.ImgLinks ?? new List<string>()
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
        return $"Thêm câu hỏi thành công";
    }
    
    // Add Questions
    public async Task<string> AddQuestions(string subjectId, string questionBankId, List<SubjectQuestionDto> questions)
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

        foreach (var question in questions)
        {
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

            // Chỉ thêm nếu tag không rỗng
            var chapter = newQuestion.Tags[0];
            var level = newQuestion.Tags[1];

            if (!string.IsNullOrWhiteSpace(chapter) && !questionBank.AllChapter.Contains(chapter))
            {
                questionBank.AllChapter.Add(chapter);
            }

            if (!string.IsNullOrWhiteSpace(level) && !questionBank.AllLevel.Contains(level))
            {
                questionBank.AllLevel.Add(level);
            }
        }

        await _subjectRepository.AddQuestionAsync(subjectId, subject);
        return $"ok";
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
    
    public async Task<(string? status, string? message, QuestionBanksModel? questionBank)> UpdateQuestionBankAsync(UpdateQuestionBankRequestDto request)
    {
        var updated = await _subjectRepository.UpdateQuestionBankAsync(request);

        if (updated == null)
        {
            return ("error", "Subject or QuestionBank not found", null);
        }

        return ("success", "QuestionBank updated successfully", updated);
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
    public async Task<List<TagsClassification>> GetTagsClassificationAsync(
        string subjectId, 
        string questionBankId, 
        string type = "both")
    {
        var subject = await _subjectRepository.GetSubjectByIdAsync(subjectId);
        if (subject == null) return [];

        var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == questionBankId);
        if (questionBank == null) return [];

        // Trường hợp both: giữ nguyên logic cũ
        if (type.Equals("both", StringComparison.OrdinalIgnoreCase))
        {
            var tagsDictionary = new Dictionary<(string Chapter, string Level), int>();

            foreach (var question in questionBank.QuestionList)
            {
                if (question.Tags is { Count: >= 2 })
                {
                    var key = (Chapter: question.Tags[0], Level: question.Tags[1]);
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

        // Trường hợp level: chỉ lấy level duy nhất
        if (type.Equals("level", StringComparison.OrdinalIgnoreCase))
        {
            var levels = questionBank.QuestionList
                .Where(q => q.Tags is { Count: >= 2 })
                .Select(q => q.Tags[1]) // Level nằm ở vị trí [1]
                .Distinct()
                .Select(l => new TagsClassification
                {
                    Chapter = string.Empty,
                    Level = l,
                    Total = questionBank.QuestionList.Count(q => q.Tags is { Count: >= 2 } && q.Tags[1] == l)
                })
                .ToList();

            return levels;
        }

        // Trường hợp chapter: chỉ lấy chapter duy nhất
        if (type.Equals("chapter", StringComparison.OrdinalIgnoreCase))
        {
            var chapters = questionBank.QuestionList
                .Where(q => q.Tags is { Count: >= 2 })
                .Select(q => q.Tags[0]) // Chapter nằm ở vị trí [0]
                .Distinct()
                .Select(c => new TagsClassification
                {
                    Chapter = c,
                    Level = string.Empty,
                    Total = questionBank.QuestionList.Count(q => q.Tags is { Count: >= 2 } && q.Tags[0] == c)
                })
                .ToList();

            return chapters;
        }

        return [];
    }
    
    public async Task<bool> AddQuestionsAsync(AddQuestionsRequestDto request)
    {
        return await _subjectRepository.AddQuestionsAsync(request);
    }
    
    public async Task<bool> AddQuestionsWithImagesAsync(string subjectId, string questionBankId, List<AddSubjectQuestionWithImageDto> questions)
    {
        foreach (var q in questions)
        {
            var imgLinks = new List<string>();

            if (q.Images is { Count: > 0 })
            {
                foreach (var file in q.Images)
                {
                    var link = await _s3Service.UploadFileAsync(file);
                    imgLinks.Add(link);
                }
            }

            // Gán lại link ảnh sau khi upload
            q.ImgLinks = imgLinks;
            q.Images = null; // tránh serialize thừa
        }

        return await _subjectRepository.AddQuestionsWithImagesAsync(subjectId, questionBankId, questions);
    }
}
