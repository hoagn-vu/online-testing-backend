using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories;

public class LevelRepository
{
    private readonly IMongoCollection<LevelModel> _levels;

    public LevelRepository(IMongoDatabase database)
    {
        _levels = database.GetCollection<LevelModel>("level");
    }

    private static readonly FilterDefinition<LevelModel> LevelBaseFilter = Builders<LevelModel>.Filter.Empty;

    private FilterDefinition<LevelModel> LevelFilterByName(string? keyword)
    {
        var builder = Builders<LevelModel>.Filter;

        if (string.IsNullOrEmpty(keyword))
        {
            return LevelBaseFilter;
        }

        return builder.And(
                LevelBaseFilter,
                builder.Regex(s => s.LevelName, new BsonRegularExpression(keyword, "i"))
        );
    }

    //Get all level
    public async Task<List<LevelModel>> GetAllLevelAsync(string? keyword, int page, int pageSize)
    {
        var filter = LevelFilterByName(keyword);

        return await _levels.Find(filter)
            .SortByDescending(l => l.Id)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    //Get level by id
    public async Task<LevelModel> GetLevelByIdAsync(string id)
    {
        return await _levels.Find(l => l.Id == id).FirstOrDefaultAsync();
    }

    //Create new level
    public async Task CreateLevelAsync(LevelModel level)
    {
        await _levels.InsertOneAsync(level);
    }

    //Delete level
    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _levels.DeleteOneAsync(l => l.Id == id);
        return result.DeletedCount > 0;
    }

    //Update level name
    public async Task<bool> UpdateNameAsync(string id, string newName)
    {
        var update = Builders<LevelModel>.Update.Set(l => l.LevelName, newName);
        var result = await _levels.UpdateOneAsync(l => l.Id == id, update);
        return result.ModifiedCount > 0;
    }
}
