using Backend_online_testing.Models;
using Backend_online_testing.Repositories;

namespace Backend_online_testing.Services;

public class LogService
{
    private readonly LogRepository _logRepository;

    public LogService(LogRepository logRepository)
    {
        _logRepository = logRepository;
    }

    public async Task<List<LogsModel>> GetLogsAsync()
    {
        return await _logRepository.GetAllLogsAsync();
    }

    public async Task AddLogAsync(string madeBy, string action, string details)
    {
        var log = new LogsModel
        {
            MadeBy = madeBy,
            LogAction = action,
            LogDetails = details,
            LogAt = DateTime.UtcNow
        };

        await _logRepository.InsertLogAsync(log);
    }
}