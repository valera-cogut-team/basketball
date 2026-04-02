using Timer.Domain;

namespace Timer.Facade
{
    public interface ITimerFacade : ITimerActions
    {
        void Enable();
        void Disable();
    }
}
