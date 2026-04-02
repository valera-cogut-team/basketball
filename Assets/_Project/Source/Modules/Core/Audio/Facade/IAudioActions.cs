namespace Audio.Facade
{
    /// <summary>Settings-style actions (can be split for UI later).</summary>
    public interface IAudioActions
    {
        void SetMasterVolume(float linear01);
        void SetSfxVolume(float linear01);
        void SetMusicVolume(float linear01);
        void SetMuted(bool muted);
    }
}
