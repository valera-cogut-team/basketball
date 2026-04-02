using System;
using Logger.Application;
using Logger.Domain;

namespace Logger.Facade
{
    public sealed class LoggerFacade : ILoggerFacade
    {
        private readonly ILoggerService _service;
        private readonly string _source;

        public LoggerFacade(ILoggerService service, string source = null)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _source = source;
        }

        public void LogInfo(string message) => _service.Log(LogEntryData.Create(LogLevel.Info, message, source: _source));
        public void LogWarning(string message) => _service.Log(LogEntryData.Create(LogLevel.Warning, message, source: _source));
        public void LogError(string message, Exception exception = null) => _service.Log(LogEntryData.Create(LogLevel.Error, message, exception, source: _source));
        public void LogDebug(string message) => _service.Log(LogEntryData.Create(LogLevel.Debug, message, source: _source));
        public void LogCritical(string message, Exception exception = null) => _service.Log(LogEntryData.Create(LogLevel.Critical, message, exception, source: _source));
    }
}
