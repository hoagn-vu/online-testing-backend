#pragma warning disable SA1309
namespace Backend_online_testing.Controllers
{
    using Backend_online_testing.Models;
    using Backend_online_testing.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using MongoDB.Bson;

    [Route("api/room")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly RoomService _roomService;

        // Hiện chưa có tài khoản nên dùng Id sau để test tạm
        private ObjectId tempId = ObjectId.Parse("67b75fc3e54f92629f3d7378");

        public RoomController(RoomService roomService)
        {
            this._roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => this.Ok(await this._roomService.GetRoomsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(ObjectId id)
        {
            var product = await this._roomService.GetByIdAsync(id);
            return product == null ? this.NotFound() : this.Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoomModel room)
        {
            await this._roomService.CreateAsync(room, this.tempId);
            return this.CreatedAtAction(nameof(this.Get), new { id = room.Id }, room);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(ObjectId id, RoomModel room)
        {
            var existingProduct = await this._roomService.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return this.NotFound();
            }

            room.Id = id;
            await this._roomService.UpdateAsync(id, room, this.tempId);
            return this.NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(ObjectId id)
        {
            var product = await this._roomService.GetByIdAsync(id);
            if (product == null)
            {
                return this.NotFound();
            }

            await this._roomService.SoftDeleteAsync(id, this.tempId);
            return this.NoContent();
        }

        [HttpDelete("del/{id}")]
        public async Task<IActionResult> Delete(ObjectId id)
        {
            var product = await this._roomService.GetByIdAsync(id);
            if (product == null)
            {
                return this.NotFound();
            }

            await this._roomService.DeleteAsync(id, this.tempId);
            return this.NoContent();
        }
    }
}
