using System;
using System.Collections.Generic;
using Timer.Domain;
using Logger.Facade;

namespace Timer.Application
{
    public class TimerService : ITimerService
    {
        private readonly Dictionary<int, TimerData> _timers = new Dictionary<int, TimerData>();
        private readonly object _lock = new object();
        private readonly ILoggerFacade _logger;
        private readonly List<TimerData> _toUpdate = new List<TimerData>(64);
        private readonly List<TimerData> _completed = new List<TimerData>(16);
        private int _nextId = 1;

        public TimerService(ILoggerFacade logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

        public TimerHandle CreateTimer(float duration, Action onComplete = null)
        {
            var id = _nextId++; if (id == 0) id = _nextId++;
            var t = new TimerData { Id = id, Duration = duration, Elapsed = 0f, IsPaused = false, IsCompleted = false, OnComplete = onComplete };
            lock (_lock) _timers[id] = t;
            return new TimerHandle(id);
        }

        public void UpdateTimers(float deltaTime)
        {
            lock (_lock) { _toUpdate.Clear(); foreach (var t in _timers.Values) if (!t.IsPaused && !t.IsCompleted) _toUpdate.Add(t); }
            _completed.Clear();
            foreach (var t in _toUpdate)
            {
                var u = t; u.Elapsed += deltaTime;
                if (u.Elapsed >= u.Duration) { u.Elapsed = u.Duration; u.IsCompleted = true; _completed.Add(u); }
                lock (_lock) _timers[t.Id] = u;
            }
            foreach (var t in _completed)
            {
                try { t.OnComplete?.Invoke(); } catch (Exception ex) { _logger.LogError($"Timer callback: {ex.Message}", ex); }
                lock (_lock) _timers.Remove(t.Id);
            }
        }

        public void PauseTimer(TimerHandle h) { lock (_lock) { if (_timers.TryGetValue(h.Id, out var t)) { t.IsPaused = true; _timers[h.Id] = t; } } }
        public void ResumeTimer(TimerHandle h) { lock (_lock) { if (_timers.TryGetValue(h.Id, out var t)) { t.IsPaused = false; _timers[h.Id] = t; } } }
        public void CancelTimer(TimerHandle h) { lock (_lock) _timers.Remove(h.Id); }
        public TimerData? GetTimer(TimerHandle h) { lock (_lock) return _timers.TryGetValue(h.Id, out var t) ? t : (TimerData?)null; }
        public void Clear() { lock (_lock) _timers.Clear(); }
    }
}
