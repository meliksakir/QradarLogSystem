namespace QradarLogSystem.Api.Models
{
    public class DatasetImportResult
    {
        public string FileName { get; set; } = string.Empty;

        public string DetectedFormat { get; set; } = string.Empty;

        public int TotalRecords { get; set; }

        public int SuccessCount { get; set; }

        public int FailedCount { get; set; }

        public double TotalProcessingTimeMs { get; set; }

        public List<NormalizedEvent> SuccessfulEvents { get; set; } = new();

        public List<string> FailedEvents { get; set; } = new();
    }
}