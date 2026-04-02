using LifeCycle.Facade;

namespace LifeCycle.Application
{
    public interface ILifeCycleService
    {
        void RegisterUpdateHandler(IUpdateHandler handler);
        void UnregisterUpdateHandler(IUpdateHandler handler);
        void RegisterLateUpdateHandler(ILateUpdateHandler handler);
        void UnregisterLateUpdateHandler(ILateUpdateHandler handler);
        void RegisterFixedUpdateHandler(IFixedUpdateHandler handler);
        void UnregisterFixedUpdateHandler(IFixedUpdateHandler handler);
        void OnUpdate(float deltaTime);
        void OnLateUpdate(float deltaTime);
        void OnFixedUpdate(float fixedDeltaTime);
        void Clear();
    }
}
