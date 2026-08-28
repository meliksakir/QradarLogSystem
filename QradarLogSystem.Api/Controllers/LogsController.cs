using Microsoft.AspNetCore.Mvc;
using QradarLogSystem.Api.Services;

namespace QradarLogSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly LogReaderService _logReaderService;

        public LogsController(
            LogReaderService logReaderService)
        {
            _logReaderService = logReaderService;
        }

        [HttpGet]
        public IActionResult GetLogs()
        {
            var logs = _logReaderService.GetLogs();

            return Ok(new
            {
                totalCount = logs.Count,

                infoCount = logs.Count(
                    log => log.Level == "INFO"),

                errorCount = logs.Count(
                    log => log.Level == "ERROR"),

                logs
            });
        }
    }
}