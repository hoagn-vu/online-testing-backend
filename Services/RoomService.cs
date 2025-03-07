using Backend_online_testing.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend_online_testing.Services
{
    public class RoomService
    {
        private readonly IMongoCollection<RoomModel> _roomCollection;

        public RoomService(IMongoDatabase database)
        {
            _roomCollection = database.GetCollection<RoomModel>("rooms");
        }

        //public async Task<List<RoomModel>> GetRoomsAsync() => await _roomCollection.Find(p => true).ToListAsync();
        public async Task<List<RoomModel>> GetRoomsAsync() => await _roomCollection.Find(p => !p.Status.Equals("deleted")).ToListAsync();

        public async Task<RoomModel> GetByIdAsync(ObjectId id) => await _roomCollection.Find(p => p.Id == id && !p.Status.Equals("deleted")).FirstOrDefaultAsync();

        public async Task CreateAsync(RoomModel room, ObjectId userId)
        {
            room.Logs ??= new List<RoomLogModel>();
            room.Logs.Add(new RoomLogModel { UserId = userId, Type = "created" });
            await _roomCollection.InsertOneAsync(room);
        }

        public async Task UpdateAsync(ObjectId id, RoomModel room, ObjectId userId)
        {
            var existingRoom = await GetByIdAsync(id);
            if (existingRoom == null) return;

            room.Id = id;
            room.Logs = existingRoom.Logs ?? new List<RoomLogModel>();
            room.Logs.Add(new RoomLogModel { UserId = userId, Type = "updated" });
            await _roomCollection.ReplaceOneAsync(r => r.Id == id, room);
        }

        public async Task DeleteAsync(ObjectId id, ObjectId userId)
        {
            var existingRoom = await GetByIdAsync(id);
            if (existingRoom == null) return;

            existingRoom.Logs ??= new List<RoomLogModel>();
            existingRoom.Logs.Add(new RoomLogModel { UserId = userId, Type = "deleted" });
            await _roomCollection.ReplaceOneAsync(r => r.Id == id, existingRoom);
            await _roomCollection.DeleteOneAsync(p => p.Id == id);
        }

        public async Task SoftDeleteAsync(ObjectId id, ObjectId userId)
        {
            var existingRoom = await GetByIdAsync(id);
            if (existingRoom == null) return;

            existingRoom.Status = "deleted";
            existingRoom.Logs ??= new List<RoomLogModel>();
            existingRoom.Logs.Add(new RoomLogModel { UserId = userId, Type = "deleted" });
            await _roomCollection.ReplaceOneAsync(r => r.Id == id, existingRoom);
        }

        //public async Task CreateAsync(RoomModel room) => await _roomCollection.InsertOneAsync(room);

        //public async Task UpdateAsync(ObjectId id, RoomModel room) => await _roomCollection.ReplaceOneAsync(r => r.Id == id, room);

        //public async Task DeleteAsync(ObjectId id) => await _roomCollection.DeleteOneAsync(p => p.Id == id);
    }
}
