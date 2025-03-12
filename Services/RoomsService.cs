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
            this._rooms = database.GetCollection<RoomsModel>("Rooms");
        }

        // Get all room
        public async Task<(List<RoomsModel>, long)> GetAllRooms(string? keyword, int page, int pageSize)
        {
            var filter = Builders<RoomsModel>.Filter.Empty;

            if (!string.IsNullOrEmpty(keyword))
            {
                filter = Builders<RoomsModel>.Filter.Or(
                    Builders<RoomsModel>.Filter.Regex(u => u.RoomName, new BsonRegularExpression(keyword, "i")),
                    Builders<RoomsModel>.Filter.Regex(u => u.RoomLocation, new BsonRegularExpression(keyword, "i")),
                    Builders<RoomsModel>.Filter.Regex(u => u.RoomStatus, new BsonRegularExpression(keyword, "i")));
            }

            var totalRecords = await this._rooms.CountDocumentsAsync(filter);

            var rooms = await this._rooms
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

            return (rooms, totalRecords);
        }

        // Find room using RoomName
        public async Task<List<RoomsModel>> SearchByNameRoom(string name)
        {
            var filter = Builders<RoomsModel>.Filter.Regex(x => x.RoomName, new MongoDB.Bson.BsonRegularExpression(name, "i"));

            return await this._rooms.Find(filter).ToListAsync();
        }

        // Create Room
        public async Task InsertRoom(RoomDTO roomData)
        {
            // var roomLogData = new RoomLogsModel
            // {
            //    LogId = "1",
            //    RoomLogUserId = roomData.UserId,
            //    RoomLogType = "Created",
            //    RoomChangeAt = DateTime.UtcNow,
            // };
            var lastRoom = await this._rooms
                .Find(Builders<RoomsModel>.Filter.Empty) // Get all data
                .Sort(Builders<RoomsModel>.Sort.Descending(r => r.Id)) // Sort ID
                .Limit(1) // Get First Element
                .FirstOrDefaultAsync();

            string newId = (int.TryParse(lastRoom?.Id, out int lastId) ? lastId + 1 : 1).ToString();

            var newRoom = new RoomsModel
            {
                Id = newId,
                RoomName = roomData.RoomName,
                RoomStatus = roomData.RoomStatus,
                RoomCapacity = roomData.Capacity,

                // RoomLogs = new List<RoomLogsModel> { roomLogData },
            };

            await this._rooms.InsertOneAsync(newRoom);
        }

        // Update Room
        public async Task UpdateRoom(RoomDTO roomData)
        {
            // Find room following RoomName
            var filter = Builders<RoomsModel>.Filter.Eq(r => r.RoomName, roomData.RoomName);
            var room = await this._rooms.Find(filter).FirstOrDefaultAsync();

            if (room == null)
            {
                Console.WriteLine("Room Not Found!");
                return;
            }

            // Get ID Room Log
            // string newLogId = (room.RoomLogs.Any() && int.TryParse(room.RoomLogs.Max(log => log.LogId), out int lastLogId))
            //    ? (lastLogId + 1).ToString()
            //    : "1";

            // var roomLogData = new RoomLogsModel
            // {
            //    LogId = newLogId,
            //    RoomLogUserId = roomData.UserId,
            //    RoomChangeAt = DateTime.UtcNow,
            //    RoomLogType = "Updated",
            // };

            // Add another field
            var update = Builders<RoomsModel>.Update
                .Set(r => r.RoomStatus, roomData.RoomStatus)
                .Set(r => r.RoomCapacity, roomData.Capacity);

                // .Push(r => r.RoomLogs, roomLogData);

            // Update
            await this._rooms.UpdateOneAsync(filter, update);
        }

        // Update Room Status - Delete Room
        public async Task DeleteRoom(DeleteRoomDto roomData)
        {
            var filter = Builders<RoomsModel>.Filter.Eq(r => r.RoomName, roomData.RoomName);
            var room = await this._rooms.Find(filter).FirstOrDefaultAsync();

            if (room == null)
            {
                Console.WriteLine("Room not exist");
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
                    .Set(r => r.RoomStatus, "Unavailable/Deleted");

                    // .Push(r => r.RoomLogs, roomLogData);

                // Update
                await this._rooms.UpdateOneAsync(filter, update);
            }
        }

        public async Task SeedSampleData()
        {
            var sampleRooms = new List<RoomsModel>
            {
                new RoomsModel
                {
                    Id = "1",
                    RoomName = "Room A",
                    RoomStatus = "Available",
                    RoomCapacity = 10,
                    RoomLocation = "Cơ sở 1",
                },
                new RoomsModel
                {
                    Id = "2",
                    RoomName = "Room B",
                    RoomStatus = "Available",
                    RoomCapacity = 15,
                    RoomLocation = "Cơ sở 2",
                },
            };

            await this._rooms.InsertManyAsync(sampleRooms);
        }
    }
}
