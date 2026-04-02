using LifeCycle.Application;

namespace LifeCycle.Facade
{
    public class LifeCycleFacade : ILifeCycleFacade
    {
        private readonly ILifeCycleService _service;
        public LifeCycleFacade(ILifeCycleService service) { _service = service ?? throw new System.ArgumentNullException(nameof(service)); }
        public void RegisterUpdateHandler(IUpdateHandler handler) => _service.RegisterUpdateHandler(handler);
        public void UnregisterUpdateHandler(IUpdateHandler handler) => _service.UnregisterUpdateHandler(handler);
        public void RegisterLateUpdateHandler(ILateUpdateHandler handler) => _service.RegisterLateUpdateHandler(handler);
        public void UnregisterLateUpdateHandler(ILateUpdateHandler handler) => _service.UnregisterLateUpdateHandler(handler);
        public void RegisterFixedUpdateHandler(IFixedUpdateHandler handler) => _service.RegisterFixedUpdateHandler(handler);
        public void UnregisterFixedUpdateHandler(IFixedUpdateHandler handler) => _service.UnregisterFixedUpdateHandler(handler);

        public void TickUpdate(float deltaTime) => _service.OnUpdate(deltaTime);
        public void TickLateUpdate(float deltaTime) => _service.OnLateUpdate(deltaTime);
        public void TickFixedUpdate(float fixedDeltaTime) => _service.OnFixedUpdate(fixedDeltaTime);
    }
}
