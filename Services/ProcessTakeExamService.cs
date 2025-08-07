using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Services;

public class ProcessTakeExamService
{
    private readonly IMongoCollection<OrganizeExamModel> _organizeExamCollection;
    private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
    private readonly IMongoCollection<RoomsModel> _roomsCollection;
    private readonly IMongoCollection<UsersModel> _usersCollection;
    private readonly IMongoCollection<ExamsModel> _examsCollection;
    private readonly IMongoCollection<ExamMatricesModel> _examMatricesCollection;

    public ProcessTakeExamService(IMongoDatabase database)
    {
        _organizeExamCollection = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
        _roomsCollection = database.GetCollection<RoomsModel>("rooms");
        _usersCollection = database.GetCollection<UsersModel>("users");
        _examsCollection = database.GetCollection<ExamsModel>("exams");
        _examMatricesCollection = database.GetCollection<ExamMatricesModel>("examMatrices");
    }

    public async Task<string?> ToggleSessionStatus(string organizeExamId, string sessionId)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(x => x.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(x => x.Sessions, s => s.SessionId == sessionId)
        );
        
        var session = (await _organizeExamCollection.Find(filter).FirstOrDefaultAsync())?
            .Sessions.FirstOrDefault(s => s.SessionId == sessionId);

        if (session == null) return null;
        
        var newStatus = session.SessionStatus == "active" ? "closed" : "active";
        var update = Builders<OrganizeExamModel>.Update.Set("sessions.$.sessionStatus", newStatus);

