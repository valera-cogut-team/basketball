namespace Basketball.Application
{
    /// <summary>
    /// Resets hoop goal detection (upper-then-lower trigger sequence), e.g. when the ball respawns.
    /// </summary>
    public interface IBasketballGoalSequenceReset
    {
        void ResetGoalSequence();
    }
}
