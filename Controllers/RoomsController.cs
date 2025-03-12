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
        public async Task<ActionResult<RoomsModel>> GetAllRoom()
        {
            var rooms = await this._roomsService.GetAllRooms();
            return this.Ok(rooms);
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
        public async Task<IActionResult> CreateRoom([FromBody] RoomDTO roomDTO)
        {
            await this._roomsService.InsertRoom(roomDTO);
            return this.Ok(new { message = "Room created sucessfully" });
        }

        // Update Room
        [HttpPost("{roomId}")]
        public async Task<IActionResult> UpdateRoom([FromBody] RoomDTO roomDTO)
        {
            await this._roomsService.UpdateRoom(roomDTO);
            return this.Ok(new { message = "Room updated successfully" });
        }

        // Delete Room
        [HttpDelete("delete-room")]
        public async Task<IActionResult> DeleteRoom([FromBody] DeleteRoomDto roomDeleteDTO)
        {
            await this._roomsService.DeleteRoom(roomDeleteDTO);
            return this.Ok(new { message = "Room delete/change status sucessfully" });
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