        var result = await _organizeExamCollection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0 ? newStatus : null;
    }

    public async Task<string?> ToggleRoomStatus(string organizeExamId, string sessionId, string roomId)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(x => x.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(x => x.Sessions, s => s.SessionId == sessionId)
        );
        
        var session = (await _organizeExamCollection.Find(filter).FirstOrDefaultAsync())?
            .Sessions.FirstOrDefault(s => s.SessionId == sessionId);

        if (session == null) return null;
        
        var room = session.RoomsInSession.FirstOrDefault(r => r.RoomInSessionId == roomId);
        if (room == null) return null;


        var newStatus = room.RoomStatus == "active" ? "closed" : "active";
        var update = Builders<OrganizeExamModel>.Update.Set("sessions.$.rooms.$[room].roomStatus", newStatus);

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new JsonArrayFilterDefinition<BsonDocument>($"{{ 'room.roomId': '{roomId}' }}")
        };
        
        var candidateIds = room.CandidateIds;
        var userFilter = Builders<UsersModel>.Filter.In(u => u.Id, candidateIds);
        var userUpdate = Builders<UsersModel>.Update.Set("takeExams.$[].status", newStatus);

        await _usersCollection.UpdateManyAsync(userFilter, userUpdate);

        var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };
        var result = await _organizeExamCollection.UpdateOneAsync(filter, update, updateOptions);

        return result.ModifiedCount > 0 ? newStatus : null;
    }

    public async Task<List<ProcessTakeExamDto>> GetActiveExam(string userId)
    {
        List<ProcessTakeExamDto> allActiveExams = [];
        var user = await _usersCollection.Find(usr => usr.Id == userId).FirstOrDefaultAsync();
        if (user == null) return allActiveExams;
    
        List<string> accessStatus = ["active", "notin", "reinexam", "outexam"];
        if (user.TakeExam == null) return allActiveExams;
        var activeExams = user.TakeExam.Where(e => accessStatus.Contains(e.Status)).ToList();
            
        foreach (var ex in activeExams)
        {
            var orgExam = _organizeExamCollection.Find(oe => oe.Id == ex.OrganizeExamId).FirstOrDefault();
            var session = orgExam?.Sessions.FirstOrDefault(s => s.SessionId == ex.SessionId);
                
            allActiveExams.Add(new ProcessTakeExamDto
            {
                OrganizeExamId = ex.OrganizeExamId,
                OrganizeExamName = orgExam != null ? orgExam.OrganizeExamName : "",
                SessionId = ex.SessionId,
                SessionName = session != null ? session.SessionName : "",
                RoomId = ex.RoomId,
                ExamType = orgExam != null ? orgExam.ExamType : "",
                MatrixId = orgExam != null ? orgExam.MatrixId : "",
                ExamId = ex.ExamId,
                Status = ex.Status,
            });
        }

        return allActiveExams;
    }
    
    public async Task<TakeExamDto?> TakeExam(string organizeExamId, string sessionId, string roomId, string userId)
    {
        var user = await _usersCollection
            .Find(u => u.Id == userId)
            .FirstOrDefaultAsync();

        if (user == null || user.TakeExam == null)
            return null;

        var matchedTakeExam = user.TakeExam
            .FirstOrDefault(t =>
                t.OrganizeExamId == organizeExamId &&
                t.SessionId == sessionId &&
                t.RoomId == roomId);

        if (matchedTakeExam == null)
            return null;

        var organizeExam = await _organizeExamCollection
            .Find(x => x.Id == organizeExamId)
            .FirstOrDefaultAsync();

        if (organizeExam == null || organizeExam.ExamType != "auto")
            return null;

        var subject = await _subjectsCollection
            .Find(s => s.Id == organizeExam.SubjectId)
            .FirstOrDefaultAsync();

        if (subject == null || subject.QuestionBanks == null)
            return null;

        var matchedBank = subject.QuestionBanks
            .FirstOrDefault(qb => qb.QuestionBankId == organizeExam.QuestionBankId);

        if (matchedBank == null || matchedBank.QuestionList == null)
            return null;

        var totalQuestions = organizeExam.TotalQuestions ?? 0;

        List<string> selectedQuestionIds;

        // Nếu đã đủ số câu hỏi => không random thêm mới
        if (matchedTakeExam.Answers.Count == totalQuestions)
        {
            selectedQuestionIds = matchedTakeExam.Answers.Select(a => a.QuestionId).ToList();
        }
        else
        {
            // Random thêm câu hỏi nếu chưa đủ
            var availableQuestions = matchedBank.QuestionList
                .Where(q => q.QuestionStatus == "available")
                .ToList();

            var random = new Random();
            var selectedQuestions = availableQuestions
                .OrderBy(_ => random.Next())
                .Take(totalQuestions)
                .ToList();

            foreach (var question in selectedQuestions)
            {
                // Chỉ thêm những câu hỏi chưa có trong answers
                if (!matchedTakeExam.Answers.Any(a => a.QuestionId == question.QuestionId))
                {
                    matchedTakeExam.Answers.Add(new AnswersModel
                    {
                        QuestionId = question.QuestionId,
                        AnswerChosen = new List<string>()
                    });
                }
            }

            selectedQuestionIds = matchedTakeExam.Answers.Select(a => a.QuestionId).ToList();
        }

        // Map ra danh sách câu hỏi DTO từ danh sách QuestionId
        var questionDtos = new List<TakeExamQuestionDto>();

        foreach (var questionId in selectedQuestionIds)
        {
            var question = matchedBank.QuestionList.FirstOrDefault(q => q.QuestionId == questionId);
            if (question == null) continue;

            var answerModel = matchedTakeExam.Answers.FirstOrDefault(a => a.QuestionId == questionId);
            var chosenOptionIds = answerModel?.AnswerChosen ?? new List<string>();

            var optionDtos = question.Options.Select(opt => new TakeExamOptionDto
            {
                OptionId = opt.OptionId,
                OptionText = opt.OptionText,
                IsChosen = chosenOptionIds.Contains(opt.OptionId)
            }).ToList();

            questionDtos.Add(new TakeExamQuestionDto
            {
                QuestionId = question.QuestionId,
                IsRandomOrder = question.IsRandomOrder,
                QuestionText = question.QuestionText,
                Options = optionDtos
            });
        }

        // Cập nhật StartAt và Status nếu chưa có
        if (matchedTakeExam.StartAt == null) matchedTakeExam.StartAt = DateTime.UtcNow;
        
        int finalDuration = organizeExam.Duration;
        if (matchedTakeExam.Status == "reinexam" &&
            matchedTakeExam.StartAt.HasValue &&
            matchedTakeExam.FinishedAt.HasValue)
        {
            var timeUsed = (matchedTakeExam.FinishedAt.Value - matchedTakeExam.StartAt.Value).TotalMinutes;
            finalDuration = Math.Max(0, organizeExam.Duration - (int)timeUsed);
        }
        
        matchedTakeExam.Status = "inexam";

        // Lưu lại vào DB
        var update = Builders<UsersModel>.Update.Set(u => u.TakeExam, user.TakeExam);
        await _usersCollection.UpdateOneAsync(u => u.Id == userId, update);

        // Trả về DTO
        return new TakeExamDto
        {
            TakeExamId = matchedTakeExam.Id,
            OrganizeExamId = organizeExam.Id,
            OrganizeExamName = organizeExam.OrganizeExamName,
            Duration = finalDuration,
            SessionId = sessionId,
            RoomId = roomId,
            Questions = questionDtos,
            Status = matchedTakeExam.Status,
        };
    }

    // public async Task<bool> SubmitAnswers(string userId, string takeExamId, string type, List<SubmitAnswersRequest> answersRequest)
    // {
    //     var user = await _usersCollection
    //         .Find(u => u.Id == userId)
    //         .FirstOrDefaultAsync();
    //
    //     if (user == null || user.TakeExam == null) return false;
    //
    //     var takeExam = user.TakeExam.FirstOrDefault(te => te.Id == takeExamId);
    //     if (takeExam == null) return false;
    //
    //     var organizeExam = await _organizeExamCollection
    //         .Find(x => x.Id == takeExam.OrganizeExamId)
    //         .FirstOrDefaultAsync();
    //     if (organizeExam == null) return false;
    //
    //     var subject = await _subjectsCollection
    //         .Find(x => x.Id == organizeExam.SubjectId)
    //         .FirstOrDefaultAsync();
    //     if (subject == null) return false;
    //
    //     var questionBank = subject.QuestionBanks
    //         .FirstOrDefault(qb => qb.QuestionBankId == organizeExam.QuestionBankId);
    //     if (questionBank == null) return false;
    //
    //     foreach (var req in answersRequest)
    //     {
    //         var existingAnswer = takeExam.Answers.FirstOrDefault(a => a.QuestionId == req.QuestionId);
    //
    //         if (existingAnswer != null)
    //         {
    //             existingAnswer.AnswerChosen = req.OptionIds ?? [];
    //         }
    //         else
    //         {
    //             takeExam.Answers.Add(new AnswersModel
    //             {
    //                 QuestionId = req.QuestionId,
    //                 AnswerChosen = req.OptionIds ?? []
    //             });
    //         }
    //     }
    //
    //     // Cập nhật tiến độ
    //     takeExam.Progress = takeExam.Answers.Count(a => a.AnswerChosen != null && a.AnswerChosen.Any());
    //     
    //     if (type == "submit")
    //     {
    //         int correctCount = 0;
    //         double totalScoreByAnswer = 0;
    //
    //         foreach (var ans in takeExam.Answers)
    //         {
    //             var question = questionBank.QuestionList
    //                 .FirstOrDefault(q => q.QuestionId == ans.QuestionId);
    //             if (question == null) continue;
    //
    //             var correctOptions = question.Options
    //                 .Where(o => o.IsCorrect == true)
    //                 .Select(o => o.OptionId)
    //                 .OrderBy(x => x)
    //                 .ToList();
    //
    //             var chosenOptions = ans.AnswerChosen.OrderBy(x => x).ToList();
    //
    //             var isCorrect = correctOptions.SequenceEqual(chosenOptions);
    //             ans.IsCorrect = isCorrect;
    //             if (isCorrect)
    //             {
    //                 correctCount++;
    //                 if (ans.Score.HasValue && ans.Score.Value > 0)
    //                 {
    //                     totalScoreByAnswer += ans.Score.Value;
    //                 }
    //             }
    //         }
    //
    //         var totalQuestions = organizeExam.TotalQuestions ?? 1;
    //         var maxScore = organizeExam.MaxScore ?? 10;
    //
    //         takeExam.TotalScore = totalScoreByAnswer > 0
    //             ? totalScoreByAnswer
    //             : correctCount * (maxScore / (double)totalQuestions);
    //
    //         takeExam.Status = "done";
    //     }
    //     
    //     takeExam.FinishedAt = DateTime.UtcNow;
    //
    //     // Ghi lại vào MongoDB
    //     var update = Builders<UsersModel>.Update.Set(u => u.TakeExam, user.TakeExam);
    //     await _usersCollection.UpdateOneAsync(u => u.Id == userId, update);
    //
    //     return true;
    // }
    public async Task<(string, bool)> SubmitAnswers(string userId, string takeExamId, string type, List<SubmitAnswersRequestDto>? answersRequest)
    {
        var user = await _usersCollection
            .Find(u => u.Id == userId)
            .FirstOrDefaultAsync();

        if (user == null || user.TakeExam == null) return ("error-user", false);

        var takeExam = user.TakeExam.FirstOrDefault(te => te.OrganizeExamId == takeExamId);
        if (takeExam == null) return ("error-texam", false);

        var organizeExam = await _organizeExamCollection
            .Find(x => x.Id == takeExam.OrganizeExamId)
            .FirstOrDefaultAsync();
        if (organizeExam == null) return ("error-oexam", false);

        var subject = await _subjectsCollection
            .Find(x => x.Id == organizeExam.SubjectId)
            .FirstOrDefaultAsync();
        if (subject == null) return ("error-subject", false);

        var questionBank = subject.QuestionBanks
            .FirstOrDefault(qb => qb.QuestionBankId == organizeExam.QuestionBankId);
        if (questionBank == null) return ("error-qb", false);

        // Chỉ xử lý answersRequest nếu type != "submit"
        if (type != "submit" && answersRequest != null)
        {
            foreach (var req in answersRequest)
            {
                var existingAnswer = takeExam.Answers.FirstOrDefault(a => a.QuestionId == req.QuestionId);

                if (existingAnswer != null)
                {
                    existingAnswer.AnswerChosen = req.OptionIds ?? [];
                }
                else
                {
                    takeExam.Answers.Add(new AnswersModel
                    {
                        QuestionId = req.QuestionId,
                        AnswerChosen = req.OptionIds ?? []
                    });
                }
            }

            // Cập nhật tiến độ
            takeExam.Progress = takeExam.Answers.Count(a => a.AnswerChosen != null && a.AnswerChosen.Any());
        }

        if (type == "submit")
        {
            int correctCount = 0;
            double totalScoreByAnswer = 0;

            foreach (var ans in takeExam.Answers)
            {
                var question = questionBank.QuestionList
                    .FirstOrDefault(q => q.QuestionId == ans.QuestionId);
                if (question == null) continue;

                var correctOptions = question.Options
                    .Where(o => o.IsCorrect == true)
                    .Select(o => o.OptionId)
                    .OrderBy(x => x)
                    .ToList();

                var chosenOptions = ans.AnswerChosen.OrderBy(x => x).ToList();

                var isCorrect = correctOptions.SequenceEqual(chosenOptions);
                ans.IsCorrect = isCorrect;
                if (isCorrect)
                {
                    correctCount++;
                    if (ans.Score is > 0)
                    {
                        totalScoreByAnswer += ans.Score.Value;
                    }
                }
            }

            var totalQuestions = organizeExam.TotalQuestions ?? 1;
            var maxScore = organizeExam.MaxScore ?? 10;

            takeExam.TotalScore = totalScoreByAnswer > 0
                ? totalScoreByAnswer
                : correctCount * (maxScore / (double)totalQuestions);

            takeExam.Status = "done";
        }

        takeExam.FinishedAt = DateTime.UtcNow;

        var update = Builders<UsersModel>.Update.Set(u => u.TakeExam, user.TakeExam);
        await _usersCollection.UpdateOneAsync(u => u.Id == userId, update);

        return ("ok", true);
    }

    
    public async Task<List<TrackExamDto>> TrackActiveExam(string userId)
    {
        var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null || user.TrackExam == null)
            return [];

        var result = new List<TrackExamDto>();

        foreach (var track in user.TrackExam)
        {
            var organizeExam = await _organizeExamCollection
                .Find(o => o.Id == track.OrganizeExamId)
                .FirstOrDefaultAsync();

            if (organizeExam == null)
                continue;

            var session = organizeExam.Sessions
                .FirstOrDefault(s => s.SessionId == track.SessionId && s.SessionStatus == "active");

            if (session == null)
                continue;

            var room = session.RoomsInSession.FirstOrDefault(r => r.RoomInSessionId == track.RoomId);
            var roomDetail = room != null
                ? await _roomsCollection.Find(r => r.Id == room.RoomInSessionId).FirstOrDefaultAsync()
                : null;

            result.Add(new TrackExamDto
            {
                OrganizeExamId = organizeExam.Id,
                OrganizeExamName = organizeExam.OrganizeExamName,
                SessionId = session.SessionId,
                SessionName = session.SessionName,
                RoomId = room?.RoomInSessionId ?? string.Empty,
                RoomName = roomDetail?.RoomName ?? "Unknown Room",
                Status = room?.RoomStatus ?? string.Empty
            });
        }

        return result;
    }
    
    public async Task<TrackExamDetailDto> TrackExamDetail(string organizeExamId, string sessionId, string roomId)
    {
        var exam = await _organizeExamCollection.Find(x => x.Id == organizeExamId).FirstOrDefaultAsync();
        if (exam == null) return null;

        var session = exam.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session == null) return null;

        var roomInSession = session.RoomsInSession.FirstOrDefault(r => r.RoomInSessionId == roomId);
        if (roomInSession == null) return null;

        var roomInfo = await _roomsCollection.Find(r => r.Id == roomId).FirstOrDefaultAsync();
        var candidateIds = roomInSession.CandidateIds;

        var candidates = await _usersCollection.Find(u => candidateIds.Contains(u.Id)).ToListAsync();

        var candidateDtos = candidates.Select(user =>
        {
            var takeExam = user.TakeExam?.FirstOrDefault(t =>
                t.OrganizeExamId == organizeExamId &&
                t.SessionId == sessionId &&
                t.RoomId == roomId
            );

            return new TrackCandidateDto
            {
                UserId = user.Id,
                UserCode = user.UserCode,
                FullName = user.FullName,
                Status = takeExam?.Status ?? "N/A",
                StartedAt = takeExam?.StartAt,
                FinishedAt = takeExam?.FinishedAt,
                Progress = takeExam?.Progress ?? 0,
                TotalScore = takeExam?.TotalScore,
                ViolationCount = takeExam?.ViolationCount ?? 0,
                TakeExamId = takeExam?.Id ?? string.Empty,
            };
        }).ToList();

        return new TrackExamDetailDto
        {
            OrganizeExamId = exam.Id,
            OrganizeExamName = exam.OrganizeExamName,
            Duration = exam.Duration,
            TotalQuestion = exam.TotalQuestions ?? 0,
            SessionId = session.SessionId,
            SessionName = session.SessionName,
            RoomId = roomId,
            RoomName = roomInfo?.RoomName ?? "N/A",
            Candidates = candidateDtos,
            Status = session.SessionStatus
        };
    }
    
    public async Task<bool> IncreaseViolationCount(string userId, string takeExamId)
    {
        var filter = Builders<UsersModel>.Filter.And(
            Builders<UsersModel>.Filter.Eq(u => u.Id, userId),
            Builders<UsersModel>.Filter.ElemMatch(u => u.TakeExam, te => te.Id == takeExamId)
        );

        var update = Builders<UsersModel>.Update.Inc("takeExams.$.violationCount", 1);

        var result = await _usersCollection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }
    
    public async Task<bool> UpdateStatusAndReason(string userId, string takeExamId, string type, string? unrecognizedReason)
    {
        var filter = Builders<UsersModel>.Filter.And(
            Builders<UsersModel>.Filter.Eq(u => u.Id, userId),
            Builders<UsersModel>.Filter.ElemMatch(u => u.TakeExam, te => te.Id == takeExamId)
        );
        
        

        var updates = new List<UpdateDefinition<UsersModel>>
        {
            Builders<UsersModel>.Update.Set("takeExams.$.status", type)
        };
        
        if (type == "active")
        {
            updates.Add(Builders<UsersModel>.Update.Set("takeExams.$.answers", new List<AnswersModel>()));
        }

        if (!string.IsNullOrEmpty(unrecognizedReason))
        {
            updates.Add(Builders<UsersModel>.Update.Set("takeExams.$.unrecognizedReason", unrecognizedReason));
        }

        var update = Builders<UsersModel>.Update.Combine(updates);
        var result = await _usersCollection.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0;
    }

    public async Task<(string, string)> CheckCanContinue(string userId, string takeExamId)
    {
        var user = await _usersCollection
            .Find(u => u.Id == userId)
            .FirstOrDefaultAsync();

        if (user == null || user.TakeExam == null) return ("", "");

        var takeExam = user.TakeExam.FirstOrDefault(te => te.Id == takeExamId);

        if (takeExam == null) return ("", "");

        return (takeExam.Status, takeExam.UnrecognizedReason ?? "");
    }
    
    public async Task<ExamResultDto?> GetExamResult(string userId, string takeExamId)
    {
        var filter = Builders<UsersModel>.Filter.Eq(u => u.Id, userId);
        var user = await _usersCollection.Find(filter).FirstOrDefaultAsync();

        if (user == null || user.TakeExam == null)
            return null;

        var takeExam = user.TakeExam.FirstOrDefault(te => te.OrganizeExamId == takeExamId);

        if (takeExam == null)
            return null;

        var totalQuestions = takeExam.Answers.Count;
        var correctAnswers = takeExam.Answers.Count(a => a.IsCorrect);
        var totalScore = takeExam.TotalScore;
        var finishedAt = takeExam.FinishedAt;

        var organizeExam = _organizeExamCollection.Find(o => o.Id == takeExam.OrganizeExamId).FirstOrDefaultAsync();

        return new ExamResultDto
        {
            OrganizeExamName = organizeExam.Result.OrganizeExamName,
            CandidateName = user.FullName,
            CandidateCode = user.UserCode,
            TotalQuestions = totalQuestions,
            CorrectAnswers = correctAnswers,
            TotalScore = totalScore,
            FinishedAt = finishedAt
        };
    }
    
    public async Task<List<ExamHistoryDto>> GetUserExamHistory(string userId)
    {
        var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null || user.TakeExam == null) return new List<ExamHistoryDto>();

        var results = new List<ExamHistoryDto>();

        foreach (var takeExam in user.TakeExam)
        {
            if (takeExam.Status != "done") continue;
            var organizeExam = await _organizeExamCollection.Find(x => x.Id == takeExam.OrganizeExamId).FirstOrDefaultAsync();
            if (organizeExam == null) continue;

            var subject = await _subjectsCollection.Find(x => x.Id == organizeExam.SubjectId).FirstOrDefaultAsync();

            results.Add(new ExamHistoryDto
            {
                OrganizeExamName = organizeExam.OrganizeExamName,
                SubjectName = subject?.SubjectName ?? "N/A",
                TotalScore = takeExam.TotalScore,
                FinishedAt = takeExam.FinishedAt,
                UnrecognizedReason = takeExam.UnrecognizedReason
            });
        }

        return results;
    }



}