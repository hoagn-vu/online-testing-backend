using MongoDB.Driver;
using Backend_online_testing.Models;
using Backend_online_testing.Dtos;
using Microsoft.Extensions.Configuration;
namespace Backend_online_testing.Repositories;

public class RoomRepository
{
    private readonly IMongoCollection<RoomsModel> _rooms;
    
    public RoomRepository(IMongoDatabase database)
    {
        _rooms = database.GetCollection<RoomsModel>("rooms");
    }
    
    public async Task<long> CountAsync(FilterDefinition<RoomsModel> filter)
    {
        return await _rooms.CountDocumentsAsync(filter);
    }

    public async Task<List<RoomsModel>> GetRoomsAsync(FilterDefinition<RoomsModel> filter, int skip, int limit)
    {
        return await _rooms.Find(filter)
            .Skip(skip)
            .Limit(limit)
            .SortByDescending(r => r.Id)
            .ToListAsync();
    }

    public async Task<RoomsModel?> GetRoomByIdAsync(string roomId)
    {
        return await _rooms.Find(r => r.Id == roomId).FirstOrDefaultAsync();
    }
    
    public async Task<List<RoomOptionsDto>> GetRoomOptionsAsync()
    {
        var filter = Builders<RoomsModel>.Filter.Ne(r => r.RoomStatus, "deleted");
        var projection = Builders<RoomsModel>.Projection
            .Expression(rm => new RoomOptionsDto
            {
                RoomId = rm.Id,
                RoomName = rm.RoomName,
                RoomLocation = rm.RoomLocation,
                RoomCapacity = rm.RoomCapacity
            });

        return await _rooms.Find(filter).Project(projection).ToListAsync();
    }

    public async Task InsertAsync(RoomsModel room)
    {
        await _rooms.InsertOneAsync(room);
    }

    public async Task<RoomsModel?> GetByIdAsync(string roomId)
    {
        var filter = Builders<RoomsModel>.Filter.Eq(r => r.Id, roomId);
        return await _rooms.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<UpdateResult> UpdateAsync(string roomId, UpdateDefinition<RoomsModel> update)
    {
        var filter = Builders<RoomsModel>.Filter.Eq(r => r.Id, roomId);
        return await _rooms.UpdateOneAsync(filter, update);
    }
}