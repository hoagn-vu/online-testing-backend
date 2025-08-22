using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;

namespace Backend_online_testing.Services
{
    public interface IGenerateExamService
    {
        Task<(string, GenerateExamResponseDto?)> GenerateExamAsync(GenerateExamRequestDto request);
    }

    public class GenerateExamService : IGenerateExamService
    {
        private readonly IGenerateExamRepository _generateExamRepository;

        public GenerateExamService(IGenerateExamRepository generateExamRepository)
        {
            _generateExamRepository = generateExamRepository;
        }

        public async Task<(string, GenerateExamResponseDto?)> GenerateExamAsync(GenerateExamRequestDto request)
        {
            var organizeExam = await _generateExamRepository.GetOrganizeExamByIdAsync(request.OrganizeExamId);
            if (organizeExam == null) return ("error-organize-exam", null);

            // Trường hợp 1: auto
            if (organizeExam.ExamType == "auto")
            {
                var subject = await _generateExamRepository.GetSubjectByIdAsync(organizeExam.SubjectId);
                if (subject == null) return ("error-subject", null);

                var questionBank = subject.QuestionBanks
                    .FirstOrDefault(qb => qb.QuestionBankId == organizeExam.QuestionBankId);
                if (questionBank == null) return ("error-question-bank", null);

                var availableQuestions = questionBank.QuestionList
                    .Where(q => q.QuestionStatus == "available")
                    .ToList();

                var random = new Random();
                var selectedQuestions = availableQuestions
                    .OrderBy(x => random.Next())
                    .Take(organizeExam.TotalQuestions ?? 0)
                    .ToList();

                // Tạo danh sách answers rỗng cho user
                var answers = selectedQuestions.Select(q => new AnswersModel
                {
                    QuestionId = q.QuestionId,
                    Score = null,
                    AnswerChosen = new List<string>(),
                    IsCorrect = false
                }).ToList();

                // Update user TakeExam
                var user = await _generateExamRepository.GetUserByIdAsync(request.UserId);
                if (user == null) return ("error-user", null);

                var takeExam = user.TakeExam
                    ?.FirstOrDefault(t =>
                        t.OrganizeExamId == request.OrganizeExamId &&
                        t.SessionId == request.SessionId &&
                        t.RoomId == request.RoomId);

                if (takeExam == null) return ("error-take-exam", null);

                takeExam.StartAt = DateTime.UtcNow;
                takeExam.Status = "in_exam";
                takeExam.Answers = answers;

                await _generateExamRepository.UpdateUserTakeExamAsync(request.UserId, takeExam);

                // Chuẩn hóa question response
                var questionDtos = selectedQuestions.Select(q => new GenerateExamQuestionResponseDto
                {
                    QuestionId = q.QuestionId,
                    QuestionType = q.QuestionType,
                    QuestionText = q.QuestionText,
                    ImgLinks = q.ImgLinks,
                    IsRandomOrder = q.IsRandomOrder,
                    Options = q.Options.Select(o => new GenerateExamOptionResponseDto
                    {
                        OptionId = o.OptionId,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList();
                
                return ("success", new GenerateExamResponseDto
                {
                    ExamName = organizeExam.OrganizeExamName,
                    Duration = organizeExam.Duration,
                    TotalQuestions = questionDtos.Count,
                    MaxScore = organizeExam.MaxScore ?? 10,
                    Questions = questionDtos
                });
            }

            // Trường hợp 2: examType = "exams"
            if (organizeExam.ExamType == "exams")
            {
                if (organizeExam.Exams == null || !organizeExam.Exams.Any()) return ("error-exam-set", null);

                var random = new Random();
                var examId = organizeExam.Exams[random.Next(organizeExam.Exams.Count)];

                var exam = await _generateExamRepository.GetExamByIdAsync(examId);
                if (exam == null) return ("error-get-exam", null);

                var subject = await _generateExamRepository.GetSubjectByIdAsync(exam.SubjectId);
                if (subject == null) return ("error-subject", null);
                
                var questionBank = subject.QuestionBanks
                    .FirstOrDefault(qb => qb.QuestionBankId == exam.QuestionBankId);
                if (questionBank == null) return ("error-question-bank", null);

                var questionDict = questionBank.QuestionList.ToDictionary(q => q.QuestionId, q => q);

                // Tạo danh sách answer theo QuestionSet
                var answers = exam.QuestionSet.Select(qs => new AnswersModel
                {
                    QuestionId = qs.QuestionId,
                    Score = qs.QuestionScore,
                    AnswerChosen = new List<string>(),
                    IsCorrect = false
                }).ToList();

                // Update user TakeExam
                var user = await _generateExamRepository.GetUserByIdAsync(request.UserId);
                if (user == null) return ("error-user", null);

                var takeExam = user.TakeExam
                    ?.FirstOrDefault(t =>
                        t.OrganizeExamId == request.OrganizeExamId &&
                        t.SessionId == request.SessionId &&
                        t.RoomId == request.RoomId);
                if (takeExam == null) return ("error-take-exam", null);

                takeExam.ExamId = exam.Id;
                takeExam.StartAt = DateTime.UtcNow;
                takeExam.Status = "in_exam";
                takeExam.Answers = answers;

                await _generateExamRepository.UpdateUserTakeExamAsync(request.UserId, takeExam);
                
                // Chuẩn hóa question response từ QuestionBank
                var questionDtos = exam.QuestionSet
                    .Where(qs => questionDict.ContainsKey(qs.QuestionId))
                    .Select(qs =>
                    {
                        var q = questionDict[qs.QuestionId];
                        return new GenerateExamQuestionResponseDto
                        {
                            QuestionId = q.QuestionId,
                            QuestionType = q.QuestionType,
                            QuestionText = q.QuestionText,
                            ImgLinks = q.ImgLinks,
                            IsRandomOrder = q.IsRandomOrder,
                            Options = q.Options.Select(o => new GenerateExamOptionResponseDto
                            {
                                OptionId = o.OptionId,
                                OptionText = o.OptionText
                            }).ToList()
                        };
                    }).ToList();

                return ( "success", new GenerateExamResponseDto
                {
                    ExamName = exam.ExamName,
                    Duration = organizeExam.Duration,
                    TotalQuestions = questionDtos.Count,
                    MaxScore = organizeExam.MaxScore ?? 10,
                    Questions = questionDtos
                });
            }

            // Trường hợp 3: examType = "matrix"
            if (organizeExam.ExamType == "matrix")
            {
                // TODO: xử lý sau
            }

            return ("error-exam-type", null);
        }
    }
}
