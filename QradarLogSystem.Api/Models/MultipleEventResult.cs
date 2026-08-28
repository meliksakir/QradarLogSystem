namespace QradarLogSystem.Api.Models
{
    public class MultipleEventResult
    {
        public int TotalCount { get; set; }

        public int SuccessCount { get; set; }

        public int FailedCount { get; set; }

        public double TotalProcessingTimeMs { get; set; }

        public List<NormalizedEvent> SuccessfulEvents { get; set; } = new();

        public List<string> FailedEvents { get; set; } = new();
    }
}
