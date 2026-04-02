using System;
using Timer.Domain;

namespace Timer.Application
{
    public interface ITimerService
    {
        TimerHandle CreateTimer(float duration, Action onComplete = null);
        void UpdateTimers(float deltaTime);
        void PauseTimer(TimerHandle handle);
        void ResumeTimer(TimerHandle handle);
        void CancelTimer(TimerHandle handle);
        TimerData? GetTimer(TimerHandle handle);
        void Clear();
    }
}
