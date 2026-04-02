namespace Input.Domain
{
    /// <summary>
    /// Touch phase independent of UnityEngine, aligned with legacy Input touch phase ordinals.
    /// </summary>
    public enum InputTouchPhase
    {
        Began = 0,
        Moved = 1,
        Stationary = 2,
        Ended = 3,
        Canceled = 4,
    }
}
