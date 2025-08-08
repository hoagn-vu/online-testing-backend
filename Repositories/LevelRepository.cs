using Backend_online_testing.Models;
using MongoDB.Driver;

namespace Backend_online_testing.Repositories;

public class LevelRepository
{
    private readonly IMongoCollection<LevelModel> _levels;

    public LevelRepository(IMongoDatabase database)
    {
        _levels = database.GetCollection<LevelModel>("level");
    }

    //Get all level
    public async Task<List<LevelModel>> GetAllLevelAsync()
    {
        return await _levels.Find(_ => true).ToListAsync();
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
