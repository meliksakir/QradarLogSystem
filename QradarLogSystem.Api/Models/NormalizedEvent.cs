namespace QradarLogSystem.Api.Models
{
    public class NormalizedEvent
    {
        public long Qid { get; set; }

        public string EventName { get; set; } = string.Empty;

        public string SourceIp { get; set; } = string.Empty;

        public string DestinationIp { get; set; } = string.Empty;

        public int SourcePort { get; set; }

        public int DestinationPort { get; set; }

        public string Username { get; set; } = string.Empty;

        public int Severity { get; set; }

        public string SeverityLevel { get; set; } = string.Empty;

        public int Magnitude { get; set; }

        public int LogSourceId { get; set; }

        public string LogSourceName { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public string ParseStatus { get; set; } = string.Empty;

        public double ProcessingTimeMs { get; set; }
    }
}