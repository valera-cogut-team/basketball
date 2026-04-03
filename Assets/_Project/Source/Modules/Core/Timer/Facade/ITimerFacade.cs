using System;
using Timer.Domain;

namespace Timer.Facade
{
    public interface ITimerFacade
    {
        TimerHandle CreateTimer(float duration, Action onComplete = null);
        void PauseTimer(TimerHandle handle);
        void ResumeTimer(TimerHandle handle);
        void CancelTimer(TimerHandle handle);
        TimerData? GetTimer(TimerHandle handle);

        void Enable();
        void Disable();
    }
}
