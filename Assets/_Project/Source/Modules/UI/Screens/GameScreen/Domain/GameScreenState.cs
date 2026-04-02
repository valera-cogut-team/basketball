namespace GameScreen.Domain
{
    /// <summary>
    /// State for Game screen (active game session).
    /// </summary>
    public struct GameScreenState
    {
        public string TableId;
        public bool IsConnected;
        public bool IsMyTurn;
    }
}
