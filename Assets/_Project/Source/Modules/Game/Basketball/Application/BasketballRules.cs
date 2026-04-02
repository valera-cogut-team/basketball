using Basketball.Domain;
using UnityEngine;

namespace Basketball.Application
{
    public static class BasketballRules
    {
        public static bool TryRegisterScore(BasketballGameState state, BasketballTuningConfig tuning)
        {
            var t = tuning ?? BasketballTuningConfig.CreateRuntimeDefault();
            var now = Time.unscaledTime;
            if (now - state.LastScoreUnscaledTime < t.scoreCooldownSeconds)
                return false;

            state.Score++;
            if (state.Score > state.BestScore)
                state.BestScore = state.Score;
            state.LastScoreUnscaledTime = now;
            return true;
        }

        public static void SetPhase(BasketballGameState state, BasketballBallPhase phase)
        {
            state.Phase = phase;
        }
    }
}
