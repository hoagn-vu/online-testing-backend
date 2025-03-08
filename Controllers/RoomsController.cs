using backend_online_testing.Models;
using backend_online_testing.Services;
using backend_online_testing.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_online_testing.Controllers
{
    [Route("api/rooms")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly RoomsService _roomsService;

        public RoomsController(RoomsService roomsService)
        {
            _roomsService = roomsService;
        }

        //Get all room
        [HttpGet]
        public async Task<ActionResult<RoomsModel>> GetAllRoom()
        {
            var rooms = await _roomsService.GetAllRooms();
            return Ok(rooms);
        }

        //Search by name
        [HttpPost("search-name")]
        public async Task<ActionResult<RoomsModel>> SearchByRoomName([FromQuery] string name)
        {
            var rooms = await _roomsService.SearchByNameRoom(name);
            return Ok(rooms);
        }

        //Create Room
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] RoomDTO roomDTO)
        {
            await _roomsService.InsertRoom(roomDTO);
            return Ok(new { message = "Room created sucessfully" });
        }

        //Update Room
        [HttpPost("{roomId}")]
        public async Task<IActionResult> UpdateRoom([FromBody] RoomDTO roomDTO)
        {
            await _roomsService.UpdateRoom(roomDTO);
            return Ok(new { message = "Room updated successfully" });
        }

        //Delete Room
        [HttpDelete("delete-room")]
        public async Task<IActionResult> DeleteRoom([FromBody] DeleteRoomDto roomDeleteDTO)
        {
            await _roomsService.DeleteRoom(roomDeleteDTO);
            return Ok(new { message = "Room delete/change status sucessfully" });
        }

        //Insert sample data
        [HttpPost("seed")]
        public async Task<IActionResult> SeedData()
        {
            await _roomsService.SeedSampleData();
            return Ok("Insert Room Data successfully");
        }
    }
}
