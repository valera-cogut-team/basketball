using System;

namespace Logger.Facade
{
    public interface ILoggerActions
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception exception = null);
        void LogDebug(string message);
        void LogCritical(string message, Exception exception = null);
    }
}
