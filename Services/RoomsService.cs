using Backend_online_testing.Dtos;
using Backend_online_testing.Repositories;
using Backend_online_testing.Models;
using System.Xml.Linq;
using Backend_online_testing.DTO;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace Backend_online_testing.Services
{
    public class RoomsService
    {
        //private readonly IMongoCollection<RoomsModel> _rooms;
        private readonly RoomRepository _roomRepository;
        private readonly LogService _logService;
        
        public RoomsService(RoomRepository repository, LogService logService)
        {
            _roomRepository = repository;
            _logService = logService;
        }

        // Get all room
        public async Task<(List<RoomsModel>, long)> GetRooms(string? keyword, int page, int pageSize)
        {
            var filter = Builders<RoomsModel>.Filter.Ne(r => r.RoomStatus, "deleted");

            if (!string.IsNullOrEmpty(keyword))
            {
                var keywordFilter = Builders<RoomsModel>.Filter.Regex(r => r.RoomName, new BsonRegularExpression(keyword, "i"));
                filter = Builders<RoomsModel>.Filter.And(filter, keywordFilter);

                // var regex = new BsonRegularExpression(keyword, "i");
                // var keywordFilter = Builders<RoomsModel>.Filter.Or(
                //     Builders<RoomsModel>.Filter.Regex(r => r.RoomName, regex),
                //     Builders<RoomsModel>.Filter.Regex(r => r.RoomLocation, regex),
                //     Builders<RoomsModel>.Filter.Regex(r => r.RoomStatus, regex));
                // filter = Builders<RoomsModel>.Filter.And(filter, keywordFilter);
            }

            var total = await _roomRepository.CountAsync(filter);
            var rooms = await _roomRepository.GetRoomsAsync(filter, (page - 1) * pageSize, pageSize);

            return (rooms, total);
        }
        
        public async Task<List<RoomOptionsDto>> GetRoomOptionsAsync()
        {
            return await _roomRepository.GetRoomOptionsAsync();
        }

        public async Task<string> CreateRoomAsync(RoomDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.RoomName))
                return "Room name cannot be empty";

            var newRoom = new RoomsModel
            {
                Id = ObjectId.GenerateNewId().ToString(),
                RoomName = dto.RoomName,
                RoomStatus = "available",
                RoomCapacity = dto.RoomCapacity,
                RoomLocation = dto.RoomLocation
            };

            await _roomRepository.InsertAsync(newRoom);
            await _logService.AddLogAsync("update thanh userId sau", "create", $"Thêm phòng: {newRoom.RoomName}");

            var inserted = await _roomRepository.GetByIdAsync(newRoom.Id);
            return inserted != null ? "Success" : "Failed to create room.";
        }

        public async Task<string> UpdateRoomAsync(RoomDto dto, string roomId)
        {
            var existingRoom = await _roomRepository.GetByIdAsync(roomId);
            if (existingRoom == null)
                return "Room Not Found!";

            var update = Builders<RoomsModel>.Update
                .Set(r => r.RoomName, dto.RoomName)
                .Set(r => r.RoomStatus, dto.RoomStatus)
                .Set(r => r.RoomCapacity, dto.RoomCapacity)
                .Set(r => r.RoomLocation, dto.RoomLocation);

            var result = await _roomRepository.UpdateAsync(roomId, update);
            if (result.ModifiedCount > 0)
            {
                await _logService.AddLogAsync("update thanh userId sau", "update", $"Cập nhật phòng {roomId} - {dto.RoomName}");
            }

            return result.MatchedCount switch
            {
                > 0 when result.ModifiedCount > 0 => "Success",
                > 0 => "No changes made",
                _ => "Update Failed"
            };
        }

        public async Task<string> DeleteRoomAsync(string roomId)
        {
            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room == null)
                return "Room Not Found!";

            var update = Builders<RoomsModel>.Update
                .Set(r => r.RoomStatus, "deleted");
                // Nếu có thêm log: .Push(r => r.RoomLogs, roomLog)

            var result = await _roomRepository.UpdateAsync(roomId, update);
            if (result.ModifiedCount > 0)
            {
                await _logService.AddLogAsync("update thanh userId sau", "delete", $"Xóa  phòng {roomId}");
            }
            return result.ModifiedCount > 0 ? "Success" : "Disable room error";
        }
    }
}
