namespace Backend_online_testing.Services;

using Backend_online_testing.DTO;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using Backend_online_testing.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

public class ExamsService
{
    private readonly ExamRepository _examsRepository;

    public ExamsService(ExamRepository examRepository)
    {
        _examsRepository = examRepository;
    }
    
    // Find all document
    public async Task<(List<ExamResponseDto>, long)> GetExams(string? keyword, int skip, int pageSize)
    {
        var totalCount = await _examsRepository.CountAsync(keyword);
        var exams = await _examsRepository.GetExamsAsync(keyword, skip, pageSize);

        var examResponseList = new List<ExamResponseDto>();

        foreach (var exam in exams)
        {
            // Get subject information
            var subject = await _examsRepository.GetSubjectsAsync(exam);
            var subjectName = subject?.SubjectName ?? string.Empty;

            // Lấy thông tin ngân hàng câu hỏi từ danh sách QuestionBanks trong Subject
            var questionBankName = string.Empty;
            if (subject != null)
            {
                var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == exam.QuestionBankId);
                questionBankName = questionBank?.QuestionBankName ?? string.Empty;
            }

            examResponseList.Add(new ExamResponseDto
            {
                Id = exam.Id,
                ExamCode = exam.ExamCode,
                ExamName = exam.ExamName,
                SubjectId = exam.SubjectId,
                SubjectName = subjectName,
                ExamStatus = exam.ExamStatus,
                QuestionBankId = exam.QuestionBankId,
                QuestionBankName = questionBankName
            });
        }

        return (examResponseList, totalCount);
    }

    // Create Exam
    public async Task<string> CreateExam(ExamDto createExamData)
    {
        // Check name exam is existed
        var existingExam = await _examsRepository.GetExamByNameAsync(createExamData.ExamName);
        if (existingExam != null)
        {
            return "Exam name already exists.";
        }

        // var logCreateData = new ExamLogsModel
        // {
        //    ExamLogUserId = createExamData.ExamLogUserId,
        //    ExamLogType = "Created",
        //    ExamChangeAt = DateTime.Now,
        // };
        var newExamData = new ExamsModel
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ExamCode = createExamData.ExamCode,
            ExamName = createExamData.ExamName,
            SubjectId = createExamData.SubjectId,
            ExamStatus = createExamData.ExamStatus,
            QuestionBankId = createExamData.QuestionBankId,
            QuestionSet = new List<QuestionSetsModel>(),

            // ExamLogs = new List<ExamLogsModel> { logCreateData },
        };

        try
        {
            await _examsRepository.InsertExamAsync(newExamData);
            return "Exam created successfully.";
        }
        catch (Exception ex)
        {
            // Console.WriteLine($"Error inserting exam: {ex.Message}");
            return $"Failed to create exam: {ex.Message}";
        }
    }

    // Update Exam(Not include question)
    public async Task<bool> UpdateExam(ExamDto updateExamData, string examId, string userLogId)
    {
        var result = await _examsRepository.UpdateExamAsync(updateExamData, examId, userLogId);
        return result.ModifiedCount > 0;
    }

    // Add question one/list
    public async Task<string> AddExamQuestion(ExamQuestionDTO questionData, string examId, string userLogId)
    {
        if (questionData == null)
        {
            return "Invalid data";
        }

        var exam = await _examsRepository.GetByIdAsync(examId);
        if (exam == null)
        {
            return "Exam not found";
        }

        var existingQuestions = exam.QuestionSet.Select(q => q.QuestionId).ToHashSet();
        var newQuestions = questionData.QuestionSets.Where(q => !existingQuestions.Contains(q.QuestionId)).ToList();

        if (!newQuestions.Any())
        {
            return "Question already exists";
        }

        var result = await _examsRepository.AddQuestionAsync(examId, newQuestions);

        return result.ModifiedCount > 0 ? "Question added successfully" : "Failure create question";
    }

    // Update Question
    public async Task<string> UpdateExamQuestion(string examId, string questionId, string userLogId, double questionScore)
    {
        // if (questionData == null || examId == null || questionData.QuestionSets == null || questionData.QuestionSets.Count == 0)
        // {
        //    return "Invalid data";
        // }

        // var questionSet = questionData.QuestionSets.First();
        // var questionScore = questionSet.QuestionScore;
        //var filter = Builders<ExamsModel>.Filter.And(
        //    Builders<ExamsModel>.Filter.Eq(e => e.Id, examId),
        //    Builders<ExamsModel>.Filter.ElemMatch(e => e.QuestionSet, qs => qs.QuestionId == questionId));

        //var update = Builders<ExamsModel>.Update.Set("QuestionSet.$.QuestionScore", questionScore);

        //var result = await this._examsCollection.UpdateOneAsync(filter, update);
        var result = await _examsRepository.UpdateExamQuestionAsync(examId, questionId, questionScore);
        // var updateQuestionLog = new ExamLogsModel
        // {
        //    ExamLogUserId = userLogId,
        //    ExamLogType = "Update Question",
        //    ExamChangeAt = DateTime.Now,
        // };
        if (result.ModifiedCount > 0)
        {
            // var addLog = Builders<ExamsModel>.Update.Push(e => e.ExamLogs, updateQuestionLog);
            // var logResult = await this._examsCollection.UpdateOneAsync(filter, addLog);
            return "Question updated successfully";
        }

        return "Question not found";
    }

    // Delete Question in Exam
    public async Task<string> DeleteExamQuestion(string examId, string questionId, string userLogId)
    {
        // if (questionData == null || examId == null || questionData.QuestionSets == null || questionData.QuestionSets.Count == 0)
        // {
        //    return "Invalid data";
        // }
        // var question = questionData.QuestionSets.First();
        //var filter = Builders<ExamsModel>.Filter.And(
        //    Builders<ExamsModel>.Filter.Eq(e => e.Id, examId),
        //    Builders<ExamsModel>.Filter.ElemMatch(e => e.QuestionSet, qs => qs.QuestionId == questionId));

        //var delete = Builders<ExamsModel>.Update.PullFilter(e => e.QuestionSet, qs => qs.QuestionId == questionId);

        //var result = await this._examsCollection.UpdateOneAsync(filter, delete);
        var result = await _examsRepository.DeleteExamQuestionAsync(examId, questionId);
        // var deleteQuestionLog = new ExamLogsModel
        // {
        //    ExamLogUserId = userLogId,
        //    ExamLogType = "Delete Question",
        //    ExamChangeAt = DateTime.Now,
        // };
        if (result.ModifiedCount > 0)
        {
            // var filterExam = Builders<ExamsModel>.Filter.Eq(e => e.Id, examId);

            // var updateLog = Builders<ExamsModel>.Update.Push(e => e.ExamLogs, deleteQuestionLog);
            // var logResult = await this._examsCollection.UpdateOneAsync(filterExam, updateLog);
            return "Question deleted successfully";
        }

        return "Question not found or already deleted";
    }
}
