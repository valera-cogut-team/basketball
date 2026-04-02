using UnityEngine;

namespace Audio.Application
{
    /// <summary>Internal 2D audio routing (music + one-shot SFX).</summary>
    public interface IAudioService
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

        void Shutdown();
    }
}
