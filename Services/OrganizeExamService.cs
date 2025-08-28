using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;

namespace Backend_online_testing.Services;

public class OrganizeExamService
{
    private readonly IMongoCollection<OrganizeExamModel> _organizeExamCollection;
    private readonly IMongoCollection<SubjectsModel> _subjectsCollection;
    private readonly IMongoCollection<RoomsModel> _roomsCollection;
    private readonly IMongoCollection<UsersModel> _usersCollection;
    private readonly IMongoCollection<ExamsModel> _examsCollection;
    private readonly IMongoCollection<ExamMatricesModel> _examMatricesCollection;
    private readonly OrganizeExamRepository _organizeExamRepository;

    public OrganizeExamService(IMongoDatabase database, OrganizeExamRepository organizeExamRepository)
    {
        _organizeExamCollection = database.GetCollection<OrganizeExamModel>("organizeExams");
        _subjectsCollection = database.GetCollection<SubjectsModel>("subjects");
        _roomsCollection = database.GetCollection<RoomsModel>("rooms");
        _usersCollection = database.GetCollection<UsersModel>("users");
        _examsCollection = database.GetCollection<ExamsModel>("exams");
        _examMatricesCollection = database.GetCollection<ExamMatricesModel>("examMatrices");
        _organizeExamRepository = organizeExamRepository;
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
            .SortByDescending(ex => ex.Id)
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
    
    public async Task<List<OrganizeExamOptionsDto>> GetOrganizeExamOptions(string? subjectId, string? status)
    {
        var filter = Builders<OrganizeExamModel>.Filter.Ne(ex => ex.OrganizeExamStatus, "deleted");

        if (!string.IsNullOrEmpty(subjectId))
        {
            filter = Builders<OrganizeExamModel>.Filter.And(
                filter,
                Builders<OrganizeExamModel>.Filter.Eq(ex => ex.SubjectId, subjectId));
        }

        if (!string.IsNullOrEmpty(status))
        {
            filter = Builders<OrganizeExamModel>.Filter.And(
                filter,
                Builders<OrganizeExamModel>.Filter.Eq(ex => ex.OrganizeExamStatus, status));
        }
        
        var organizeExams = await _organizeExamCollection
            .Find(filter)
            .SortByDescending(ex => ex.Id)
            .Project(e => new OrganizeExamOptionsDto
            {
                Id = e.Id,
                OrganizeExamName = e.OrganizeExamName,
                SubjectId = e.SubjectId
            })
            .ToListAsync();
        
        return organizeExams;
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
            .OrderBy(ss => ss.StartAt)
            .ThenBy(ss => ss.FinishAt)
            .ToList();

        var paginatedSessions = allSessions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ss => new SessionsDto
            {
                SessionId = ss.SessionId,
                SessionName = ss.SessionName,
                StartAt = ss.StartAt,
                FinishAt = ss.FinishAt,
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

        var allRooms = session.RoomsInSession
            .AsEnumerable()
            .Reverse()
            .ToList();

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
                    SupervisorName = user.FullName,
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

        // ✅ Lấy tất cả thông tin candidates
        var candidateInfos = await _usersCollection
            .Find(u => allCandidates.Contains(u.Id))
            .ToListAsync();

        // ✅ Sort theo FirstName trước, rồi LastName
        candidateInfos = candidateInfos
            .OrderBy(u => u.FirstName, StringComparer.Create(new CultureInfo("vi-VN"), ignoreCase: true))
            .ThenBy(u => u.LastName, StringComparer.Create(new CultureInfo("vi-VN"), ignoreCase: true))
            .ToList();

        var paginatedCandidates = candidateInfos
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var candidatesResponse = new List<CandidatesInSessionRoomDto>();

        foreach (var candidate in paginatedCandidates)
        {
            //var candidate = await _usersCollection.Find(u => u.Id == cand).FirstOrDefaultAsync();
            var examResult = candidate.TakeExam?.Find(er => er.OrganizeExamId == organizeExamId && er.SessionId == sessionId && er.RoomId == roomId);

            candidatesResponse.Add(new CandidatesInSessionRoomDto
            {
                CandidateId = candidate.Id,
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
    
    public async Task<(string status, OrganizeExamModel? exam)> CreateOrganizeExamWithSessions(OrganizeExamRequestDto dto)
    {
        var totalQuestions = dto.TotalQuestions ?? 0;

        var newExam = new OrganizeExamModel
        {
            OrganizeExamName = dto.OrganizeExamName,
            Duration = dto.Duration * 60,
            TotalQuestions = totalQuestions,
            MaxScore = dto.MaxScore,
            SubjectId = dto.SubjectId,
            QuestionBankId = dto.QuestionBankId,
            ExamType = dto.ExamType,
            MatrixId = dto.MatrixId,
            Exams = dto.Exams,
            OrganizeExamStatus = dto.OrganizeExamStatus,
            Sessions = []
        };

        if (dto.Sessions is not null && dto.Sessions.Any())
        {
            foreach (var sessionDto in dto.Sessions)
            {
                // // Kiểm tra trùng lặp StartAt & FinishAt
                // var existed = newExam.Sessions.Any(s =>
                //     s.StartAt == sessionDto.StartAt &&
                //     s.FinishAt == sessionDto.StartAt.AddMinutes(dto.Duration));
                //
                // if (existed)
                // {
                //     return ("duplicate-session-time", null);
                // }
                
                var newStart = sessionDto.StartAt;
                var newFinish = sessionDto.FinishAt ?? sessionDto.StartAt.AddMinutes(dto.Duration);

                // Kiểm tra chồng chéo
                var overlapped = newExam.Sessions.Any(s =>
                    s.StartAt < newFinish && newStart < s.FinishAt);

                if (overlapped)
                {
                    return ("session-overlap", null);
                }

                var session = new SessionsModel
                {
                    SessionName = sessionDto.SessionName,
                    StartAt = sessionDto.StartAt,
                    FinishAt = sessionDto.StartAt.AddMinutes(dto.Duration),
                    ForceEndAt = sessionDto.StartAt.AddMinutes(3 * dto.Duration),
                    SessionStatus = sessionDto.SessionStatus
                };

                newExam.Sessions.Add(session);
            }
        }

        await _organizeExamCollection.InsertOneAsync(newExam);
        return ("success", newExam);
    }
    
    public async Task<string> UpdateOrganizeExam(string organizeExamId, OrganizeExamRequestDto dto)
    {
        try
        {
            var updates = new List<UpdateDefinition<OrganizeExamModel>>();

            if (!string.IsNullOrEmpty(dto.OrganizeExamName))
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.OrganizeExamName, dto.OrganizeExamName));

            if (dto.Duration > 0)
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.Duration, dto.Duration * 60));

            if (dto.TotalQuestions.HasValue)
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.TotalQuestions, dto.TotalQuestions.Value));

            if (dto.MaxScore.HasValue)
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.MaxScore, dto.MaxScore.Value));

