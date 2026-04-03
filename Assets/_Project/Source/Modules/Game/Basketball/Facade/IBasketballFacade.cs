using Basketball.Domain;
using UniRx;

namespace Basketball.Facade
{
    public interface IBasketballFacade : IBasketballState
    {
        /// <summary>Current score; updates after successful baskets and <see cref="ResetRun"/>.</summary>
        IReadOnlyReactiveProperty<int> ScoreRx { get; }

        IReadOnlyReactiveProperty<int> BestScoreRx { get; }

        IReadOnlyReactiveProperty<BasketballBallPhase> PhaseRx { get; }

        /// <summary>Normalized aim charge while the ball is held (0–1).</summary>
        IReadOnlyReactiveProperty<float> AimChargeRx { get; }

        /// <summary>Increments score if rules allow. Returns whether a new point was registered.</summary>
        bool NotifyBasketMade();

        void SetPhase(BasketballBallPhase phase);
        void SetAimCharge(float charge01);
        void ResetRun();
    }
}
