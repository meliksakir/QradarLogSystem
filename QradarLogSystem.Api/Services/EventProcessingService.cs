using System.Diagnostics;
using QradarLogSystem.Api.Data;
using QradarLogSystem.Api.Data.Entities;
using QradarLogSystem.Api.Logging;
using QradarLogSystem.Api.Models;

namespace QradarLogSystem.Api.Services
{
    public class EventProcessingService
    {
        private readonly EventParser _eventParser;
        private readonly EventNormalizer _eventNormalizer;
        private readonly IFileLogger _fileLogger;
        private readonly QradarDbContext _dbContext;

        public EventProcessingService(
            EventParser eventParser,
            EventNormalizer eventNormalizer,
            IFileLogger fileLogger,
            QradarDbContext dbContext)
        {
            _eventParser = eventParser;
            _eventNormalizer = eventNormalizer;
            _fileLogger = fileLogger;
            _dbContext = dbContext;
        }

        // Tek bir eventi işler
        public NormalizedEvent Process(string rawEvent)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var parsedEvent =
                    _eventParser.Parse(rawEvent);

                var normalizedEvent =
                    _eventNormalizer.Normalize(parsedEvent);

                stopwatch.Stop();

                normalizedEvent.ProcessingTimeMs =
                    stopwatch.Elapsed.TotalMilliseconds;

                _fileLogger.LogInfo(
                    $"Event processed successfully. " +
                    $"QID={normalizedEvent.Qid} " +
                    $"Severity={normalizedEvent.SeverityLevel} " +
                    $"ProcessingTimeMs={normalizedEvent.ProcessingTimeMs:F4}");

                var eventEntity = new EventEntity
                {
                    Qid = normalizedEvent.Qid,
                    EventName = normalizedEvent.EventName,
                    SourceIp = normalizedEvent.SourceIp,
                    DestinationIp = normalizedEvent.DestinationIp,
                    SourcePort = normalizedEvent.SourcePort,
                    DestinationPort = normalizedEvent.DestinationPort,
                    Username = normalizedEvent.Username,
                    Severity = normalizedEvent.Severity,
                    SeverityLevel = normalizedEvent.SeverityLevel,
                    Magnitude = normalizedEvent.Magnitude,
                    LogSourceId = normalizedEvent.LogSourceId,
                    LogSourceName = normalizedEvent.LogSourceName,
                    Payload = normalizedEvent.Payload,
                    ParseStatus = normalizedEvent.ParseStatus,
                    ProcessingTimeMs = normalizedEvent.ProcessingTimeMs,
                    CreatedAt = DateTime.Now
                };

                _dbContext.Events.Add(eventEntity);
                _dbContext.SaveChanges();

                return normalizedEvent;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _fileLogger.LogError(
                    $"Event processing failed. Error={ex.Message}");

                throw;
            }
        }

        // Birden fazla eventi işler
        public MultipleEventResult ProcessMultiple(List<string> rawEvents)
        {
            var totalStopwatch = Stopwatch.StartNew();

            var result = new MultipleEventResult
            {
                TotalCount = rawEvents.Count
            };

            foreach (var rawEvent in rawEvents)
            {
                try
                {
                    var normalizedEvent = Process(rawEvent);

                    result.SuccessfulEvents.Add(normalizedEvent);
                    result.SuccessCount++;
                }
                catch
                {
                    result.FailedEvents.Add(rawEvent);
                    result.FailedCount++;
                }
            }

            totalStopwatch.Stop();

            result.TotalProcessingTimeMs =
                totalStopwatch.Elapsed.TotalMilliseconds;

            _fileLogger.LogInfo(
                $"Multiple event processing completed. " +
                $"Total={result.TotalCount} " +
                $"Success={result.SuccessCount} " +
                $"Failed={result.FailedCount} " +
                $"TotalProcessingTimeMs={result.TotalProcessingTimeMs:F4}");

            var processingRunEntity = new ProcessingRunEntity
            {
                TotalCount = result.TotalCount,
                SuccessCount = result.SuccessCount,
                FailedCount = result.FailedCount,
                TotalProcessingTimeMs = result.TotalProcessingTimeMs,
                CreatedAt = DateTime.Now
            };

            _dbContext.ProcessingRuns.Add(processingRunEntity);
            _dbContext.SaveChanges();

            return result;
        }
    }
}