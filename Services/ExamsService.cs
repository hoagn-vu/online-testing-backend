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

            // Get question bank name
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

        var newExamData = new ExamsModel
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ExamCode = createExamData.ExamCode,
            ExamName = createExamData.ExamName,
            SubjectId = createExamData.SubjectId,
            ExamStatus = createExamData.ExamStatus,
            QuestionBankId = createExamData.QuestionBankId,
            QuestionSet = new List<QuestionSetsModel>(),
        };

        try
        {
            await _examsRepository.InsertExamAsync(newExamData);
            return "Exam created successfully.";
        }
        catch (Exception ex)
        {
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
        var result = await _examsRepository.UpdateExamQuestionAsync(examId, questionId, questionScore);
        if (result.ModifiedCount > 0)
        {
            return "Question updated successfully";
        }

        return "Question not found";
    }

    // Delete Question in Exam
    public async Task<string> DeleteExamQuestion(string examId, string questionId, string userLogId)
    {
        var result = await _examsRepository.DeleteExamQuestionAsync(examId, questionId);

        if (result.ModifiedCount > 0)
        {
            return "Question deleted successfully";
        }

        return "Question not found or already deleted";
    }
}
