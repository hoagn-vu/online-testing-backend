namespace Backend_online_testing.Services;

using Backend_online_testing.DTO;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using Backend_online_testing.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficeOpenXml.Style.XmlAccess;

public class ExamsService
{
    private readonly IMongoCollection<ExamsModel> _examsCollection;
    private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
    private readonly ExamRepository _examsRepository;
    private readonly SubjectRepository _subjectRepository;

    public ExamsService(IMongoDatabase database, ExamRepository examRepository, SubjectRepository subjectRepository)
    {
        this._examsCollection = database.GetCollection<ExamsModel>("exams");
        this._subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
        _examsRepository = examRepository;
        _subjectRepository = subjectRepository;
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
            var subject = await _examsRepository.GetSubjectByIdAsync(exam);
            var subjectName = subject?.SubjectName ?? string.Empty;

            // Get question bank name
            var questionBankName = string.Empty;
            if (subject != null)
            {
                var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == exam.QuestionBankId);
                questionBankName = questionBank?.QuestionBankName ?? string.Empty;
            }
            
            var matrixName = string.Empty;
            if (!string.IsNullOrEmpty(exam.MatrixId))
            {
                var matrix = await _examsRepository.GetMatrixByIdAsync(exam.MatrixId);
                matrixName = matrix?.MatrixName;
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
                QuestionBankName = questionBankName,
                MatrixId = exam.MatrixId,
                MatrixName = matrixName
            });
        }

        return (examResponseList, totalCount);
    }

    // Get question by exam code
    public async Task<ExamsModel?> GetExamByCodeAsync(string examId)
    {
        return await _examsRepository.GetByIdAsync(examId);
    }

    public async Task<(string,ExamDetailResponseDto?)> GetExamQuestionsWithDetailsAsync(string examId)
    {
        //var exam = await _examsCollection.Find(e => e.Id == examId).FirstOrDefaultAsync();
        var exam = await _examsRepository.GetByIdAsync(examId);
        if (exam == null)
        {
            return ("error-exam", null);
        }

        // Tìm subject
        //var subject = await _subjectsCollection
        //    .Find(s => s.Id == exam.SubjectId)
        //    .FirstOrDefaultAsync();
        var subject = await _examsRepository.GetSubjectByIdAsync(exam);

        if (subject == null)
        {
            return ("error-subject", null);
        }

        var questionBank = subject.QuestionBanks
            .FirstOrDefault(qb => qb.QuestionBankId == exam.QuestionBankId);

        if (questionBank == null)
        {
            return ("error-questionBank", null);
        }

        var detailedQuestions = new List<ExamQuestionDetailDTO>();

        foreach (var examQ in exam.QuestionSet)
        {
            var realQuestion = questionBank.QuestionList
                .FirstOrDefault(q => q.QuestionId == examQ.QuestionId);

            if (realQuestion == null)
            {
                Console.WriteLine($"Không tìm thấy câu hỏi trong questionBank!");
            }
            else
            {
                string chapter = realQuestion.Tags?.Count > 0 ? realQuestion.Tags[0] : "";
                string level = realQuestion.Tags?.Count > 1 ? realQuestion.Tags[1] : "";

                detailedQuestions.Add(new ExamQuestionDetailDTO
                {
                    
                    QuestionId = realQuestion.QuestionId,
                    QuestionText = realQuestion.QuestionText,
                    Chapter = chapter,
                    Level = level,
                    QuestionScore = (double)examQ.QuestionScore,
                    Options = realQuestion.Options.Select(o => new OptionDetailDTO
                    {
                        OptionId = o.OptionId,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                });
            }
        }
        var examDetail = new ExamDetailResponseDto
        {
            Id = exam.Id,
            ExamCode = exam.ExamCode,
            ExamName = exam.ExamName,
            ExamStatus = exam.ExamStatus,
            SubjectName = subject.SubjectName,
            QuestionBankName = questionBank.QuestionBankName,
            ListQuestion = detailedQuestions,

        };

        return ("Ok",examDetail);
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
            //QuestionSet = new List<QuestionSetsModel>(),
            QuestionSet = createExamData.QuestionSets,
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
    public async Task<bool> UpdateExam(ExamDto updateExamData, string examId)
    {
        var result = await _examsRepository.UpdateExamAsync(updateExamData, examId);
        return result.MatchedCount > 0;
    }


    // Add question one/list
    public async Task<(string?, string?, string?)> AddExamQuestion([FromBody] ExamQuestionDTO questionData, string examId, string userLogId)
    {
        if (questionData == null)
        {
            return (null, null, "Invalid data");
        }

        var exam = await _examsRepository.GetByIdAsync(examId);
        if (exam == null)
        {
            return (null, null, "Exam not found");
        }

        var existingQuestions = exam.QuestionSet.Select(q => q.QuestionId).ToHashSet();
        var newQuestions = questionData.QuestionSets.Where(q => !existingQuestions.Contains(q.QuestionId)).ToList();

        if (!newQuestions.Any())
        {
            return (null, null, "Question already exists");
        }

        var result = await _examsRepository.AddQuestionAsync(examId, newQuestions);

        return result.ModifiedCount > 0 ? (exam.ExamCode, exam.ExamName, "Question added successfully") : (exam.ExamCode, exam.ExamName, "Failure create question");
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

    // Insert example data
    public async Task SeedData()
    {
        var exampleExams = new List<ExamsModel>
        {
            new ExamsModel
            {
                ExamCode = "EX123",
                ExamName = "Math xam 1",
                SubjectId = "MATH001",
                QuestionBankId = "67ce3d5ac07467bf499bfdfe",
                QuestionSet = new List<QuestionSetsModel>
                {
                    new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 5.0 },
                    new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 3.0 },
                },
                ExamStatus = "Active",
            },
            new ExamsModel
            {
                ExamCode = "EX124",
                ExamName = "History exam 2",
                SubjectId = "HIS002",
                QuestionBankId = "67ce3d5ac07467bf499bfdfe",
                QuestionSet = new List<QuestionSetsModel>
                {
                    new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 4.0 },
                    new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 2.5 },
                },
                ExamStatus = "Pending",
            },
            new ExamsModel
            {
                ExamCode = "EX125",
                ExamName = "Sample Exam 3",
                SubjectId = "GEO003",
                QuestionBankId = "67ce3d5ac07467bf499bfdfe",
                QuestionSet = new List<QuestionSetsModel>
                {
                    new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 6.0 },
                    new QuestionSetsModel { QuestionId = ObjectId.GenerateNewId().ToString(), QuestionScore = 4.5 },
                },
                ExamStatus = "Completed",
            },
        };

        await this._examsCollection.InsertManyAsync(exampleExams);
    }
    
    public async Task<List<ExamOptionsDTO>> GetExamOptionsAsync(string? subjectId, string? questionBankId)
    {
        return await _examsRepository.GetExamOptionsAsync(subjectId, questionBankId);
    }
}
