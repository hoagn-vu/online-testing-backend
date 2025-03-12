#pragma warning disable SA1309
namespace Backend_online_testing.Services
{
    using Backend_online_testing.Models;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public class RoomService
    {
        private readonly IMongoCollection<RoomModel> _roomCollection;

        public RoomService(IMongoDatabase database)
        {
            this._roomCollection = database.GetCollection<RoomModel>("rooms");
        }

        public async Task<List<RoomModel>> GetRoomsAsync() => await this._roomCollection.Find(p => !p.Status.Equals("deleted")).ToListAsync();

        public async Task<RoomModel> GetByIdAsync(ObjectId id) => await this._roomCollection.Find(p => p.Id == id && !p.Status.Equals("deleted")).FirstOrDefaultAsync();

        public async Task CreateAsync(RoomModel room, ObjectId userId)
        {
            room.Logs ??= new List<RoomLogModel>();
            room.Logs.Add(new RoomLogModel { UserId = userId, Type = "created" });
            await this._roomCollection.InsertOneAsync(room);
        }

        public async Task UpdateAsync(ObjectId id, RoomModel room, ObjectId userId)
        {
            var existingRoom = await this.GetByIdAsync(id);
            if (existingRoom == null)
            {
                return;
            }

            room.Id = id;
            room.Logs = existingRoom.Logs ?? new List<RoomLogModel>();
            room.Logs.Add(new RoomLogModel { UserId = userId, Type = "updated" });
            await this._roomCollection.ReplaceOneAsync(r => r.Id == id, room);
        }

        public async Task DeleteAsync(ObjectId id, ObjectId userId)
        {
            var existingRoom = await this.GetByIdAsync(id);
            if (existingRoom == null)
            {
                return;
            }

            existingRoom.Logs ??= new List<RoomLogModel>();
            existingRoom.Logs.Add(new RoomLogModel { UserId = userId, Type = "deleted" });
            await this._roomCollection.ReplaceOneAsync(r => r.Id == id, existingRoom);
            await this._roomCollection.DeleteOneAsync(p => p.Id == id);
        }

        public async Task SoftDeleteAsync(ObjectId id, ObjectId userId)
        {
            var existingRoom = await this.GetByIdAsync(id);
            if (existingRoom == null)
            {
                return;
            }

            existingRoom.Status = "deleted";
            existingRoom.Logs ??= new List<RoomLogModel>();
            existingRoom.Logs.Add(new RoomLogModel { UserId = userId, Type = "deleted" });
            await this._roomCollection.ReplaceOneAsync(r => r.Id == id, existingRoom);
        }
    }
}
