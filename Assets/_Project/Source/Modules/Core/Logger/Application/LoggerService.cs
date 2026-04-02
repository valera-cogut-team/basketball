using System;
using System.Collections.Generic;
using System.Diagnostics;
using Logger.Domain;

namespace Logger.Application
{
    public class LoggerService : ILoggerService
    {
        private readonly List<ILogSink> _sinks = new List<ILogSink>();
        private readonly object _lock = new object();
        private LogLevel _minLevel = LogLevel.Debug;

        public void AddSink(ILogSink sink)
        {
            if (sink == null) throw new ArgumentNullException(nameof(sink));
            lock (_lock) _sinks.Add(sink);
        }

        public void RemoveSink(ILogSink sink) { lock (_lock) _sinks.Remove(sink); }
        public void SetMinLevel(LogLevel level) => _minLevel = level;

        public void Log(LogEntryData entry)
        {
            if (entry.Level < _minLevel) return;
            lock (_lock)
            {
                foreach (var s in _sinks)
                {
                    try { s.Write(entry); }
                    catch (Exception ex) { Debug.WriteLine($"LoggerService sink write failed: {ex}"); }
                }
            }
        }

        public void Flush()
        {
            lock (_lock)
            {
                foreach (var s in _sinks)
                {
                    try { s.Flush(); }
                    catch (Exception ex) { Debug.WriteLine($"LoggerService sink flush failed: {ex}"); }
                }
            }
        }
    }
}
