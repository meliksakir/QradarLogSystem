using QradarLogSystem.Api.Models;

namespace QradarLogSystem.Api.Services
{
    public class LogReaderService
    {
        private readonly string _logFilePath;

        public LogReaderService()
        {
            _logFilePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Logs",
                "application.txt");
        }

        public List<LogEntry> GetLogs()
        {
            var logs = new List<LogEntry>();

            if (!File.Exists(_logFilePath))
                return logs;

            var lines = File.ReadAllLines(_logFilePath);

            var id = 1;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var logEntry = ParseLogLine(line, id);

                logs.Add(logEntry);

                id++;
            }

            return logs;
        }

        private LogEntry ParseLogLine(string line, int id)
        {
            var time = string.Empty;
            var level = "INFO";
            var message = line;

            if (line.StartsWith("["))
            {
                var closingBracketIndex = line.IndexOf(']');

                if (closingBracketIndex > 0)
                {
                    time = line.Substring(
                        1,
                        closingBracketIndex - 1);

                    var remaining = line
                        .Substring(closingBracketIndex + 1)
                        .Trim();

                    if (remaining.StartsWith(
                        "ERROR ",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        level = "ERROR";

                        message = remaining
                            .Substring("ERROR ".Length)
                            .Trim();
                    }
                    else if (remaining.StartsWith(
                        "INFO ",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        level = "INFO";

                        message = remaining
                            .Substring("INFO ".Length)
                            .Trim();
                    }
                    else
                    {
                        message = remaining;
                    }
                }
            }

            return new LogEntry
            {
                Id = id,
                Time = time,
                Level = level,
                Message = message
            };
        }
    }
}