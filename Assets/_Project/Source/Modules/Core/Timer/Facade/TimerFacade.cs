using System;
using Core;
using LifeCycle.Facade;
using Timer.Application;
using Timer.Domain;

namespace Timer.Facade
{
    public class TimerFacade : ITimerFacade
    {
        private readonly ITimerService _service;
        private readonly ILifeCycleFacade _lifeCycle;
        private TimerUpdateHandler _handler;
        private bool _enabled;

        public TimerFacade(ITimerService service, IModuleContext context)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _lifeCycle = context.GetModuleFacade<ILifeCycleFacade>();
        }

        public TimerHandle CreateTimer(float duration, Action onComplete = null) => _service.CreateTimer(duration, onComplete);
        public void PauseTimer(TimerHandle h) => _service.PauseTimer(h);
        public void ResumeTimer(TimerHandle h) => _service.ResumeTimer(h);
        public void CancelTimer(TimerHandle h) => _service.CancelTimer(h);
        public TimerData? GetTimer(TimerHandle h) => _service.GetTimer(h);

        public void Enable()
        {
            if (_enabled) return;
            _enabled = true;
            _handler = new TimerUpdateHandler(_service);
            _lifeCycle?.RegisterUpdateHandler(_handler);
        }

        public void Disable()
        {
            if (!_enabled) return;
            _enabled = false;
            if (_handler != null) { _lifeCycle?.UnregisterUpdateHandler(_handler); _handler = null; }
        }
    }
}
