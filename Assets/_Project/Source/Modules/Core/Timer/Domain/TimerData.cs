using System;

namespace Timer.Domain
{
    public struct TimerData
    {
        public Action OnComplete { get; set; }
        public int Id { get; set; }
        public float Duration { get; set; }
        public float Elapsed { get; set; }
        private byte _flags;
        public bool IsPaused { get => (_flags & 1) != 0; set => _flags = (byte)(value ? _flags | 1 : _flags & ~1); }
        public bool IsCompleted { get => (_flags & 2) != 0; set => _flags = (byte)(value ? _flags | 2 : _flags & ~2); }
        public float Progress => Duration > 0f ? (Elapsed >= Duration ? 1f : (Elapsed <= 0f ? 0f : Elapsed / Duration)) : 0f;
        public float Remaining => Duration > Elapsed ? (Duration - Elapsed) : 0f;
    }
}
