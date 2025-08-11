using Backend_online_testing.Dtos;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers;

[ApiController]
[Route("api/level")]
public class LevelController : ControllerBase
{
    private readonly LevelService _levelService;

    public LevelController(LevelService levelService)
    {
        _levelService = levelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(string? keyword, int page, int pageSize)
    {
        var levels = await _levelService.GetAllLevelAsync(keyword, page, pageSize);
        return Ok(levels);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var level = await _levelService.GetLevelByIdAsync(id);
        if (level == null)
            return NotFound();
        return Ok(level);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LevelCreateDto dto)
    {
        var created = await _levelService.CreateLevelAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var success = await _levelService.DeleteLevelAsync(id);
        if (!success)
            return NotFound();
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateName(string id, [FromBody] LevelCreateDto dto)
    {
        var success = await _levelService.UpdateLevelNameAsync(id, dto.LevelName ?? "");
        if (!success)
            return NotFound();
        return NoContent();
    }
}
