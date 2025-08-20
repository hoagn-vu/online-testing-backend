using Microsoft.AspNetCore.Mvc;

namespace Backend_online_testing.Controllers
{
    [Route("api/vuxs-test")]
    [ApiController]
    public class VuxsTestController : ControllerBase
    {
        // GET: api/test/echo?query=abc
        [HttpGet("echo")]
        public IActionResult EchoGet([FromQuery] string? query)
        {
            var response = new
            {
                method = "GET",
                query
            };

            return Ok(response);
        }

        // POST: api/test/echo?query=abc
        // Body: { "msg": "hello" }
        [HttpPost("echo")]
        public IActionResult EchoPost([FromQuery] string? query, [FromBody] object? body)
        {
            var response = new
            {
                method = "POST",
                query,
                body
            };

            return Ok(response);
        }

        // PUT: api/test/echo?query=abc
        // Body: { "msg": "update" }
        [HttpPut("echo")]
        public IActionResult EchoPut([FromQuery] string? query, [FromBody] object? body)
        {
            var response = new
            {
                method = "PUT",
                query,
                body
            };

            return Ok(response);
        }
    }
}