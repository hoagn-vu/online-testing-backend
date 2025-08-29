using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;

namespace Backend_online_testing.Services;

public interface ILogsService
{
    Task WriteLogAsync(CreateLogDto dto);
    Task<List<LogResponseDto>> GetLogsAsync();
    Task<LogResponseDto?> GetLogByIdAsync(string id);
}

public class LogService : ILogsService
{
    private readonly ILogsRepository _logsRepository;

    public LogService(ILogsRepository logsRepository)
    {
        _logsRepository = logsRepository;
    }

    public async Task WriteLogAsync(CreateLogDto dto)
    {
        var log = new LogsModel
        {
            MadeBy = dto.MadeBy,
            LogAction = dto.LogAction,
            LogDetails = dto.LogDetails,
            LogAt = DateTime.UtcNow,
            AffectedObject = dto.AffectedObject ?? null
        };

        await _logsRepository.AddLogAsync(log);
    }

    public async Task<List<LogResponseDto>> GetLogsAsync()
    {
        var logs = await _logsRepository.GetAllLogsAsync();
        return logs.Select(l => new LogResponseDto
        {
            Id = l.Id,
            MadeBy = l.MadeBy,
            LogAction = l.LogAction,
            LogAt = l.LogAt,
            LogDetails = l.LogDetails,
            AffectedObject = l.AffectedObject
        }).ToList();
    }

    public async Task<LogResponseDto?> GetLogByIdAsync(string id)
    {
        var log = await _logsRepository.GetLogByIdAsync(id);
        if (log == null) return null;

        return new LogResponseDto
        {
            Id = log.Id,
            MadeBy = log.MadeBy,
            LogAction = log.LogAction,
            LogAt = log.LogAt,
            LogDetails = log.LogDetails,
            AffectedObject = log.AffectedObject
        };
    }
}
