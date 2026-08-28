using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QradarLogSystem.Api.Data;

namespace QradarLogSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly QradarDbContext _dbContext;

        public SystemController(QradarDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var databaseConnected =
                    await _dbContext.Database.CanConnectAsync();

                var totalEvents =
                    await _dbContext.Events.CountAsync();

                var successfulEvents =
                    await _dbContext.Events
                        .CountAsync(e => e.ParseStatus == "SUCCESS");

                var failedEvents =
                    await _dbContext.Events
                        .CountAsync(e => e.ParseStatus != "SUCCESS");

                var lastEvent =
                    await _dbContext.Events
                        .OrderByDescending(e => e.CreatedAt)
                        .FirstOrDefaultAsync();

                return Ok(new
                {
                    status = "running",
                    apiStatus = "healthy",
                    databaseConnected,
                    databaseStatus = databaseConnected
                        ? "connected"
                        : "disconnected",

                    totalEvents,
                    successfulEvents,
                    failedEvents,

                    lastEventAt = lastEvent != null
                        ? lastEvent.CreatedAt
                        : (DateTime?)null,

                    message =
                        "QRadar Log System API and database are operational."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    apiStatus = "running",
                    databaseConnected = false,
                    databaseStatus = "error",
                    message =
                        "Database health check could not be completed.",
                    error = ex.Message
                });
            }
        }
    }
}