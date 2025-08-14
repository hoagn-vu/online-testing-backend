using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Backend_online_testing.Services;

public class TrackExamService
{
    private readonly TrackExamRepository _trackExamRepository;

    public TrackExamService(TrackExamRepository trackExamRepository)
    {
        _trackExamRepository = trackExamRepository;
    }

    //Get all track exam
    public async Task<UserAllTrackExamResponseDto> GetAllTrackExamAsync(string userId)
    {
        var user = await _trackExamRepository.GetUserByUserId(userId);

        if (user == null || user.TrackExam == null || !user.TrackExam.Any())
        {
            return new UserAllTrackExamResponseDto
            {
                UserId = userId,
                TrackExams = new List<TrackExamsInfo>()
            };
        }

        var trackExamInfos = new List<TrackExamsInfo>();

        foreach (var trackExam in user.TrackExam)
        {
            var organizeExam = await _trackExamRepository.GetOrganizeExamById(trackExam.OrganizeExamId);

            trackExamInfos.Add(new TrackExamsInfo
            {
                TrackExamId = trackExam.Id,
                OrganizeExamId = trackExam.OrganizeExamId,
                SessionId = trackExam.SessionId,
                RoomId = trackExam.RoomId,
                OrganizeExamName = organizeExam ?.OrganizeExamName ?? string.Empty
            });
        }

        return new UserAllTrackExamResponseDto
        {
            UserId = userId,
            TrackExams = trackExamInfos
        };
    }

    //Get track exam by id
    public async Task<bool> DeleteTrackExamByIdAsync(string userId, string trackExamId)
    {
        return await _trackExamRepository.DeleteTrackExamByIdAsync(userId, trackExamId);
    }

    //Create track exam
    public async Task<bool> CreateTrackExamAsync(string userId, CreateTrackExamDto trackExamDto)
    {
        return await _trackExamRepository.CreateTrackExamAsync(userId, trackExamDto);
    }

    //Get list user name, user code and user status
    public async Task<CandidateDetailsDto> GetCandidateDetailsWithStatus(string organizeExamId, string sessionId, string roomId)
    {
        var organizeExam = await _trackExamRepository.GetOrganizeExamById(organizeExamId);
        if (organizeExam == null || organizeExam.Sessions == null)
        {
            return new CandidateDetailsDto { OrganizeExamId = organizeExamId, SessionId = sessionId, RoomId = roomId };
        }

        var totalQuestions = organizeExam.TotalQuestions ?? 0;
        var session = organizeExam.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session == null || session.RoomsInSession == null)
        {
            return new CandidateDetailsDto { OrganizeExamId = organizeExamId, SessionId = sessionId, RoomId = roomId };
        }

        var room = session.RoomsInSession.FirstOrDefault(r => r.RoomInSessionId == roomId);
        var candidateIds = room?.CandidateIds ?? new List<string>();
        if (!candidateIds.Any())
        {
            return new CandidateDetailsDto { OrganizeExamId = organizeExamId, SessionId = sessionId, RoomId = roomId };
        }

        var userDetails = new List<CandidateInfoDto>();
        foreach (var candidateId in candidateIds)
        {
            var user = await _trackExamRepository.GetUserByUserId(candidateId);
            if (user != null)
            {
                var takeExam = user.TakeExam?
                .FirstOrDefault(t =>
                    t.OrganizeExamId == organizeExamId &&
                    t.SessionId == sessionId &&
                    t.RoomId == roomId);

                var takeExamStatus = takeExam?.Status ?? "Not Started";
                var startAt = takeExam?.StartAt;
                var finishAt = takeExam?.FinishedAt;
                var progress = takeExam?.Progress ?? 0;

                userDetails.Add(new CandidateInfoDto { UserName = user.UserName, UserCode = user.UserCode, FullName = user.FullName, Status = takeExamStatus, StartAt = startAt, FinishAt = finishAt, Progress = progress, TotalQuestions = totalQuestions });
            }
        }

        // Ensure a return statement after the loop
        return new CandidateDetailsDto
        {
            OrganizeExamId = organizeExamId,
            SessionId = sessionId,
            RoomId = roomId,
            Candidates = userDetails
        };
    }
    //Report 
    public async Task<ReportDto> Report(string organizeExamId, string sessionId, string roomId)
    {
        var organizeExam = await _trackExamRepository.GetOrganizeExamById(organizeExamId);
        if (organizeExam == null || organizeExam.Sessions == null)
        {
            return new ReportDto { OrganizeExamId = organizeExamId, SessionId = sessionId, RoomId = roomId };
        }

        var subjectName = await _trackExamRepository.GetSubjectNameByIdAsync(organizeExam.SubjectId) ?? "";
        var session = organizeExam.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session == null || session.RoomsInSession == null)
        {
            return new ReportDto { OrganizeExamId = organizeExamId, SessionId = sessionId, RoomId = roomId };
        }

        var room = session.RoomsInSession.FirstOrDefault(r => r.RoomInSessionId == roomId);
        var roomName = await _trackExamRepository.GetRoomNameByIdAsync(room.RoomInSessionId) ?? "";

        var candidateIds = room?.CandidateIds ?? new List<string>();
        if (!candidateIds.Any())
        {
            return new ReportDto { OrganizeExamId = organizeExamId, SessionId = sessionId, RoomId = roomId };
        }

        var userDetails = new List<CandidateReportInfo>();
        foreach (var candidateId in candidateIds)
        {
            var user = await _trackExamRepository.GetUserByUserId(candidateId);
            if (user != null)
            {
                var takeExam = user.TakeExam?
                .FirstOrDefault(t =>
                    t.OrganizeExamId == organizeExamId &&
                    t.SessionId == sessionId &&
                    t.RoomId == roomId);

                var totalScore = takeExam?.TotalScore ?? 0;
                var date = user.DateOfBirth ?? ""; 

                userDetails.Add(new CandidateReportInfo { UserId = user.Id, FullName = user.FullName, UserCode = user.UserCode, DateOfBirth = date, TotalScore = totalScore });
            }
        }

        // Ensure a return statement after the loop
        return new ReportDto
        {
            OrganizeExamId = organizeExamId,
            OrganizeName = organizeExam.OrganizeExamName,
            SubjectName = subjectName,
            SessionId = sessionId,
            SessionName = session.SessionName,
            RoomId = roomId,
            RoomName = roomName,
            Candidates = userDetails
        };
    }
}
