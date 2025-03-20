#pragma warning disable SA1309
namespace Backend_online_testing.Services
{
    using System.Xml.Linq;
    using Backend_online_testing.DTO;
    using Backend_online_testing.Models;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.IdGenerators;
    using MongoDB.Driver;

    public class RoomsService
    {
        private readonly IMongoCollection<RoomsModel> _rooms;

        public RoomsService(IMongoDatabase database)
        {
            _rooms = database.GetCollection<RoomsModel>("rooms");
        }

        // Get all room
        public async Task<(List<RoomsModel>, long)> GetRooms(string? keyword, int page, int pageSize)
        {
            var filter = Builders<RoomsModel>.Filter.Ne(r => r.RoomStatus, "deleted");

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = Builders<RoomsModel>.Filter.Regex(r => r.RoomName, new BsonRegularExpression(keyword, "i"));
                // filter = Builders<RoomsModel>.Filter.Or(
                //     Builders<RoomsModel>.Filter.Regex(u => u.RoomName, new BsonRegularExpression(keyword, "i")),
                //     Builders<RoomsModel>.Filter.Regex(u => u.RoomLocation, new BsonRegularExpression(keyword, "i")),
                //     Builders<RoomsModel>.Filter.Regex(u => u.RoomStatus, new BsonRegularExpression(keyword, "i")));
            }

            var totalCount = await _rooms.CountDocumentsAsync(filter);

            var rooms = await _rooms
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

            return (rooms, totalCount);
        }

        // Find room using RoomName
        public async Task<List<RoomsModel>> SearchByNameRoom(string name)
        {
            var filter = Builders<RoomsModel>.Filter.Regex(x => x.RoomName, new MongoDB.Bson.BsonRegularExpression(name, "i"));

            return await this._rooms.Find(filter).ToListAsync();
        }

        // Create Room
        public async Task<string> CreateRoom(RoomDto roomData)
        {
            if (string.IsNullOrWhiteSpace(roomData.RoomName))
            {
                return "Room name cannot be empty.";
            }

            var newId = ObjectId.GenerateNewId().ToString();

            var newRoom = new RoomsModel
            {
                Id = newId,
                RoomName = roomData.RoomName,
                RoomStatus = "Active",
                RoomCapacity = roomData.RoomCapacity,
                RoomLocation = roomData.RoomLocation,
            };

            await this._rooms.InsertOneAsync(newRoom);

            var insertedRoom = await this._rooms.Find(r => r.Id == newId).FirstOrDefaultAsync();
            return insertedRoom != null ? "Success" : "Failed to create room.";
        }

        // Update Room
        public async Task<string> UpdateRoom(RoomDto roomData, string roomId)
        {
            // Find room following RoomName
            var filter = Builders<RoomsModel>.Filter.Eq(r => r.Id, roomId);
            var room = await this._rooms.Find(filter).FirstOrDefaultAsync();

            if (room == null)
            {
                return "Room Not Found!";
            }

            // Add another field
            var update = Builders<RoomsModel>.Update
                .Set(r => r.RoomStatus, roomData.RoomStatus)
                .Set(r => r.RoomCapacity, roomData.RoomCapacity)
                .Set(r => r.RoomName, roomData.RoomName)
                .Set(r => r.RoomLocation, roomData.RoomLocation);

            // Update
            var result = await this._rooms.UpdateOneAsync(filter, update);

            if (result.MatchedCount > 0 && result.ModifiedCount > 0)
            {
                return "Success";
            }
            else if (result.MatchedCount > 0)
            {
                return "No changes made";
            }
            else
            {
                return "Update Failed";
            }
        }

        // Update Room Status - Delete Room
        public async Task<string> DeleteRoom(string roomId, string userLogId)
        {
            var filter = Builders<RoomsModel>.Filter.Eq(r => r.Id, roomId);
            var room = await this._rooms.Find(filter).FirstOrDefaultAsync();

            if (room == null)
            {
                return "Room Not Found!";
            }
            else
            {
                // Get ID from LogRoom
                // string newLogId = (room.RoomLogs.Count != 0 && int.TryParse(room.RoomLogs.Max(log => log.LogId), out int lastLogId))
                //     ? (lastLogId + 1).ToString() : "1";
                // var roomLogData = new RoomLogsModel
                // {
                //    LogId = newLogId,
                //    RoomLogUserId = roomData.UserId,
                //    RoomLogType = "Updated",
                //    RoomChangeAt = DateTime.Now,
                // };

                // Update file
                var update = Builders<RoomsModel>.Update
                    .Set(r => r.RoomStatus, "Disable");

                // .Push(r => r.RoomLogs, roomLogData);

                // Update
                var result = await this._rooms.UpdateOneAsync(filter, update);

                return result.ModifiedCount > 0 ? "Success" : "Disable room error";
            }
        }

        public async Task SeedSampleData()
        {
            var sampleRooms = new List<RoomsModel>
            {
                new RoomsModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    RoomName = "Room A",
                    RoomStatus = "available",
                    RoomCapacity = 10,
                    RoomLocation = "Cơ sở 1",
                },
                new RoomsModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    RoomName = "Room B",
                    RoomStatus = "available",
                    RoomCapacity = 15,
                    RoomLocation = "Cơ sở 2",
                },
            };

            await this._rooms.InsertManyAsync(sampleRooms);
        }
    }
}