            if (!string.IsNullOrEmpty(dto.SubjectId))
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.SubjectId, dto.SubjectId));

            if (!string.IsNullOrEmpty(dto.QuestionBankId))
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.QuestionBankId, dto.QuestionBankId));

            if (!string.IsNullOrEmpty(dto.ExamType))
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.ExamType, dto.ExamType));

            if (!string.IsNullOrEmpty(dto.MatrixId))
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.MatrixId, dto.MatrixId));

            if (dto.Exams != null && dto.Exams.Any())
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.Exams, dto.Exams));

            if (!string.IsNullOrEmpty(dto.OrganizeExamStatus))
                updates.Add(Builders<OrganizeExamModel>.Update.Set(e => e.OrganizeExamStatus, dto.OrganizeExamStatus));

            if (!updates.Any())
                return "Không có dữ liệu nào để cập nhật";

            var updateDefinition = Builders<OrganizeExamModel>.Update.Combine(updates);

            await _organizeExamCollection.UpdateOneAsync(
                e => e.Id == organizeExamId,
                updateDefinition
            );
            return "Cập nhật kỳ thi thành công";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

        
    public async Task<(string status, OrganizeExamModel? exam)> AddSession(string examId, SessionRequestDto dto)
    {
        var exam = await _organizeExamCollection.Find(e => e.Id == examId).FirstOrDefaultAsync();
        if (exam == null)
            return ("exam-not-found", null);

        var organizeExamDuration = exam.Duration;

        var newStart = dto.StartAt;
        var newFinish = dto.FinishAt ?? dto.StartAt.AddSeconds(organizeExamDuration);

        // Kiểm tra chồng chéo với session đã có
        var overlapped = exam.Sessions.Any(s =>
            s.StartAt < newFinish && newStart < s.FinishAt);

        if (overlapped)
        {
            return ("session-overlap", null);
        }

        var newSession = new SessionsModel
        {
            SessionName = dto.SessionName,
            StartAt = newStart,
            FinishAt = newFinish,
            ForceEndAt = newStart.AddSeconds(3 * organizeExamDuration),
            SessionStatus = dto.SessionStatus
        };

        var update = Builders<OrganizeExamModel>.Update.Push(e => e.Sessions, newSession);
        var updatedExam = await _organizeExamCollection.FindOneAndUpdateAsync(
            e => e.Id == examId,
            update,
            new FindOneAndUpdateOptions<OrganizeExamModel> { ReturnDocument = ReturnDocument.After });

        return ("success", updatedExam);
    }

    // update 1 session
    public async Task<string> UpdateSession(string examId, string sessionId, SessionRequestDto dto)
    {
        // Lấy duration nhanh gọn, chỉ project field cần
        var organizeExamDuration = await _organizeExamCollection
            .Find(x => x.Id == examId)
            .Project(x => x.Duration)
            .FirstOrDefaultAsync();

        if (organizeExamDuration == default)
            return "Không tìm thấy kỳ thi";

        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(e => e.Id, examId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(e => e.Sessions, s => s.SessionId == sessionId)
        );

        var finishAt = dto.StartAt.AddSeconds(organizeExamDuration);
        var forceEndAt = dto.StartAt.AddSeconds(3 * organizeExamDuration);

        var update = Builders<OrganizeExamModel>.Update
            .Set("sessions.$.sessionName", dto.SessionName)
            .Set("sessions.$.startAt", dto.StartAt)
            .Set("sessions.$.finishAt", finishAt)
            .Set("sessions.$.forceEndAt", forceEndAt)
            .Set("sessions.$.sessionStatus", dto.SessionStatus);

        var result = await _organizeExamCollection.UpdateOneAsync(filter, update);

        return result.ModifiedCount > 0
            ? "Cập nhật ca thi thành công"
            : "Không tìm thấy ca thi";
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
    
    public async Task<string> AddCandidateToRoom(string examId, string sessionId, string roomId, CandidatesInSessionRoomRequestDto dto)
    {
        var filter = Builders<OrganizeExamModel>.Filter.And(
            Builders<OrganizeExamModel>.Filter.Eq(o => o.Id, examId),
            Builders<OrganizeExamModel>.Filter.ElemMatch(o => o.Sessions, s => s.SessionId == sessionId)
        );

        List<string> candidates = dto.CandidateIds;

        if (candidates == null || !candidates.Any())
        {
            if (dto.UserCodes != null && dto.UserCodes.Any())
            {
                var filterUserCodes = Builders<UsersModel>.Filter.In(u => u.UserCode, dto.UserCodes);
                var users = await _usersCollection.Find(filterUserCodes).ToListAsync();

                if (users.Count != dto.UserCodes.Count)
                {
                    throw new Exception("Some user codes are invalid");
                }

                candidates = users.Select(u => u.Id).ToList();
            }
            else
            {
                throw new Exception("CandidateIds and UserCodes are both empty");
            }
        }

        // Lấy danh sách candidateIds hiện tại
        var projection = Builders<OrganizeExamModel>.Projection
            .ElemMatch(o => o.Sessions, s => s.SessionId == sessionId);
        var exam = await _organizeExamCollection
            .Find(o => o.Id == examId)
            .Project<OrganizeExamModel>(projection)
            .FirstOrDefaultAsync();

        var session = exam?.Sessions.FirstOrDefault();
        var room = session?.RoomsInSession.FirstOrDefault(r => r.RoomInSessionId == roomId);

        if (room == null)
            throw new Exception("Room not found");

        var existingCandidateIds = room.CandidateIds ?? new List<string>();

        // Lọc thí sinh mới (chưa có trong danh sách)
        var newCandidates = candidates
            .Where(c => !existingCandidateIds.Contains(c))
            .ToList();

        if (!newCandidates.Any())
        {
            return "Tất cả thí sinh đã tồn tại trong phòng này";
        }

        // Cập nhật danh sách TakeExam cho từng thí sinh
        foreach (var candidateId in newCandidates)
        {
            var filterTakeExam = Builders<UsersModel>.Filter.Eq(u => u.Id, candidateId);
            var user = await _usersCollection.Find(filterTakeExam).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new Exception($"User not found: {candidateId}");
            }

            user.TakeExam ??= new List<TakeExamsModel>();

            var exists = user.TakeExam.Exists(te =>
                te.OrganizeExamId == examId &&
                te.SessionId == sessionId &&
                te.RoomId == roomId);

            if (!exists)
            {
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
        }

        // Push danh sách thí sinh mới vào CandidateIds
        var update = Builders<OrganizeExamModel>.Update.PushEach("sessions.$.rooms.$[room].candidateIds", newCandidates);
        var arrayFilters = new List<ArrayFilterDefinition>
        {
            new JsonArrayFilterDefinition<BsonDocument>($"{{'room.roomId': '{roomId}'}}")
        };

        await _organizeExamCollection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions
            {
                ArrayFilters = arrayFilters
            }
        );

        return $"Đã thêm {newCandidates.Count} thí sinh vào phòng";
    }



    public async Task<List<OrganizeExamResponseDto>> GetExamsByCandidateId(string candidateId)
    {
        var user = await _organizeExamRepository.GetUserByIdAsync(candidateId);
        if (user == null || user.TakeExam == null) return new List<OrganizeExamResponseDto>();

        var notStartedExams = user.TakeExam
            .Where(t => t is { RoomStatus: "active", Status: "not_started" or "re_open" })
            .ToList();
        var response = new List<OrganizeExamResponseDto>();

        foreach (var takeExam in notStartedExams)
        {
            var organizeExam = await _organizeExamRepository.GetOrganizeExamByIdAsync(takeExam.OrganizeExamId);
            if (organizeExam == null) continue;

            var session = organizeExam.Sessions.FirstOrDefault(s => s.SessionId == takeExam.SessionId);
            var roomName = string.Empty;

            if (!string.IsNullOrEmpty(takeExam.RoomId))
            {
                var room = await _organizeExamRepository.GetRoomByIdAsync(takeExam.RoomId);
                roomName = room?.RoomName ?? string.Empty;
            }

            response.Add(new OrganizeExamResponseDto
            {
                Id = organizeExam.Id,
                OrganizeExamName = organizeExam.OrganizeExamName,
                Duration = organizeExam.Duration,
                TotalQuestions = organizeExam.TotalQuestions,
                MaxScore = organizeExam.MaxScore,
                SubjectId = organizeExam.SubjectId,
                QuestionBankId = organizeExam.QuestionBankId,
                ExamType = organizeExam.ExamType,
                SessionId = session?.SessionId,
                SessionName = session?.SessionName ?? string.Empty,
                RoomId = takeExam.RoomId,
                RoomName = roomName,
                UserTakeExamId = takeExam.Id
            });
        }

        return response;
        // var filter = Builders<OrganizeExamModel>.Filter.ElemMatch(
        //     o => o.Sessions,
        //     session => session.RoomsInSession.Any(room => room.CandidateIds.Any(c => c == candidateId))
        // );
        //
        // var exams = await _organizeExamCollection.Find(filter).ToListAsync();
        // return exams.Select(exam => new OrganizeExamResponseDto
        // {
        //     Id = exam.Id,
        //     OrganizeExamName = exam.OrganizeExamName,
        //     Duration = exam.Duration,
        //     TotalQuestions = exam.TotalQuestions,
        //     MaxScore = exam.MaxScore,
        //     SubjectId = exam.SubjectId,
        //     QuestionBankId = exam.QuestionBankId,
        //     ExamType = exam.ExamType,
        //     SessionId = exam.Sessions.FirstOrDefault()?.SessionId,
        //     RoomId = exam.Sessions.FirstOrDefault()?.RoomsInSession.FirstOrDefault()?.RoomInSessionId,
        //     UserTakeExamId = _usersCollection.
        // }).ToList();
    }
    
    public async Task<(List<QuestionResponseDto>, int, string)> GetQuestionsByExamId(string organizeExamId)
    {
        var exam = await _organizeExamCollection.Find(x => x.Id == organizeExamId).FirstOrDefaultAsync();
        if (exam == null || string.IsNullOrEmpty(exam.SubjectId))
        {
            return (new List<QuestionResponseDto>(), 0, string.Empty);
        }

        var subject = await _subjectsCollection.Find(s => s.Id == exam.SubjectId).FirstOrDefaultAsync();
        if (subject == null)
        {
            return (new List<QuestionResponseDto>(), 0, string.Empty);
        }

        List<QuestionModel> availableQuestions = [];
        List<QuestionModel> selectedQuestions = [];

        if (exam.ExamType == "matrix" && !string.IsNullOrEmpty(exam.MatrixId))
        {
            var matrix = await _examMatricesCollection
                .Find(x => x.Id == exam.MatrixId && x.MatrixStatus != "deleted")
                .FirstOrDefaultAsync();

            if (matrix == null)
            {
                return (new List<QuestionResponseDto>(), 0, string.Empty);
            }

            var questionBank = subject.QuestionBanks
                .FirstOrDefault(qb => qb.QuestionBankId == matrix.QuestionBankId);

            if (questionBank == null)
            {
                return (new List<QuestionResponseDto>(), 0, string.Empty);
            }

            availableQuestions = questionBank.QuestionList
                .Where(q => q.QuestionStatus == "available" && q.Tags?.Count >= 2)
                .ToList();

            foreach (var tag in matrix.MatrixTags)
            {
                var matchedQuestions = availableQuestions
                    .Where(q => q.Tags[0] == tag.Chapter && q.Tags[1] == tag.Level)
                    .OrderBy(_ => Guid.NewGuid()) // random
                    .Take(tag.QuestionCount)
                    .ToList();

                selectedQuestions.AddRange(matchedQuestions);
            }
        }
        else if (exam is { ExamType: "exams", Exams.Count: > 0 })
        {
            var randomExamId = exam.Exams.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();

            var examSet = await _examsCollection
                .Find(e => e.Id == randomExamId && e.ExamStatus == "available")
                .FirstOrDefaultAsync();

            if (examSet == null || string.IsNullOrEmpty(examSet.QuestionBankId))
            {
                return (new List<QuestionResponseDto>(), 0, string.Empty);
            }

            var questionBank = subject.QuestionBanks
                .FirstOrDefault(qb => qb.QuestionBankId == examSet.QuestionBankId);

            if (questionBank == null)
            {
                return (new List<QuestionResponseDto>(), 0, string.Empty);
            }

            var questionDict = questionBank.QuestionList
                .Where(q => q.QuestionStatus == "available")
                .ToDictionary(q => q.QuestionId, q => q);

            foreach (var qs in examSet.QuestionSet)
            {
                if (questionDict.TryGetValue(qs.QuestionId, out var question))
                {
                    selectedQuestions.Add(question);
                }
            }
        }
        else // examType == "auto" hoặc khác
        {
            if (string.IsNullOrEmpty(exam.QuestionBankId))
            {
                return (new List<QuestionResponseDto>(), 0, string.Empty);
            }

            var questionBank = subject.QuestionBanks
                .FirstOrDefault(qb => qb.QuestionBankId == exam.QuestionBankId);

            if (questionBank == null)
            {
                return (new List<QuestionResponseDto>(), 0, string.Empty);
            }

            selectedQuestions = questionBank.QuestionList
                .Where(q => q.QuestionStatus == "available")
                .Take(exam.TotalQuestions ?? 0)
                .ToList();
        }

        var questionDtos = selectedQuestions
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

        return (questionDtos, exam.Duration, exam.Sessions.FirstOrDefault()?.SessionId ?? string.Empty);
    }

    //Add room to session
    // public async Task AddRoomsToSession_StrictAsync(string organizeExamId, string sessionId, AddRoomToSessionRequest request)
    // {
    //     if (string.IsNullOrWhiteSpace(organizeExamId)) throw new ArgumentException("examId is required");
    //     if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentException("sessionId is required");
    //     if (request is null) throw new ArgumentNullException(nameof(request));
    //     if (request.RoomIds is null || request.RoomIds.Count == 0)
    //         throw new ArgumentException("RoomIds must not be empty");
    //
    //     if (request.RoomIds.Any(x => x.Quantity < 0))
    //         throw new ArgumentException("Quantity must be >= 0");
    //
    //     var roomIds = request.RoomIds.Select(x => x.RoomId).Distinct().ToList();
    //
    //     //Validate room
    //     if (!await _organizeExamRepository.RoomsExistInMasterAsync(roomIds))
    //         throw new InvalidOperationException("Some RoomId do not exist in collection 'rooms'.");
    //
    //     // Validate supervisors
    //     var allSupIds = request.RoomIds.SelectMany(x => x.SupervisorIds).Distinct().ToList();
    //     if (allSupIds.Count > 0 && !await _organizeExamRepository.AreAllSupervisorsAsync(allSupIds))
    //         throw new InvalidOperationException("Some SupervisorId are not role=supervisor.");
    //
    //     // Candidate pool groupUserIds
    //     var rawUserIds = await _organizeExamRepository.GetUserIdsByGroupIdsAsync(request.GroupUserIds ?? new());
    //     if (rawUserIds.Count == 0)
    //         throw new InvalidOperationException("No candidates found in provided groupUserIds.");
    //
    //     // Sort candidate Id (FirstName -> LastName)
    //     var orderedCandidateIds = await _organizeExamRepository.GetOrderedCandidatesAsync(rawUserIds);
    //
    //     // Nếu có bất kỳ room nào đã tồn tại trong session -> fail
    //     var anyExists = await _organizeExamRepository.AnyRoomExistsInSessionAsync(organizeExamId, sessionId, roomIds);
    //     if (anyExists)
    //         throw new InvalidOperationException("One or more rooms already exist in this session. Operation aborted.");
    //
    //     // Location by quantity
    //     var q = new Queue<string>(orderedCandidateIds);
    //     var roomsToAdd = new List<(string RoomId, List<string> SupervisorIds, List<string> CandidateIds, string? RoomStatus)>();
    //
    //     foreach (var r in request.RoomIds)
    //     {
    //         var cands = new List<string>();
    //         var take = r.Quantity;
    //         while (take > 0 && q.Count > 0)
    //         {
    //             cands.Add(q.Dequeue());
    //             take--;
    //         }
    //
    //         roomsToAdd.Add((
    //             RoomId: r.RoomId,
    //             SupervisorIds: (r.SupervisorIds ?? new()).Distinct().ToList(),
    //             CandidateIds: cands,
    //             RoomStatus: "closed"
    //         ));
    //     }
    //
    //     // Atomic push: chỉ thành công nếu tất cả room đều chưa tồn tại
    //     var ok = await _organizeExamRepository.AddRoomsIfAllAbsentAsync(organizeExamId, sessionId, roomsToAdd);
    //     if (!ok)
    //         throw new InvalidOperationException("Failed to add rooms (session not found or some rooms existed concurrently).");
    //
    //     //Add user.TakeExam and user.TrackExam
    //     foreach (var r in roomsToAdd)
    //     {
    //         // users[*].takeExams (candidate)
    //         await _organizeExamRepository.AddTakeExamsForCandidatesAsync(
    //             organizeExamId, sessionId, r.RoomId, r.CandidateIds);
    //
    //         // users[*].trackExams (supervisor)
    //         await _organizeExamRepository.AddTrackExamsForSupervisorsAsync(
    //             organizeExamId, sessionId, r.RoomId, r.SupervisorIds, "closed");
    //         
    //         // === Thêm RoomSchedule cho RoomsModel ===
    //         await _organizeExamRepository.AddRoomScheduleAsync(
    //             organizeExamId, sessionId, r.RoomId, r.CandidateIds.Count);
    //     }
    // }
    //
    public async Task<(string status, string message)> AddRoomsToSession_StrictAsync(
        string organizeExamId, string sessionId, AddRoomToSessionRequest request)
    {
        if (string.IsNullOrWhiteSpace(organizeExamId)) 
            return ("missing-exam-id", "Vui lòng bổ sung thông tin kỳ thi");
        if (string.IsNullOrWhiteSpace(sessionId)) 
            return ("missing-session-id", "Vui lòng bổ sung thông tin ca thi");
        if (request is null) 
            return ("missing-request-body", "Vui lòng bổ sung thông tin tìm kiếm");
        if (request.RoomIds is null || request.RoomIds.Count == 0)
            return ("empty-room-ids", "Danh sách phòng không được để trống");

        if (request.RoomIds.Any(x => x.Quantity <= 0))
            return ("invalid-room-quantity", "Vui lòng nhập số lượng thí sinh trong phòng thi hợp lệ");

        var roomIds = request.RoomIds.Select(x => x.RoomId).Distinct().ToList();

        if (!await _organizeExamRepository.RoomsExistInMasterAsync(roomIds))
            return ("room-not-found", "Một số phòng thi không tồn tại");

        var allSupIds = request.RoomIds.SelectMany(x => x.SupervisorIds ?? new()).Distinct().ToList();
        if (allSupIds.Count > 0 && !await _organizeExamRepository.AreAllSupervisorsAsync(allSupIds))
            return ("invalid-supervisor", "Vui lòng chọn giám thị hợp lệ");

        var rawUserIds = await _organizeExamRepository.GetUserIdsByGroupIdsAsync(request.GroupUserIds ?? new());
        if (rawUserIds.Count == 0)
            return ("candidates-not-found", "Không tìm thấy thí sinh trong các nhóm");

        var orderedCandidateIds = await _organizeExamRepository.GetOrderedCandidatesAsync(rawUserIds);

        var anyExists = await _organizeExamRepository.AnyRoomExistsInSessionAsync(organizeExamId, sessionId, roomIds);
        if (anyExists)
            return ("room-already-in-session", "Vui lòng không thêm phòng đã sử dụng trong ca thi.");

        var roomInfos = await _organizeExamRepository.GetRoomsByIdsAsync(roomIds);
        // var totalCapacity = roomInfos.Sum(r => r.RoomCapacity ?? 0);
        // var totalRequested = request.RoomIds.Sum(r => r.Quantity);
        var totalRequested = request.RoomIds.Sum(r => r.Quantity);
        var totalCapacity = roomInfos.Sum(r => r.RoomCapacity ?? 0);
        var totalCandidates = orderedCandidateIds.Count;

        if (totalRequested > totalCapacity)
            return ("capacity-exceeded", $"Số chỗ ngồi các phòng thi ({totalCapacity}) không đáp ứng tổng số lượng thí sinh ({totalRequested})");
        
        if (totalRequested < totalCandidates)
            return ("insufficient-quantity", $"Tổng số lượng thí sinh mỗi phòng ({totalRequested}) không đáp ứng tổng số lượng thí sinh ({totalCandidates})");

        foreach (var r in request.RoomIds)
        {
            var roomInfo = roomInfos.FirstOrDefault(x => x.Id == r.RoomId);
            if (roomInfo != null && r.Quantity > (roomInfo.RoomCapacity ?? 0))
                return ("room-capacity-exceeded", $"Phòng thi {roomInfo.RoomName} vượt quá số lượng chỗ ngồi");
        }

        var existingCandidateIds = await _organizeExamRepository.GetCandidateIdsInSessionAsync(organizeExamId, sessionId);
        if (orderedCandidateIds.Intersect(existingCandidateIds).Any())
            return ("candidate-conflict", "Một số thí sinh đã trong ca thi");

        var existingSupIds = await _organizeExamRepository.GetSupervisorIdsInSessionAsync(organizeExamId, sessionId);
        if (allSupIds.Intersect(existingSupIds).Any())
            return ("supervisor-conflict", "Một số giám thị đã được phân công trong ca thi");

        var hasDuplicateSession = await _organizeExamRepository.HasDuplicateSessionTimeAsync(organizeExamId, sessionId);
        if (hasDuplicateSession)
            return ("session-time-duplicate", "Một ca thi khác đã được thêm với cùng khoảng thời gian");

        var q = new Queue<string>(orderedCandidateIds);
        var roomsToAdd = new List<(string RoomId, List<string> SupervisorIds, List<string> CandidateIds, string? RoomStatus)>();

        foreach (var r in request.RoomIds)
        {
            var cands = new List<string>();
            var take = r.Quantity;
            while (take > 0 && q.Count > 0)
            {
                cands.Add(q.Dequeue());
                take--;
            }

            roomsToAdd.Add((
                RoomId: r.RoomId,
                SupervisorIds: (r.SupervisorIds ?? new()).Distinct().ToList(),
                CandidateIds: cands,
                RoomStatus: "closed"
            ));
        }

        var ok = await _organizeExamRepository.AddRoomsIfAllAbsentAsync(organizeExamId, sessionId, roomsToAdd);
        if (!ok)
            return ("repository-failure", "Thêm phòng thi thất bại");

        foreach (var r in roomsToAdd)
        {
            await _organizeExamRepository.AddTakeExamsForCandidatesAsync(organizeExamId, sessionId, r.RoomId, r.CandidateIds);
            await _organizeExamRepository.AddTrackExamsForSupervisorsAsync(organizeExamId, sessionId, r.RoomId, r.SupervisorIds, "closed");
            await _organizeExamRepository.AddRoomScheduleAsync(organizeExamId, sessionId, r.RoomId, r.CandidateIds.Count);
        }

        return ("success", "Chia phòng thi thành công");
    }



    
    
    
    
    public async Task<(string status, string? newStatus)> UpdateStatusAsync(string id, string newStatus)
    {
        var exam = await _organizeExamRepository.GetByIdAsync(id);
        if (exam == null)
            return ("organize-exam-not-found", null);

        var updated = await _organizeExamRepository.UpdateStatusAsync(id, newStatus);
        if (!updated)
            return ("update-failed", null);

        return ("success", newStatus);
    }
    
    
}
