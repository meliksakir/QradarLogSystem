using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QradarLogSystem.Api.Data;
using QradarLogSystem.Api.Models;
using QradarLogSystem.Api.Services;

namespace QradarLogSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventProcessingService _eventProcessingService;
        private readonly QradarDbContext _dbContext;

        public EventsController(
            EventProcessingService eventProcessingService,
            QradarDbContext dbContext)
        {
            _eventProcessingService = eventProcessingService;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(
            [FromQuery] string? severity = null,
            [FromQuery] string? sourceIp = null,
            [FromQuery] string? eventName = null,
            [FromQuery] string? logSourceName = null)
        {
            var query = _dbContext.Events.AsQueryable();

            if (!string.IsNullOrWhiteSpace(severity))
            {
                var normalizedSeverity = severity.Trim().ToUpper();

                query = query.Where(e =>
                    e.SeverityLevel == normalizedSeverity);
            }

            if (!string.IsNullOrWhiteSpace(sourceIp))
            {
                query = query.Where(e =>
                    e.SourceIp != null &&
                    e.SourceIp.Contains(sourceIp));
            }

            if (!string.IsNullOrWhiteSpace(eventName))
            {
                query = query.Where(e =>
                    e.EventName != null &&
                    e.EventName.Contains(eventName));
            }

            if (!string.IsNullOrWhiteSpace(logSourceName))
            {
                query = query.Where(e =>
                    e.LogSourceName != null &&
                    e.LogSourceName.Contains(logSourceName));
            }

            var events = await query
                .OrderByDescending(e => e.Id)
                .ToListAsync();

            return Ok(new
            {
                totalCount = events.Count,
                filters = new
                {
                    severity,
                    sourceIp,
                    eventName,
                    logSourceName
                },
                events
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var eventRecord = await _dbContext.Events
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventRecord == null)
            {
                return NotFound(new
                {
                    message = $"Event with Id {id} was not found."
                });
            }

            return Ok(eventRecord);
        }

        [HttpPost]
        public IActionResult ReceiveEvent([FromBody] QradarEvent qradarEvent)
        {
            return Ok(new
            {
                message = "QRadar event received successfully.",
                receivedEvent = qradarEvent
            });
        }

        [HttpPost("parse")]
        public IActionResult ParseEvent([FromBody] RawEventRequest request)
        {
            try
            {
                var normalizedEvent =
                    _eventProcessingService.Process(request.RawEvent);

                return Ok(new
                {
                    message = "QRadar event parsed and normalized successfully.",
                    eventData = normalizedEvent
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "QRadar event could not be parsed.",
                    parseStatus = "FAILED",
                    error = ex.Message
                });
            }
        }

        [HttpPost("parse-multiple")]
        public IActionResult ParseMultipleEvents(
            [FromBody] MultipleEventRequest request)
        {
            var result =
                _eventProcessingService.ProcessMultiple(request.RawEvents);

            return Ok(new
            {
                message = "Multiple QRadar events processed.",
                result
            });
        }
    }
}