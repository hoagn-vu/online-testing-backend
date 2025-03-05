using MongoDB.Driver;
using backend_online_testing.Models;
using backend_online_testing.DTO;
using System.Xml.Linq;
using MongoDB.Bson.Serialization.IdGenerators;
namespace backend_online_testing.Services
{
    public class RoomsService
    {
        private readonly IMongoCollection<RoomsModel> _rooms;

        public RoomsService(IMongoDatabase database)
        {
            _rooms = database.GetCollection<RoomsModel>("Rooms");
        }

        //Get all room
        public async Task<List<RoomsModel>> GetAllRooms()
        {
            return await _rooms.Find(_ => true).ToListAsync();
        }

        //Find room using RoomName
        public async Task<List<RoomsModel>> SearchByNameRoom(string name)
        {
            var filter = Builders<RoomsModel>.Filter.Regex(x => x.RoomName, new MongoDB.Bson.BsonRegularExpression(name, "i"));

            return await _rooms.Find(filter).ToListAsync();
        }
        //Create Room
        public async Task InsertRoom(RoomDTO roomData)
        {
            var roomLogData = new RoomLogsModel
            {
                LogId = "1",
                RoomLogUserId = roomData.UserId,
                RoomLogType = "Created",
                RoomChangeAt = DateTime.UtcNow,
            };

            var lastRoom = await _rooms
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
                Capacity = roomData.Capacity,
                RoomLogs = new List<RoomLogsModel> { roomLogData }
            };

            await _rooms.InsertOneAsync(newRoom);
        }

        //Update Room
        public async Task UpdateRoom(RoomDTO roomData)
        {
            //Find room following RoomName
            var filter = Builders<RoomsModel>.Filter.Eq(r => r.RoomName, roomData.RoomName);
            var room = await _rooms.Find(filter).FirstOrDefaultAsync();

            if (room == null)
            {
                Console.WriteLine("Room Not Found!");
                return;
            }

            //Get ID Room Log
            string newLogId = (room.RoomLogs.Any() && int.TryParse(room.RoomLogs.Max(log => log.LogId), out int lastLogId))
                ? (lastLogId + 1).ToString()
                : "1";

            var roomLogData = new RoomLogsModel
            {
                LogId = newLogId,
                RoomLogUserId = roomData.UserId,
                RoomChangeAt = DateTime.UtcNow,
                RoomLogType = "Updated",
            };

            //Add another field
            var update = Builders<RoomsModel>.Update
                .Set(r => r.RoomStatus, roomData.RoomStatus)
                .Set(r => r.Capacity, roomData.Capacity)
                .Push(r => r.RoomLogs, roomLogData);

            //Update
            await _rooms.UpdateOneAsync(filter, update);
        }

        //Update Room Status - Delete Room
        public async Task DeleteRoom(DeleteRoomDto roomData)
        {
            var filter = Builders<RoomsModel>.Filter.Eq(r => r.RoomName, roomData.RoomName);
            var room = await _rooms.Find(filter).FirstOrDefaultAsync();

            if (room == null)
            {
                Console.WriteLine("Room not exist");
            }

            //Get ID from LogRoom
            string newLogId = (room.RoomLogs.Any() && int.TryParse(room.RoomLogs.Max(log => log.LogId), out int lastLogId))
                ? (lastLogId + 1).ToString() : "1";

            var roomLogData = new RoomLogsModel
            {
                LogId = newLogId,
                RoomLogUserId = roomData.UserId,
                RoomLogType = "Updated",
                RoomChangeAt = DateTime.Now
            };

            //Update file
            var update = Builders<RoomsModel>.Update
                .Set(r => r.RoomStatus, "Unavailable/Deleted")
                .Push(r => r.RoomLogs, roomLogData);

            //Update
            await _rooms.UpdateOneAsync(filter, update);
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
                    Capacity = 10,
                    RoomLogs = new List<RoomLogsModel> {
                        new RoomLogsModel { LogId = "1", RoomLogUserId = "1",RoomChangeAt=DateTime.Parse("2024-02-19"), RoomLogType = "Created" },
                        new RoomLogsModel { LogId = "2", RoomLogUserId = "1", RoomChangeAt=DateTime.Parse("2024-02-19"), RoomLogType = "Updated" }
                    }
                },
                new RoomsModel
                {
                    Id = "2",
                    RoomName = "Room B",
                    RoomStatus = "Available",
                    Capacity = 15,
                    RoomLogs = new List<RoomLogsModel> {
                        new RoomLogsModel { LogId = "1", RoomLogUserId = "1", RoomChangeAt=DateTime.Parse("2024-02-19"), RoomLogType = "Created" },
                        new RoomLogsModel { LogId = "2", RoomLogUserId = "1", RoomChangeAt=DateTime.Parse("2024-02-19"), RoomLogType = "Updated" }
                    }
                }
            };

            await _rooms.InsertManyAsync(sampleRooms);
        }
    }
}
