using Amazon.S3;
using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;

namespace Backend_online_testing.Services;

public class StatisticsService
{
    private readonly StatisticsRepository _statisticsRepository;

    public StatisticsService(StatisticsRepository statisticRepository)
    {
        _statisticsRepository = statisticRepository;
    }

    public async Task<OrganizeExamScoreStatistisDto> GetScoreHistogram10Async(string organizeExamId)
    {
        var organizeExam = await _statisticsRepository.GetOrganizeExamById(organizeExamId)
                  ?? throw new KeyNotFoundException("OrganizeExam not found");

        var subjectName = await _statisticsRepository.GetSubjectNameByIdAsync(organizeExam.SubjectId);

        var dto = new OrganizeExamScoreStatistisDto
        {
            OrganizeExamId = organizeExam.Id,
            OrganizeExamName = organizeExam.OrganizeExamName,
            SubjecName = subjectName ?? ""

        };

        var bins = new int[10];
        int noScoreCount = 0;
        double min = double.PositiveInfinity;
        double max = double.NegativeInfinity;
        double sum = 0.0;

        var sessions = organizeExam.Sessions ?? new List<SessionsModel>();

        foreach (var ss in sessions)
        {
            var sessionId = ss.SessionId;

            var rooms = ss.RoomsInSession ?? new List<SessionRoomsModel>();
            foreach (var room in rooms)
            {
                var roomId = room.RoomInSessionId;
                var candidates = room.CandidateIds ?? new List<string>();

                foreach (var candId in candidates)
                {
                    dto.TotalCandidates++;

                    // get user score -> takeExams following (organizeExamId, sessionId, roomId)
                    var score = await _statisticsRepository.GetTotalScoreFromTakeExamAsync(
                        candId, organizeExam.Id, sessionId, roomId);

                    if (score is null)
                    {
                        noScoreCount++; // No score
                        continue;
                    }

                    //Min, max, avg
                    var s = score.Value;
                    if (s < min) min = s;
                    if (s > max) max = s;
                    sum += s;

                    var idx = (int)Math.Floor(score.Value);
                    if (idx < 0) idx = 0;
                    if (idx > 9) idx = 9;

                    bins[idx]++;
                }
            }
        }

        // map bins -> DTO
        dto.ScoreDistribution.Bin0_1 = bins[0];
        dto.ScoreDistribution.Bin1_2 = bins[1];
        dto.ScoreDistribution.Bin2_3 = bins[2];
        dto.ScoreDistribution.Bin3_4 = bins[3];
        dto.ScoreDistribution.Bin4_5 = bins[4];
        dto.ScoreDistribution.Bin5_6 = bins[5];
        dto.ScoreDistribution.Bin6_7 = bins[6];
        dto.ScoreDistribution.Bin7_8 = bins[7];
        dto.ScoreDistribution.Bin8_9 = bins[8];
        dto.ScoreDistribution.Bin9_10 = bins[9];
        dto.MinScore = dto.TotalCandidates > 0 ? min : (double?)null;
        dto.MaxScore = dto.TotalCandidates > 0 ? max : (double?)null;
        dto.AverageScore = dto.TotalCandidates > 0 ? Math.Round(sum / dto.TotalCandidates, 2) : (double?)null;
        return dto;
    }

    public async Task<ParticipationViolationDto> GetParticipationViolationAsync(string organizeExamId)
    {
        var organizeExam = await _statisticsRepository.GetOrganizeExamById(organizeExamId)
                  ?? throw new KeyNotFoundException("OrganizeExam not found");

        var dto = new ParticipationViolationDto
        {
            OrganizeExamId = organizeExam.Id,
            OrganizeExamName = organizeExam.OrganizeExamName
        };

        var sessions = organizeExam.Sessions ?? new List<SessionsModel>();
        foreach (var ss in sessions)
        {
            var sessionId = ss.SessionId;
            var rooms = ss.RoomsInSession ?? new List<SessionRoomsModel>();

            foreach (var room in rooms)
            {
                var roomId = room.RoomInSessionId;
                var candidates = room.CandidateIds ?? new List<string>();

                foreach (var candId in candidates)
                {
                    dto.TotalCandidates++;

                    var status = await _statisticsRepository.GetTakeExamStatusAsync(
                        candId, organizeExam.Id, sessionId, roomId);

                    if (string.Equals(status, "terminate", StringComparison.OrdinalIgnoreCase))
                        dto.TotalCandidateTerminated++;
                }
            }
        }

        return dto;
    }
}
