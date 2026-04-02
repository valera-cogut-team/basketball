using Basketball.Domain;

namespace Basketball.Facade
{
    public interface IBasketballState
    {
        BasketballBallPhase Phase { get; }
        int Score { get; }
        int BestScore { get; }
        float AimCharge01 { get; }
    }
}
