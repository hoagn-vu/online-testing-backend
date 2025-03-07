using Backend_online_testing.Models;
using Backend_online_testing.Services;
using Backend_online_testing.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly RoomsService _roomsService;

        public RoomsController(RoomsService roomsService)
        {
            _roomsService = roomsService;
        }
        [HttpGet("Room")]
        public async Task<ActionResult<RoomsModel>> GetAllRoom()
        {
            var rooms = await _roomsService.GetAllRooms();
            return Ok(rooms);
        }

        [HttpPost("Room/Search")]
        public async Task<ActionResult<RoomsModel>> SearchByRoomName([FromQuery] string name)
        {
            var rooms = await _roomsService.SearchByNameRoom(name);
            return Ok(rooms);
        }

        [HttpPost("Room/Create")]
        public async Task<IActionResult> CreateRoom([FromBody] RoomDTO roomDTO)
        {
            await _roomsService.InsertRoom(roomDTO);
            return Ok(new { message = "Room created sucessfully" });
        }

        [HttpPost("Room/Update")]
        public async Task<IActionResult> UpdateRoom([FromBody] RoomDTO roomDTO)
        {
            await _roomsService.UpdateRoom(roomDTO);
            return Ok(new { message = "Room updated successfully" });
        }

        [HttpPost("Room/Delete")]
        public async Task<IActionResult> DeleteRoom([FromBody] DeleteRoomDto roomDeleteDTO)
        {
            await _roomsService.DeleteRoom(roomDeleteDTO);
            return Ok(new { message = "Room delete/change status sucessfully" });
        }

        [HttpPost("Room/Seed")]
        public async Task<IActionResult> SeedData()
        {
            await _roomsService.SeedSampleData();
            return Ok("Insert Room Data successfully");
        }
    }
}
