using Logger.Domain;

namespace Logger.Application
{
    public interface ILoggerService
    {
        void AddSink(ILogSink sink);
        void RemoveSink(ILogSink sink);
        void SetMinLevel(LogLevel level);
        void Log(LogEntryData entry);
        void Flush();
    }
}
