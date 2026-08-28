namespace QradarLogSystem.Api.Logging
{
    public interface IFileLogger
    {
        void LogInfo(string message);

        void LogError(string message);
    }
}