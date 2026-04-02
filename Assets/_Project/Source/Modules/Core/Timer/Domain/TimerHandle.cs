using System;

namespace Timer.Domain
{
    public readonly struct TimerHandle : IEquatable<TimerHandle>
    {
        public static TimerHandle Invalid => default;
        public readonly int Id;
        public TimerHandle(int id) => Id = id;
        public bool IsValid => Id != 0;
        public bool Equals(TimerHandle other) => Id == other.Id;
        public override bool Equals(object obj) => obj is TimerHandle o && Equals(o);
        public override int GetHashCode() => Id;
        public static bool operator ==(TimerHandle a, TimerHandle b) => a.Id == b.Id;
        public static bool operator !=(TimerHandle a, TimerHandle b) => a.Id != b.Id;
    }
}
