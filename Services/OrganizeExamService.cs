using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Services;

public class OrganizeExamService
{
    private readonly IMongoCollection<OrganizeExamModel> _organizeExamCollection;
    private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
    private readonly IMongoCollection<RoomsModel> _roomsCollection;
    private readonly IMongoCollection<UsersModel> _usersCollection;
    private readonly IMongoCollection<ExamsModel> _examsCollection;
    private readonly IMongoCollection<ExamMatricesModel> _examMatricesCollection;

    public OrganizeExamService(IMongoDatabase database)
    {
        _organizeExamCollection = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
        _roomsCollection = database.GetCollection<RoomsModel>("rooms");
        _usersCollection = database.GetCollection<UsersModel>("users");
        _examsCollection = database.GetCollection<ExamsModel>("exams");
        _examMatricesCollection = database.GetCollection<ExamMatricesModel>("examMatrices");
    }
    
    public async Task<(List<OrganizeExamDto>, long)> GetOrganizeExams(string? keyword, int page, int pageSize)
    {
        var filter = Builders<OrganizeExamModel>.Filter.Ne(ex => ex.OrganizeExamStatus, "deleted");
        if (!string.IsNullOrEmpty(keyword))
        {
            filter = Builders<OrganizeExamModel>.Filter.Regex(ex => ex.OrganizeExamName, new BsonRegularExpression(keyword, "i"));
        }
        
        var organizeExams = await _organizeExamCollection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
            
        var totalCount = await _organizeExamCollection.CountDocumentsAsync(filter);
        
        var organizeExamResponseList = new List<OrganizeExamDto>();

        foreach (var exam in organizeExams)
        {
            var examSet = new List<ExamInOrganizeExamDto>();
            // Lấy thông tin môn học
            var subject = await _subjectsCollection.Find(s => s.Id == exam.SubjectId).FirstOrDefaultAsync();
            var subjectName = subject?.SubjectName ?? string.Empty;

            string? matrixName = null;
            if (!string.IsNullOrEmpty(exam.MatrixId))
            {
                var matrix = await _examMatricesCollection.Find(s => s.Id == exam.MatrixId).FirstOrDefaultAsync();
                matrixName = matrix?.MatrixName ?? string.Empty;
            }

            if (exam.Exams != null)
                foreach (var exId in exam.Exams)
                {
                    var ex = await _examsCollection.Find(s => s.Id == exId).FirstOrDefaultAsync();
                    examSet.Add(new ExamInOrganizeExamDto
                    {
                        Id = ex.Id,
                        ExamCode = ex.ExamCode,
                        ExamName = ex.ExamName,
                    });
                }

            organizeExamResponseList.Add(new OrganizeExamDto
            {
                Id = exam.Id,
                OrganizeExamName = exam.OrganizeExamName,
                SubjectId = exam.SubjectId,
                SubjectName = subjectName,
                Duration = exam.Duration,
                TotalQuestions = exam.TotalQuestions,
                MaxScore = exam.MaxScore,
                ExamType = exam.ExamType,
                Exams = examSet,
                MatrixId = exam.MatrixId,
                MatrixName = matrixName,
                OrganizeExamStatus = exam.OrganizeExamStatus,
                TotalSessions = exam.Sessions.Count,
            });
        }
        
        return (organizeExamResponseList, totalCount);
    }
    
    public async Task<(string, string?, List<SessionsDto>, long)> GetSessions(string organizeExamId, string? keyword, int page, int pageSize)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(oe => oe.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.Ne(s => s.OrganizeExamStatus, "deleted")
        );
        
        if (!string.IsNullOrEmpty(keyword))
        {
            filter = Builders<OrganizeExamModel>.Filter.And(
                filter,
                Builders<OrganizeExamModel>.Filter.ElemMatch(
                    oe => oe.Sessions,
                    Builders<SessionsModel>.Filter.Regex(q => q.SessionName, new BsonRegularExpression(keyword, "i"))));
        }

