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

    public async Task<ExamSetStatisticDto> ExamSetSatisticAsync(string organizeExamId)
    {
        var organizeExam = await _statisticsRepository.GetOrganizeExamById(organizeExamId)
              ?? throw new KeyNotFoundException("OrganizeExam not found");

        var examSet = organizeExam.Exams ?? new List<string>();
        var nameMap = await _statisticsRepository.GetExamNamesByIdsAsync(examSet);

        //Map examId to ExamCountItem
        var examCountMap = examSet.Distinct()
        .ToDictionary(
            id => id,
            id => new ExamCountItem
            {
                ExamId = id,
                ExamName = (nameMap.TryGetValue(id, out var nm) ? nm : "") ?? "",
                Count = 0
            });

        //Get candidate and assignment
        var allCandidateIds = new HashSet<string>();
        var assignments = new List<(string SessionId, string RoomId, List<string> CandidateIds)>();
        foreach (var ss in organizeExam.Sessions ?? new List<SessionsModel>())
        {
            foreach (var room in ss.RoomsInSession ?? new List<SessionRoomsModel>())
            {
                var cands = room.CandidateIds ?? new List<string>();
                assignments.Add((ss.SessionId, room.RoomInSessionId, cands));
                assignments.Add((ss.SessionId, room.RoomInSessionId, cands));
                foreach (var cid in cands) allCandidateIds.Add(cid);
            }
        }

        var users = await _statisticsRepository.GetUsersByIdsAsync(allCandidateIds);
        var takeExamLookup = users.ToDictionary(u => u.Id, u => u.TakeExam ?? new List<TakeExamsModel>());

        foreach (var (sessionId, roomId, cands) in assignments)
        {
            foreach (var candId in cands)
            {
                if (!takeExamLookup.TryGetValue(candId, out var takeExams) || takeExams.Count == 0)
                    continue;

                var te = takeExams.FirstOrDefault(x =>
                    x.OrganizeExamId == organizeExam.Id &&
                    x.SessionId == sessionId &&
                    x.RoomId == roomId);

                var examId = te?.ExamId;
                if (!string.IsNullOrEmpty(examId) && examCountMap.TryGetValue(examId!, out var item))
                    item.Count++;
            }
        }

        return new ExamSetStatisticDto
        {
            OrganizeExamId = organizeExam.Id,
            OrganizeExamName = organizeExam.OrganizeExamName,
            TotalCandidates = allCandidateIds.Count,
            ExamCounts = examSet.Distinct().Select(id => examCountMap[id]).ToList()
        };
    }

    public async Task<ExamQuestionStatsResponse> GetExamQuestionStatsAsync(string organizeExamId, string examId)
    {
        // 1) Lấy organizeExam và validate examId thuộc examSet
        var organize = await _statisticsRepository.GetOrganizeExamById(organizeExamId)
                      ?? throw new KeyNotFoundException("Organize exam not found");

        if (organize.Exams == null || !organize.Exams.Contains(examId))
            throw new InvalidOperationException("ExamId không thuộc examSet của organizeExam");

        // 2) Tách ids từ sessions -> rooms -> candidateIds
        var sessionIds = (organize.Sessions ?? new())
            .Select(s => s.SessionId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var roomIds = (organize.Sessions ?? new())
            .SelectMany(s => s.RoomsInSession ?? new())
            .Select(r => r.RoomInSessionId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var candidateIds = (organize.Sessions ?? new())
            .SelectMany(s => s.RoomsInSession ?? new())
            .SelectMany(r => r.CandidateIds ?? new())
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        if (candidateIds.Count == 0)
        {
            return new ExamQuestionStatsResponse
            {
                OrganizeExamId = organizeExamId,
                ExamId = examId,
                Questions = new()
            };
        }

        // 3) Gọi repository chạy aggregation (pure query)
        var questionStats = await _statisticsRepository.AggregateQuestionStatsAsync(
            organizeExamId, examId, sessionIds, roomIds, candidateIds);

        // 4) Trả về
        return new ExamQuestionStatsResponse
        {
            OrganizeExamId = organizeExamId,
            ExamId = examId,
            Questions = questionStats
        };
    }

    //Random exam statistic
    public async Task<QuestionBankStatusDto> GetQuestionBankStatusAsync(
        string organizeExamId)
    {
        // 1) Lấy organize exam
        var organize = await _statisticsRepository.GetOrganizeExamById(organizeExamId)
                        ?? throw new InvalidOperationException("OrganizeExam not found.");

        var organizeExamName = organize.OrganizeExamName;

        var subjectId = organize.SubjectId
                        ?? throw new InvalidOperationException("OrganizeExam missing SubjectId.");
        var questionBankId = organize.QuestionBankId
                        ?? throw new InvalidOperationException("OrganizeExam missing QuestionBankId.");

        var subjectName = await _statisticsRepository.GetSubjectNameByIdAsync(subjectId);

        var questionBankName = await _statisticsRepository.GetQuestionBankNameAsync(subjectId, questionBankId);
        // 2) Thu thập candidateIds theo session/room
        var candidateIds = _statisticsRepository.CollectionCandidateIds(organize);

        // 3) Lấy danh sách câu hỏi & đáp án (SelectedCount = 0 ban đầu)
        var questions = await _statisticsRepository.GetQuestionsByOrganizeExamAsync(subjectId, questionBankId);
        long participants = 0L;

        // 4) Nếu có thí sinh, tính thống kê và merge vào Options.SelectedCount
        if (candidateIds.Count > 0 && questions.Count > 0)
        {
            // 1) Thống kê lượt chọn theo option
            var counts = await _statisticsRepository.AggregateOptionCountsAsync(organizeExamId, candidateIds);

            if (counts.Count > 0)
            {
                var usedQids = new HashSet<string>(counts.Keys);
                questions = questions.Where(q => usedQids.Contains(q.QuestionId)).ToList();
            }

            ApplyCounts(questions, counts);

            // 2) Tính tổng đúng/sai mỗi câu
            ComputePerQuestionTotals(questions);

            // 3) Đếm số thí sinh tham gia
            participants = await _statisticsRepository.CountParticipantsAsync(organizeExamId, candidateIds);
        }

        // 5) Trả về đúng DTO mong muốn
        return new QuestionBankStatusDto
        {
            OrganizeExamId = organizeExamId,
            OrganizeExamName = organizeExamName,
            SubjectName = subjectName,
            SubjecId = subjectId,
            QuestionBankId = questionBankId,
            QuestionBankName = questionBankName,
            Questions = questions,
            Participants = participants
        };
    }

    private static void ApplyCounts(
        List<QuestionItemDto> questions,
        Dictionary<string, Dictionary<string, long>> counts)
    {
        foreach (var q in questions)
        {
            if (!counts.TryGetValue(q.QuestionId, out var optionMap))
                continue;

            foreach (var opt in q.Options)
            {
                if (string.IsNullOrWhiteSpace(opt.OptionId)) continue;
                opt.SelectedCount = optionMap.TryGetValue(opt.OptionId, out var c) ? c : 0L;
            }

            if (optionMap.TryGetValue("__NONE__", out var noSel))
            {
                q.NoSelection = noSel;
            }
            else
            {
                q.NoSelection = 0;
            }
        }
    }

    private static void ComputePerQuestionTotals(List<QuestionItemDto> questions)
    {
        foreach (var q in questions)
        {
            long total = 0, correct = 0, incorrect = 0;

            foreach (var opt in q.Options)
            {
                var c = opt.SelectedCount;
                total += c;
                if (opt.IsCorrect) correct += c;
                else incorrect += c;
            }

            total += q.NoSelection;

            q.TotalSelections = total;
            q.CorrectSelections = correct;
            q.IncorrectSelections = incorrect;
        }
    }
}
