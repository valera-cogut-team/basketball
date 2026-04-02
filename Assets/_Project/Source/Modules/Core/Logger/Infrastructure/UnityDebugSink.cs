using Logger.Domain;
#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
#endif

namespace Logger.Infrastructure
{
    public class UnityDebugSink : ILogSink
    {
        public void Write(LogEntryData entry)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            var msg = $"[{entry.Timestamp:HH:mm:ss.fff}] [{entry.Level}] [{(string.IsNullOrEmpty(entry.Source) ? "Logger" : entry.Source)}] {entry.Message}";
            switch (entry.Level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    Debug.Log(msg);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(msg);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    if (entry.Exception != null) Debug.LogError($"{msg}\n{entry.Exception}");
                    else Debug.LogError(msg);
                    break;
            }
#endif
        }

        public void Flush() { }
    }
}
