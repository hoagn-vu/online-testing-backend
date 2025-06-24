using Backend_online_testing.Models;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories;

public class LogRepository
{
    private readonly IMongoCollection<LogsModel> _logs;
    
    public LogRepository(IMongoDatabase database)
    {
        _logs = database.GetCollection<LogsModel>("logs");
    }
    
    public async Task<List<LogsModel>> GetAllLogsAsync()
    {
        return await _logs.Find(_ => true).SortByDescending(l => l.LogAt).ToListAsync();
    }

    public async Task InsertLogAsync(LogsModel log)
    {
        await _logs.InsertOneAsync(log);
    }
}