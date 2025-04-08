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
    
    public async Task<OrganizeExamDto?> GetOrganizeExamById(string organizeExamId)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(oe => oe.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.Ne(s => s.OrganizeExamStatus, "deleted")
        );

        var exam = await _organizeExamCollection.Find(filter).FirstOrDefaultAsync();
        if (exam == null) return null;

        // Lấy thông tin môn học
        var subject = await _subjectsCollection.Find(s => s.Id == exam.SubjectId).FirstOrDefaultAsync();
        var subjectName = subject?.SubjectName ?? string.Empty;

        // Lấy thông tin ma trận đề
        string? matrixName = null;
        if (!string.IsNullOrEmpty(exam.MatrixId))
        {
            var matrix = await _examMatricesCollection.Find(s => s.Id == exam.MatrixId).FirstOrDefaultAsync();
            matrixName = matrix?.MatrixName ?? string.Empty;
        }

        // Lấy danh sách bài thi
        var examSet = new List<ExamInOrganizeExamDto>();
        if (exam.Exams == null || exam.Exams.Count == 0)
            return new OrganizeExamDto
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
            };
        var examFilter = Builders<ExamsModel>.Filter.In(e => e.Id, exam.Exams);
        var examList = await _examsCollection.Find(examFilter).ToListAsync();

        examSet = examList.Select(ex => new ExamInOrganizeExamDto
        {
            Id = ex.Id,
            ExamCode = ex.ExamCode,
            ExamName = ex.ExamName,
        }).ToList();

        return new OrganizeExamDto
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
        };
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

        var organizeExam = await _organizeExamCollection.Find(filter).FirstOrDefaultAsync();

        if (organizeExam == null)
        {
            return (organizeExamId, null, new List<SessionsDto>(), 0);
        }

        var allSessions = organizeExam.Sessions
            .Where(ss => string.IsNullOrEmpty(keyword) || ss.SessionName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        var paginatedSessions = allSessions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ss => new SessionsDto
            {
                SessionId = ss.SessionId,
                SessionName = ss.SessionName,
                ActiveAt = ss.ActiveAt,
                TotalRooms = ss.RoomsInSession.Count,
                SessionStatus = ss.SessionStatus,
            })
            .ToList();

        return (organizeExamId, organizeExam.OrganizeExamName, paginatedSessions, allSessions.Count);
    }

    
    public async Task<(string, string, string, string?, List<RoomsInSessionDto>, long)> GetRoomsInSession(
        string organizeExamId, string sessionId, string? keyword, int page, int pageSize)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(oe => oe.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.Ne(s => s.OrganizeExamStatus, "deleted")
        );

        var organizeExam = await _organizeExamCollection.Find(filter).FirstOrDefaultAsync();

        if (organizeExam == null)
            return (organizeExamId, "", sessionId, null, new List<RoomsInSessionDto>(), 0);

        var session = organizeExam.Sessions.FirstOrDefault(ss => ss.SessionId == sessionId);
        if (session == null)
            return (organizeExamId, organizeExam.OrganizeExamName, sessionId, null, new List<RoomsInSessionDto>(), 0);

        var allRooms = session.RoomsInSession.ToList();

        // Lọc theo keyword nếu có
        if (!string.IsNullOrEmpty(keyword))
        {
            var roomIds = allRooms.Select(r => r.RoomInSessionId).ToList();

            var matchingRooms = await _roomsCollection
                .Find(r => roomIds.Contains(r.Id) && r.RoomName.ToLower().Contains(keyword.ToLower()))
                .ToListAsync();

            var matchingRoomIds = matchingRooms.Select(r => r.Id).ToHashSet();
            allRooms = allRooms.Where(r => matchingRoomIds.Contains(r.RoomInSessionId)).ToList();
        }

        var totalCount = allRooms.Count;

        // Áp dụng phân trang
        var paginatedRooms = allRooms
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var roomsResponse = new List<RoomsInSessionDto>();

        foreach (var rm in paginatedRooms)
        {
            var room = await _roomsCollection.Find(r => r.Id == rm.RoomInSessionId).FirstOrDefaultAsync();
            var roomName = room?.RoomName ?? string.Empty;
            var roomLocation = room?.RoomLocation ?? string.Empty;

            var supervisorsInRoom = rm.SupervisorIds
                .Select(supId => _usersCollection.Find(us => us.Id == supId).FirstOrDefault())
                .Where(user => user != null)
                .Select(user => new SupervisorsInRoomModel
                {
                    SupervisorId = user.Id,
                    SupervisorName = user.UserName,
                    UserCode = user.UserCode
                })
                .ToList();

            var totalCandidates = rm.CandidateIds.Count;

            roomsResponse.Add(new RoomsInSessionDto
            {
                RoomInSessionId = rm.RoomInSessionId,
                RoomName = roomName,
                RoomLocation = roomLocation,
                Supervisors = supervisorsInRoom,
                TotalCandidates = totalCandidates,
                RoomStatus = rm.RoomStatus,
            });
        }

        return (organizeExamId, organizeExam.OrganizeExamName, sessionId, session.SessionName, roomsResponse, totalCount);
    }


    public async Task<(string, string, string, string?, string, string, List<CandidatesInSessionRoomDto>, long)>
        GetCandidatesInSessionRoom(string organizeExamId, string sessionId, string roomId, string? keyword, int page, int pageSize)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(oe => oe.Id, organizeExamId),
            Builders<OrganizeExamModel>.Filter.Ne(s => s.OrganizeExamStatus, "deleted")
        );

        var organizeExam = await _organizeExamCollection.Find(filter).FirstOrDefaultAsync();
        if (organizeExam == null)
            return (organizeExamId, "", sessionId, null, roomId, "", new List<CandidatesInSessionRoomDto>(), 0);

        var session = organizeExam.Sessions.FirstOrDefault(ss => ss.SessionId == sessionId);
        if (session == null)
            return (organizeExamId, organizeExam.OrganizeExamName, sessionId, null, roomId, "", new List<CandidatesInSessionRoomDto>(), 0);

        var room = session.RoomsInSession.FirstOrDefault(r => r.RoomInSessionId == roomId);
        if (room == null)
            return (organizeExamId, organizeExam.OrganizeExamName, sessionId, session.SessionName, roomId, "", new List<CandidatesInSessionRoomDto>(), 0);

        var roomData = await _roomsCollection.Find(r => r.Id == room.RoomInSessionId).FirstOrDefaultAsync();
        var roomName = roomData?.RoomName ?? string.Empty;

        var allCandidates = room.CandidateIds;

        // Lọc theo keyword nếu có
        if (!string.IsNullOrEmpty(keyword))
        {
            var candidates = allCandidates;
            var matchingCandidates = await _usersCollection
                .Find(u => candidates.Contains(u.Id) &&
                           (u.FullName.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) ||
                            u.UserCode.Contains(keyword, StringComparison.CurrentCultureIgnoreCase)))
                .ToListAsync();

            var matchingCandidateIds = matchingCandidates.Select(c => c.Id).ToHashSet();
            allCandidates = allCandidates.Where(c => matchingCandidateIds.Contains(c)).ToList();
        }

        var totalCount = allCandidates.Count;

        var paginatedCandidates = allCandidates
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var candidatesResponse = new List<CandidatesInSessionRoomDto>();

        foreach (var cand in paginatedCandidates)
        {
            var candidate = await _usersCollection.Find(u => u.Id == cand).FirstOrDefaultAsync();
            var examResult = candidate.TakeExam?.Find(er => er.OrganizeExamId == organizeExamId && er.SessionId == sessionId && er.RoomId == roomId);

            candidatesResponse.Add(new CandidatesInSessionRoomDto
            {
                CandidateId = cand,
                CandidateName = candidate.FullName,
                UserCode = candidate.UserCode,
                Dob = candidate.DateOfBirth,
                Gender = candidate.Gender,
                ProgressStatus = examResult?.Status,
                RecognizedResult = examResult?.Status,
                Score = examResult?.TotalScore
            });
        }

        return (organizeExamId, organizeExam.OrganizeExamName, sessionId, session.SessionName, roomId, roomName, candidatesResponse, totalCount);
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

    public async Task<string> UpdateOrganizeExam(string organizeExamId, OrganizeExamRequestDto dto)
    // public async Task<OrganizeExamModel?> UpdateOrganizeExam(string organizeExamId, OrganizeExamRequestDto dto)
    {
        try
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

            // return await _organizeExamCollection.FindOneAndUpdateAsync(
            //     e => e.Id == organizeExamId, update, new FindOneAndUpdateOptions<OrganizeExamModel> { ReturnDocument = ReturnDocument.After });
            await _organizeExamCollection.FindOneAndUpdateAsync(e => e.Id == organizeExamId, update);
            return "Cập nhật kỳ thi thành công";
        }
        catch (Exception e)
        {
            return $"Error: {e.Message}";
        }
    }
        
    public async Task<OrganizeExamModel?> AddSession(string examId, SessionRequestDto dto)
    {
        var organizeExamDuration = _organizeExamCollection.Find(oe => oe.Id == examId).FirstOrDefault().Duration;
        var newSession = new SessionsModel
        {
            SessionName = dto.SessionName,
            ActiveAt = dto.ActiveAt,
            ForceEndAt = dto.ActiveAt.AddMinutes(2 * organizeExamDuration),
            SessionStatus = dto.SessionStatus
        };

        var update = Builders<OrganizeExamModel>.Update.Push(e => e.Sessions, newSession);
        return await _organizeExamCollection.FindOneAndUpdateAsync(
            e => e.Id == examId, update, new FindOneAndUpdateOptions<OrganizeExamModel> { ReturnDocument = ReturnDocument.After });
    }

    public async Task<string> UpdateSession(string examId, string sessionId, SessionRequestDto dto)
    {
        var filter = Builders<OrganizeExamModel>.Filter.Eq(e => e.Id, examId) &
                     Builders<OrganizeExamModel>.Filter.ElemMatch(e => e.Sessions, s => s.SessionId == sessionId);
        
        var update = Builders<OrganizeExamModel>.Update
            .Set("sessions.$.sessionName", dto.SessionName)
            .Set("sessions.$.activeAt", dto.ActiveAt)
            .Set("sessions.$.sessionStatus", dto.SessionStatus);
        
        var result = await _organizeExamCollection.UpdateOneAsync(filter, update);
        
        return result.ModifiedCount > 0 ? "Cập nhật ca thi thành công" : "Không tìm thấy ca thi";
    }
    
    public async Task<OrganizeExamModel?> AddRoomToSession(string examId, string sessionId, RoomsInSessionRequestDto dto)
    {
        var filter = Builders<OrganizeExamModel>.Filter.Eq(o => o.Id, examId)
                     & Builders<OrganizeExamModel>.Filter.ElemMatch(o => o.Sessions, s => s.SessionId == sessionId);
        
        var newRoom = new SessionRoomsModel
        {
            RoomInSessionId = dto.RoomId,
            SupervisorIds = dto.SupervisorIds
        };
        
        var update = Builders<OrganizeExamModel>.Update.Push("sessions.$.rooms", newRoom);
        
        foreach (var filterTrackExam in dto.SupervisorIds.Select(supv => Builders<UsersModel>.Filter.Eq(u => u.Id, supv)))
        {
            var user = await _usersCollection.Find(filterTrackExam).FirstOrDefaultAsync();
            
            if (user == null)
            {
                throw new Exception("User not found");
            }
        
            user.TrackExam ??= [];
        
            var exists = user.TrackExam.Exists(te =>
                te.OrganizeExamId == examId &&
                te.SessionId == sessionId &&
                te.RoomId == dto.RoomId);

            if (exists) continue;
            var newTrackExam = new TrackExamsModel
            {
                OrganizeExamId = examId,
                SessionId = sessionId,
                RoomId = dto.RoomId,
            };
            
            user.TrackExam.Add(newTrackExam);
            var updateTrackExam = Builders<UsersModel>.Update.Set(u => u.TrackExam, user.TrackExam);
            await _usersCollection.UpdateOneAsync(filterTrackExam, updateTrackExam);
        }
        
        return await _organizeExamCollection.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<OrganizeExamModel>
            {
                ReturnDocument = ReturnDocument.After
            }
        );
    }
    
    public async Task<OrganizeExamModel?> AddCandidateToRoom(string examId, string sessionId, string roomId, CandidatesInSessionRoomRequestDto dto)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(o => o.Id, examId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(o => o.Sessions, s => s.SessionId == sessionId)
        );

        var candidates = dto.CandidateIds;

        var update = Builders<OrganizeExamModel>.Update.PushEach("sessions.$.rooms.$[room].candidateIds", candidates);

        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new JsonArrayFilterDefinition<BsonDocument>($"{{'room.roomId': '{roomId}'}}")
        };

        foreach (var filterTakeExam in candidates.Select(candidate => Builders<UsersModel>.Filter.Eq(u => u.Id, candidate)))
        {
            var user = await _usersCollection.Find(filterTakeExam).FirstOrDefaultAsync();
            
            if (user == null)
            {
                throw new Exception("User not found");
            }
        
            user.TakeExam ??= new List<TakeExamsModel>();
        
            var exists = user.TakeExam.Exists(te =>
                te.OrganizeExamId == examId &&
                te.SessionId == sessionId &&
                te.RoomId == roomId);

            if (exists) continue;
            var newTakeExam = new TakeExamsModel
            {
                OrganizeExamId = examId,
                SessionId = sessionId,
                RoomId = roomId,
                Status = "closed",
                Progress = 0,
                ViolationCount = 0,
                Answers = []
            };
            
            user.TakeExam.Add(newTakeExam);
            var updateTakeExam = Builders<UsersModel>.Update.Set(u => u.TakeExam, user.TakeExam);
            await _usersCollection.UpdateOneAsync(filterTakeExam, updateTakeExam);
        }

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
            session => session.RoomsInSession.Any(room => room.CandidateIds.Any(c => c == candidateId))
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
