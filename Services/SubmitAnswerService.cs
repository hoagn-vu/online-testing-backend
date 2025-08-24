using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;

namespace Backend_online_testing.Services
{
    public interface ISubmitAnswerService
    {
        Task<(string status, string message)> HandleAnswerAsync(
            string userId, string takeExamId, string type, SubmitAnswerDto? request);
    }
    
    public class SubmitAnswerService : ISubmitAnswerService
    {
        private readonly ISubmitAnswerRepository _repository;

        public SubmitAnswerService(ISubmitAnswerRepository repository)
        {
            _repository = repository;
        }

        public async Task<(string status, string message)> HandleAnswerAsync(
            string userId, string takeExamId, string type, SubmitAnswerDto? request)
        {
            var user = await _repository.GetUserByIdAsync(userId);
            if (user == null) return ("error-user", "User not found");

            var takeExam = user.TakeExam?.FirstOrDefault(te => te.Id == takeExamId);
            if (takeExam == null) return ("error-te", "TakeExam not found");

            if (type == "save" && request != null)
            {
                if (takeExam.Status != "in_exam") return ("error-status", "Exam not in progress");
                
                var answer = takeExam.Answers.FirstOrDefault(a => a.QuestionId == request.QuestionId);
                if (answer == null)
                {
                    answer = new AnswersModel
                    {
                        QuestionId = request.QuestionId,
                        AnswerChosen = request.OptionIds
                    };
                    takeExam.Answers.Add(answer);
                }
                else
                {
                    if (answer.AnswerChosen.Count == 0) takeExam.Progress += 1;

                    answer.AnswerChosen = request.OptionIds;
                }

                takeExam.FinishedAt = DateTime.UtcNow;

                await _repository.UpdateUserAsync(user);
                return ("saved", "Answer saved");
            }

            if (type == "submit")
            {
                var questionBank = await _repository.GetQuestionBankByOrganizeExamIdAsync(takeExam.OrganizeExamId);
                if (questionBank == null) return ("error-qb", "QuestionBank not found");
                
                double totalScore = 0;

                foreach (var ans in takeExam.Answers)
                {
                    var question = questionBank.QuestionList.FirstOrDefault(q => q.QuestionId == ans.QuestionId);
                    if (question == null) continue;

                    var chosenOptions = question.Options.Where(o => ans.AnswerChosen.Contains(o.OptionId)).ToList();
                    if (chosenOptions.Count == 0) continue;
                    var isAllCorrect = chosenOptions.All(o => o.IsCorrect == true);
                    var hasWrong = chosenOptions.Any(o => o.IsCorrect == false);

                    ans.IsCorrect = isAllCorrect && !hasWrong;

                    if (ans.IsCorrect)
                    {
                        totalScore += ans.Score ?? 0;
                    }
                }

                takeExam.TotalScore = totalScore;
                takeExam.Status = "done";
                takeExam.FinishedAt = DateTime.UtcNow;

                await _repository.UpdateUserAsync(user);
                return ("submitted", "Exam submitted");
            }

            return ("error", "Invalid request");
        }
    }
}