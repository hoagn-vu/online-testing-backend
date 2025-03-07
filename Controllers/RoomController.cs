using Backend_online_testing.Models;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Backend_online_testing.Controllers
{
    [Route("api/room")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly RoomService _roomService;

        // Hiện chưa có tài khoản nên dùng Id sau để test tạm
        public ObjectId tempId = ObjectId.Parse("67b75fc3e54f92629f3d7378");

        public RoomController(RoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _roomService.GetRoomsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(ObjectId id)
        {
            var product = await _roomService.GetByIdAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoomModel room)
        {
            await _roomService.CreateAsync(room, tempId);
            return CreatedAtAction(nameof(Get), new { id = room.Id }, room);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(ObjectId id, RoomModel room)
        {
            var existingProduct = await _roomService.GetByIdAsync(id);
            if (existingProduct == null) return NotFound();
            room.Id = id;
            await _roomService.UpdateAsync(id, room, tempId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(ObjectId id)
        {
            var product = await _roomService.GetByIdAsync(id);
            if (product == null) return NotFound();
            await _roomService.SoftDeleteAsync(id, tempId);
            return NoContent();
        }

        [HttpDelete("del/{id}")]
        public async Task<IActionResult> Delete(ObjectId id)
        {
            var product = await _roomService.GetByIdAsync(id);
            if (product == null) return NotFound();
            await _roomService.DeleteAsync(id, tempId);
            return NoContent();
        }
    }
}
