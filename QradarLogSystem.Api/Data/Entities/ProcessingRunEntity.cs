namespace QradarLogSystem.Api.Data.Entities
{
    public class ProcessingRunEntity
    {
        public int Id { get; set; }

        public int TotalCount { get; set; }

        public int SuccessCount { get; set; }

        public int FailedCount { get; set; }

        public double TotalProcessingTimeMs { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}