using Backend_online_testing.Models;
using Backend_online_testing.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class LogController : ControllerBase
    {
        private readonly LogService _logService;

        public LogController(LogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _logService.GetLogsAsync();
            return Ok(logs);
        }

        [HttpPost]
        public async Task<IActionResult> AddLog([FromBody] LogsModel log)
        {
            await _logService.AddLogAsync(log.MadeBy, log.LogAction, log.LogDetails);
            return Ok(new { message = "Log created successfully" });
        }
    }
}
