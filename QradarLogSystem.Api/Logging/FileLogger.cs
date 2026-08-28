namespace QradarLogSystem.Api.Logging
{
    public class FileLogger : IFileLogger
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new();

        public FileLogger()
        {
            var logDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Logs");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFilePath = Path.Combine(
                logDirectory,
                "application.txt");
        }

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        private void WriteLog(string level, string message)
        {
            var logLine =
                $"[{DateTime.Now:HH:mm:ss}] {level} {message}";

            lock (_lockObject)
            {
                File.AppendAllText(
                    _logFilePath,
                    logLine + Environment.NewLine);
            }
        }
    }
}