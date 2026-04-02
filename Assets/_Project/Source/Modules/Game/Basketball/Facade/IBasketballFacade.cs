using Basketball.Domain;
using UniRx;

namespace Basketball.Facade
{
    public interface IBasketballFacade : IBasketballState, IBasketballActions
    {
        /// <summary>Current score; updates after successful baskets and <see cref="IBasketballActions.ResetRun"/>.</summary>
        IReadOnlyReactiveProperty<int> ScoreRx { get; }

        IReadOnlyReactiveProperty<int> BestScoreRx { get; }

        IReadOnlyReactiveProperty<BasketballBallPhase> PhaseRx { get; }

        /// <summary>Normalized aim charge while the ball is held (0–1).</summary>
        IReadOnlyReactiveProperty<float> AimChargeRx { get; }
    }
}
