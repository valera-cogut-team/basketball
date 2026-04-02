using LifeCycle.Facade;
using Timer.Application;

namespace Timer.Application
{
    public class TimerUpdateHandler : IUpdateHandler
    {
        private readonly ITimerService _service;
        public TimerUpdateHandler(ITimerService service) { _service = service; }
        public void OnUpdate(float deltaTime) => _service.UpdateTimers(deltaTime);
    }
}
