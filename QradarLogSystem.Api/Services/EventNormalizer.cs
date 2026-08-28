using QradarLogSystem.Api.Models;

namespace QradarLogSystem.Api.Services
{
    public class EventNormalizer
    {
        public NormalizedEvent Normalize(QradarEvent qradarEvent)
        {
            return new NormalizedEvent
            {
                Qid = qradarEvent.Qid,
                EventName = qradarEvent.EventName,
                SourceIp = qradarEvent.SourceIp,
                DestinationIp = qradarEvent.DestinationIp,
                SourcePort = qradarEvent.SourcePort,
                DestinationPort = qradarEvent.DestinationPort,
                Username = qradarEvent.Username,

                Severity = qradarEvent.Severity,
                SeverityLevel = GetSeverityLevel(qradarEvent.Severity),

                Magnitude = qradarEvent.Magnitude,

                LogSourceId = qradarEvent.LogSourceId,
                LogSourceName = qradarEvent.LogSourceName,

                Payload = qradarEvent.Payload,

                ParseStatus = "SUCCESS"
            };
        }

        private string GetSeverityLevel(int severity)
        {
            if (severity >= 1 && severity <= 3)
                return "LOW";

            if (severity >= 4 && severity <= 6)
                return "MEDIUM";

            if (severity >= 7 && severity <= 8)
                return "HIGH";

            if (severity >= 9 && severity <= 10)
                return "CRITICAL";

            return "UNKNOWN";
        }
    }
}