        var organizeExam = await _organizeExamCollection.Find(filter).ToListAsync();
        
        var sessions = organizeExam
            .SelectMany(oe => oe.Sessions
                .Where(ss => string.IsNullOrEmpty(keyword) || ss.SessionName.ToLower().Contains(keyword.ToLower()))
                .Select(ss => new SessionsDto
                {
                    SessionId = ss.SessionId,
                    SessionName = ss.SessionName,
                    ActiveAt = ss.ActiveAt,
                    TotalRooms = ss.RoomsInSession.Count,
                    SessionStatus = ss.SessionStatus,
                }))
            .ToList();

        var totalSessions = sessions.Count;
        var organizeExamName = organizeExam.FirstOrDefault()?.OrganizeExamName;

        return (organizeExamId, organizeExamName, sessions, totalSessions);
    }
    
    public async Task<(string, string, string, string?, List<RoomsInSessionDto>, long)> GetRoomsInSession(string organizeExamId, string sessionId, string? keyWord, int page, int pageSize)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(oe => oe.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.Ne(s => s.OrganizeExamStatus, "deleted")
        );
            
        var organizeExam = await _organizeExamCollection
            .Find(filter)
            .FirstOrDefaultAsync();
    
        var session = organizeExam.Sessions.FirstOrDefault(ss => ss.SessionId == sessionId);
    
        var rooms = session?.RoomsInSession;
        
        var totalCount = rooms?.Count ?? 0;
        
        var roomsResponse = new List<RoomsInSessionDto>();
    
        foreach (var rm in rooms)
        {
            var room = await _roomsCollection.Find(r => r.Id == rm.RoomInSessionId).FirstOrDefaultAsync();
            var roomName = room.RoomName ?? string.Empty;

            var supervisorsInRoom = rm.SupervisorIds.Select(supId => _usersCollection.Find(us => us.Id == supId).FirstOrDefault()).Select(user => new SupervisorsInRoomModel { SupervisorId = user.Id, SupervisorName = user.UserName, UserCode = user.UserCode }).ToList();

            var totalCandidates = rm.Candidates?.Count ?? 0;
    
            roomsResponse.Add(new RoomsInSessionDto
            {
                RoomInSessionId = rm.RoomInSessionId,
                RoomName = roomName,
                Supervisors = supervisorsInRoom,
                TotalCandidates = totalCandidates
            });
        }
        
        return (organizeExamId, organizeExam.OrganizeExamName ,sessionId, session?.SessionName ,roomsResponse, totalCount);
    }

    public async Task<(string, string, string, string?, string, string, List<CandidatesInSessionRoomDto>, long)>
        GetCandidatesInSessionRoom(string organizeExamId, string sessionId, string roomId, string? keyWord, int page,
            int pageSize)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(oe => oe.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.Ne(s => s.OrganizeExamStatus, "deleted")
        );
            
        var organizeExam = await _organizeExamCollection
            .Find(filter)
            .FirstOrDefaultAsync();
    
        var session = organizeExam.Sessions.FirstOrDefault(ss => ss.SessionId == sessionId);
    
        var room = session?.RoomsInSession.FirstOrDefault(r => r.RoomInSessionId == roomId);
        var roomName = _roomsCollection.Find(r => room != null && r.Id == room.RoomInSessionId).FirstOrDefault()?.RoomName ?? string.Empty;

        var candidatesInRoom = room?.Candidates;
        
        var totalCount = candidatesInRoom?.Count ?? 0;
        
        var candidatesResponse = (from cand in candidatesInRoom
        let candidate = _usersCollection.Find(us => us.Id == cand.CandidateId).FirstOrDefault()
        select new CandidatesInSessionRoomDto
        {
            CandidateId = cand.CandidateId,
            CandidateName = candidate.FullName,
            UserCode = candidate.UserCode,
            ProgressStatus = cand.ProgressStatus,
            RecognizedResult = cand.RecognizedResult,
            Score = cand.TotalScore
        }).ToList();
        
        return (organizeExamId, organizeExam.OrganizeExamName ,sessionId, session?.SessionName, roomId, roomName, candidatesResponse, totalCount);
    }
    
    public async Task<OrganizeExamModel> CreateOrganizeExam(OrganizeExamRequestDto dto)
    {
        var newExam = new OrganizeExamModel
        {
            OrganizeExamName = dto.OrganizeExamName,
            Duration = dto.Duration,
            TotalQuestions = dto.TotalQuestions,
            MaxScore = dto.MaxScore,
            SubjectId = dto.SubjectId,
            QuestionBankId = dto.QuestionBankId,
            ExamType = dto.ExamType,
            MatrixId = dto.MatrixId,
            Exams = dto.Exams,
            OrganizeExamStatus = dto.OrganizeExamStatus
        };
        await _organizeExamCollection.InsertOneAsync(newExam);
        return newExam;
    }

    public async Task<OrganizeExamModel?> UpdateOrganizeExam(string id, OrganizeExamRequestDto dto)
    {
        var update = Builders<OrganizeExamModel>.Update
            .Set(e => e.OrganizeExamName, dto.OrganizeExamName)
            .Set(e => e.Duration, dto.Duration)
            .Set(e => e.TotalQuestions, dto.TotalQuestions)
            .Set(e => e.MaxScore, dto.MaxScore)
            .Set(e => e.SubjectId, dto.SubjectId)
            .Set(e => e.QuestionBankId, dto.QuestionBankId)
            .Set(e => e.ExamType, dto.ExamType)
            .Set(e => e.MatrixId, dto.MatrixId)
            .Set(e => e.Exams, dto.Exams)
            .Set(e => e.OrganizeExamStatus, dto.OrganizeExamStatus);

        return await _organizeExamCollection.FindOneAndUpdateAsync(
            e => e.Id == id, update, new FindOneAndUpdateOptions<OrganizeExamModel> { ReturnDocument = ReturnDocument.After });
    }
        
    public async Task<OrganizeExamModel?> AddSession(string examId, SessionRequestDto dto)
    {
        var newSession = new SessionsModel
        {
            SessionName = dto.SessionName,
            ActiveAt = dto.ActiveAt,
            SessionStatus = dto.SessionStatus
        };

        var update = Builders<OrganizeExamModel>.Update.Push(e => e.Sessions, newSession);
        return await _organizeExamCollection.FindOneAndUpdateAsync(
            e => e.Id == examId, update, new FindOneAndUpdateOptions<OrganizeExamModel> { ReturnDocument = ReturnDocument.After });
    }
    
    public async Task<OrganizeExamModel?> AddRoomToSession(string examId, string sessionId, RoomsInSessionRequestDto dto)
    {
        var filter = Builders<OrganizeExamModel>.Filter.Eq(o => o.Id, examId)
                     & Builders<OrganizeExamModel>.Filter.ElemMatch(o => o.Sessions, s => s.SessionId == sessionId);
        
        var newRoom = new SessionRoomsModel
        {
            RoomInSessionId = ObjectId.GenerateNewId().ToString(),
            SupervisorIds = dto.SupervisorIds
        };

        var update = Builders<OrganizeExamModel>.Update.Push("sessions.$.rooms", newRoom);
        
        return await _organizeExamCollection.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<OrganizeExamModel>
            {
                ReturnDocument = ReturnDocument.After
            }
        );
    }
    
    // public async Task<OrganizeExamModel?> AddCandidateToRoom(string examId, string sessionId, string roomId, CandidatesInSessionRoomRequestDto dto)
    // {
    //     var filter = Builders<OrganizeExamModel>.Filter.And(
    //         Builders<OrganizeExamModel>.Filter.Eq(o => o.Id, examId),
    //         Builders<OrganizeExamModel>.Filter.ElemMatch(o => o.Sessions, s => s.SessionId == sessionId && s.RoomsInSession.Any(r => r.RoomInSessionId == roomId))
    //     );
    //
    //     var candidates = dto.CandidateIds.Select(cid => new CandidatesInRoomModel { CandidateId = cid }).ToList();
    //
    //     var update = Builders<OrganizeExamModel>.Update.PushEach("sessions.$.roomsInSession.$[].candidates", candidates);
    //
    //     return await _organizeExamCollection.FindOneAndUpdateAsync(
    //         filter,
    //         update,
    //         new FindOneAndUpdateOptions<OrganizeExamModel>
    //         {
    //             ReturnDocument = ReturnDocument.After
    //         }
    //     );
    // }
    
    public async Task<OrganizeExamModel?> AddCandidateToRoom(string examId, string sessionId, string roomId, CandidatesInSessionRoomRequestDto dto)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(o => o.Id, examId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(o => o.Sessions, s => s.SessionId == sessionId)
        );

        var candidates = dto.CandidateIds.Select(cid => new CandidatesInRoomModel { CandidateId = cid }).ToList();

        var update = Builders<OrganizeExamModel>.Update.PushEach("sessions.$.rooms.$[room].candidates", candidates);

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new JsonArrayFilterDefinition<BsonDocument>($"{{'room.roomId': '{roomId}'}}")
        };

        return await _organizeExamCollection.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<OrganizeExamModel>
            {
                ReturnDocument = ReturnDocument.After,
                ArrayFilters = arrayFilters
            }
        );
    }

    public async Task<List<OrganizeExamResponseDto>> GetExamsByCandidateId(string candidateId)
    {
        var filter = Builders<OrganizeExamModel>.Filter.ElemMatch(
            o => o.Sessions,
            session => session.RoomsInSession.Any(room => room.Candidates.Any(c => c.CandidateId == candidateId))
        );

        var exams = await _organizeExamCollection.Find(filter).ToListAsync();
        return exams.Select(exam => new OrganizeExamResponseDto
        {
            Id = exam.Id,
            OrganizeExamName = exam.OrganizeExamName,
            Duration = exam.Duration,
            TotalQuestions = exam.TotalQuestions,
            MaxScore = exam.MaxScore,
            SubjectId = exam.SubjectId,
            QuestionBankId = exam.QuestionBankId,
            ExamType = exam.ExamType,
            SessionId = exam.Sessions.FirstOrDefault()?.SessionId,
            RoomId = exam.Sessions.FirstOrDefault()?.RoomsInSession.FirstOrDefault()?.RoomInSessionId
        }).ToList();
    }
    
    public async Task<(List<QuestionResponseDto>, int, string)> GetQuestionsByExamId(string organizeExamId)
    {
        var exam = await _organizeExamCollection.Find(x => x.Id == organizeExamId).FirstOrDefaultAsync();
        if (exam == null || string.IsNullOrEmpty(exam.SubjectId) || string.IsNullOrEmpty(exam.QuestionBankId))
        {
            return (new List<QuestionResponseDto>(), 0, string.Empty);
        }

        var subject = await _subjectsCollection.Find(s => s.Id == exam.SubjectId).FirstOrDefaultAsync();
        if (subject == null)
        {
            return (new List<QuestionResponseDto>(), 0, string.Empty);
        }

        var questionBank = subject.QuestionBanks.FirstOrDefault(qb => qb.QuestionBankId == exam.QuestionBankId);
        if (questionBank == null)
        {
            return (new List<QuestionResponseDto>(), 0, string.Empty);
        }

        var questions = questionBank.QuestionList
            .Where(q => q.QuestionStatus == "available")
            .Take(exam.TotalQuestions ?? 0)
            .Select(q => new QuestionResponseDto
            {
                QuestionId = q.QuestionId,
                QuestionName = q.QuestionText,
                Options = q.Options.Select(o => new OptionsResponseDto
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText
                }).ToList()
            }).ToList();

        return (questions, exam.Duration, exam.Sessions.FirstOrDefault()?.SessionId ?? string.Empty);
    }

    
    
}
