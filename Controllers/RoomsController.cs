#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using Backend_online_testing.DTO;
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly RoomsService _roomsService;

        public RoomsController(RoomsService roomsService)
        {
            this._roomsService = roomsService;
        }

        // Get all room
        [HttpGet]
        public async Task<ActionResult<RoomsModel>> GetAllRoom([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (rooms, total) = await this._roomsService.GetRooms(keyword, page, pageSize);
            return this.Ok(new { rooms, total });
        }

        // Search by name
        [HttpPost("search-name")]
        public async Task<ActionResult<RoomsModel>> SearchByRoomName([FromQuery] string name)
        {
            var rooms = await this._roomsService.SearchByNameRoom(name);
            return this.Ok(rooms);
        }

        // Create Room
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] RoomDto roomDTO)
        {
            string result = await this._roomsService.CreateRoom(roomDTO);

            if (result == "Success")
            {
                return this.Ok(new { message = "Room created successfully" });
            }
            else
            {
                return this.BadRequest(new { message = result });
            }
        }

        // Update Room
        [HttpPost("{roomId}")]
        public async Task<IActionResult> UpdateRoom([FromBody] RoomDto roomDTO, string roomId)
        {
            string result = await this._roomsService.UpdateRoom(roomDTO, roomId);

            if (result == "Success")
            {
                return this.Ok(new { message = "Room updated successfully" });
            }
            else
            {
                return this.BadRequest(new { error = result });
            }
        }

        // Delete Room
        [HttpDelete("delete-room/{roomId}")]
        public async Task<IActionResult> DeleteRoom(string roomId, string userLogId)
        {
            string result = await this._roomsService.DeleteRoom(roomId, userLogId);

            if (result == "Success")
            {
                return this.Ok(new { message = "Success" });
            }
            else
            {
                return this.BadRequest(new { error = result });
            }
        }

        // Insert sample data
        [HttpPost("seed")]
        public async Task<IActionResult> SeedData()
        {
            await this._roomsService.SeedSampleData();
            return this.Ok("Insert Room Data successfully");
        }
    }
}
