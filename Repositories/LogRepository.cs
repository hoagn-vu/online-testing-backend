using Backend_online_testing.Models;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories;

public interface ILogsRepository
{
    Task AddLogAsync(LogsModel log);
    Task<List<LogsModel>> GetAllLogsAsync();
    Task<LogsModel?> GetLogByIdAsync(string id);
}

public class LogRepository : ILogsRepository
{
    private readonly IMongoCollection<LogsModel> _logCollection;
    
    public LogRepository(IMongoDatabase database)
    {
        _logCollection = database.GetCollection<LogsModel>("logs");
    }

    public async Task AddLogAsync(LogsModel log)
    {
        await _logCollection.InsertOneAsync(log);
    }

    public async Task<List<LogsModel>> GetAllLogsAsync()
    {
        return await _logCollection.Find(_ => true)
            .SortByDescending(l => l.LogAt)
            .ToListAsync();
    }

    public async Task<LogsModel?> GetLogByIdAsync(string id)
    {
        return await _logCollection.Find(l => l.Id == id).FirstOrDefaultAsync();
    }
}