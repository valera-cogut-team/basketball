using Basketball.Domain;

namespace Basketball.Application
{
    public sealed class BasketballGameState
    {
        public BasketballBallPhase Phase { get; set; } = BasketballBallPhase.Free;
        public int Score { get; set; }
        public int BestScore { get; set; }
        public float AimCharge01 { get; set; }
        public float LastScoreUnscaledTime { get; set; }
    }
}
