namespace QradarLogSystem.Api.Models
{
    public class LogEntry
    {
        public int Id { get; set; }

        public string Time { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}