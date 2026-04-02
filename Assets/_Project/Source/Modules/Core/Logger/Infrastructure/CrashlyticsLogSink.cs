using System;
using Logger.Domain;
using UnityEngine;

#if FIREBASE_CRASHLYTICS
using Firebase.Crashlytics;
#endif

namespace Logger.Infrastructure
{
    /// <summary>
    /// Log sink that sends logs to Firebase Crashlytics (or similar crash reporting service).
    /// </summary>
    public class CrashlyticsLogSink : ILogSink
    {
        public void Write(LogEntryData entry)
        {
#if FIREBASE_CRASHLYTICS
            try
            {
                if (entry.Level >= LogLevel.Error)
                {
                    if (entry.Exception != null)
                    {
                        Crashlytics.LogException(entry.Exception);
                    }
                    else
                    {
                        Crashlytics.Log($"[{entry.Level}] {entry.Message}");
                    }
                }
                else if (entry.Level == LogLevel.Warning)
                {
                    Crashlytics.Log($"WARNING: {entry.Message}");
                }
                // Info and Debug logs are not sent to Crashlytics to reduce noise
            }
            catch (Exception ex)
            {
                // Don't log errors from Crashlytics itself to avoid loops
                Debug.LogError($"[Crashlytics] Failed to log: {ex.Message}");
            }
#else
            // No-op when Firebase Crashlytics is not available
#endif
        }

        public void Flush()
        {
            // Crashlytics doesn't require explicit flushing
        }
    }
}
