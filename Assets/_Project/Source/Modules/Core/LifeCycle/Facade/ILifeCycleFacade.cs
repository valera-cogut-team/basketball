namespace LifeCycle.Facade
{
    public interface ILifeCycleFacade
    {
        void RegisterUpdateHandler(IUpdateHandler handler);
        void UnregisterUpdateHandler(IUpdateHandler handler);
        void RegisterLateUpdateHandler(ILateUpdateHandler handler);
        void UnregisterLateUpdateHandler(ILateUpdateHandler handler);
        void RegisterFixedUpdateHandler(IFixedUpdateHandler handler);
        void UnregisterFixedUpdateHandler(IFixedUpdateHandler handler);

        /// <summary>Called by LifeCycle driver (Unity MonoBehaviour) each frame.</summary>
        void TickUpdate(float deltaTime);

        /// <summary>Called by LifeCycle driver (Unity MonoBehaviour) each frame (LateUpdate).</summary>
        void TickLateUpdate(float deltaTime);

        /// <summary>Called by LifeCycle driver (Unity MonoBehaviour) each physics step.</summary>
        void TickFixedUpdate(float fixedDeltaTime);
    }
}
