namespace QradarLogSystem.Api.Data.Entities
{
    public class EventEntity
    {
        public int Id { get; set; }

        public long Qid { get; set; }

        public string? EventName { get; set; }

        public string? SourceIp { get; set; }

        public string? DestinationIp { get; set; }

        public int? SourcePort { get; set; }

        public int? DestinationPort { get; set; }

        public string? Username { get; set; }

        public int Severity { get; set; }

        public string? SeverityLevel { get; set; }

        public int? Magnitude { get; set; }

        public int? LogSourceId { get; set; }

        public string? LogSourceName { get; set; }

        public string? Payload { get; set; }

        public string ParseStatus { get; set; } = string.Empty;

        public double ProcessingTimeMs { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}