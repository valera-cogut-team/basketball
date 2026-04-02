using System;

namespace Logger.Domain
{
    public struct LogEntryData
    {
        public string Message { get; set; }
        public string Source { get; set; }
        public Exception Exception { get; set; }
        public object Context { get; set; }
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }

        public static LogEntryData Create(LogLevel level, string message, Exception exception = null, object context = null, string source = null) =>
            new LogEntryData
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message ?? string.Empty,
                Exception = exception,
                Context = context,
                Source = source
            };
    }
}
