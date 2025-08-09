using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Repositories;
using MongoDB.Bson;

namespace Backend_online_testing.Services;

public class LevelService
{
    private readonly LevelRepository _levelRepository;

    public LevelService(LevelRepository repository)
    {
        _levelRepository = repository;
    }

    //Get all level
    public async Task<List<LevelModel>> GetAllLevelAsync(int page, int pageSize)
    {
        return await _levelRepository.GetAllLevelAsync(page, pageSize);
    }

    //Get level by id
    public async Task<LevelModel?> GetLevelByIdAsync(string id)
    {
        return await _levelRepository.GetLevelByIdAsync(id);
    }

    //Create new level
    public async Task<LevelModel> CreateLevelAsync(LevelCreateDto dto)
    {
        var level = new LevelModel
        {
            Id = ObjectId.GenerateNewId().ToString(),
            LevelName = dto.LevelName
        };

        await _levelRepository.CreateLevelAsync(level);

        return level;
    }

    //Delete Level
    public async Task<bool> DeleteLevelAsync(string id)
    {
        return await _levelRepository.DeleteAsync(id);
    }

    //Update level name
    public async Task<bool> UpdateLevelNameAsync(string id, string newName)
    {
        return await _levelRepository.UpdateNameAsync(id, newName);
    }
}
