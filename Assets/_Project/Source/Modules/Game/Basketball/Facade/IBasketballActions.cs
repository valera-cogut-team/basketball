using Basketball.Domain;

namespace Basketball.Facade
{
    public interface IBasketballActions
    {
        /// <summary>Increments score if rules allow. Returns whether a new point was registered.</summary>
        bool NotifyBasketMade();
        void SetPhase(BasketballBallPhase phase);
        void SetAimCharge(float charge01);
        void ResetRun();
    }
}
