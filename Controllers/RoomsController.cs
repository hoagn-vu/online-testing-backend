using Backend_online_testing.Dtos;
using Backend_online_testing.Models;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers
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

        // Get all room
        // [Authorize]
        // [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<RoomsModel>> GetAllRoom([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (rooms, total) = await _roomsService.GetRooms(keyword, page, pageSize);
            return this.Ok(new { rooms, total });
        }    
        
        [HttpGet("get-options")]
        public async Task<IActionResult> GetRoomOptions()
        {
            var rooms = await _roomsService.GetRoomOptionsAsync();
            return Ok(rooms);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] RoomDto dto)
        {
            var result = await _roomsService.CreateRoomAsync(dto);
            return result == "Success"
                ? Ok(new { message = "Created" })
                : BadRequest(new { message = result });
        }

        [HttpPut("{roomId}")]
        public async Task<IActionResult> UpdateRoom([FromBody] RoomDto dto, string roomId)
        {
            var result = await _roomsService.UpdateRoomAsync(dto, roomId);
            return result == "Success"
                ? Ok(new { message = "Updated" })
                : BadRequest(new { error = result });
        }

        [HttpDelete("delete-room/{roomId}")]
        public async Task<IActionResult> DeleteRoom(string roomId)
        {
            var result = await _roomsService.DeleteRoomAsync(roomId);
            return result == "Success"
                ? Ok(new { message = "Deleted" })
                : BadRequest(new { error = result });
        }
        
        [HttpGet("{roomId}/schedules")]
        public async Task<IActionResult> GetRoomSchedules(string roomId, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var request = new GetRoomSchedulesRequestDto { RoomId = roomId, Start = start, End = end };

            var result = await _roomsService.GetRoomSchedulesAsync(request);
            if (result == null)
            {
                return NotFound(new { message = "Room not found" });
            }

            return Ok(result);
        }
    }
}
