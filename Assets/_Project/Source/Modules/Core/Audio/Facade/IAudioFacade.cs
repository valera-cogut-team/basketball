using UnityEngine;

namespace Audio.Facade
{
    /// <summary>Game-facing API for music and sound effects (non-spatial 2D).</summary>
    public interface IAudioFacade
    {
        void PlaySound2D(AudioClip clip, float volumeScale = 1f);
        void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f);
        void StopMusic();
        void PauseMusic();
        void ResumeMusic();
        bool IsMusicPlaying { get; }

        float MasterVolumeLinear { get; set; }
        float SfxVolumeLinear { get; set; }
        float MusicVolumeLinear { get; set; }
        bool Muted { get; set; }

        void SetMasterVolume(float linear01);
        void SetSfxVolume(float linear01);
        void SetMusicVolume(float linear01);
        void SetMuted(bool muted);
    }
}
