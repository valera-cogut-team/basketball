namespace AppFlow.Domain
{
    /// <summary>
    /// High-level application flow states (app-level FSM).
    /// Navigation between screens is executed via ScreenRouter on state enter.
    /// </summary>
    public enum AppFlowState
    {
        Splash = 0,
        Auth = 1,
        Lobby = 2,
        Game = 3,
        Results = 4,
        MainMenu = 5
    }
}
