namespace SplashScreen.Domain
{
    /// <summary>
    /// State for Splash screen (loading/initialization).
    /// </summary>
    public struct SplashScreenState
    {
        public float Progress;
        public string StatusMessage;
        public bool IsInitialized;
    }
}
