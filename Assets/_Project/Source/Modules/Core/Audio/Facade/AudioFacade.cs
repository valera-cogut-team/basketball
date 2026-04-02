using System;
using Audio.Application;
using UnityEngine;

namespace Audio.Facade
{
    public sealed class AudioFacade : IAudioFacade
    {
        private readonly IAudioService _service;

        public AudioFacade(IAudioService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void PlaySound2D(AudioClip clip, float volumeScale = 1f) => _service.PlaySound2D(clip, volumeScale);

        public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f) => _service.PlayMusic(clip, loop, volume);

        public void StopMusic() => _service.StopMusic();

        public void PauseMusic() => _service.PauseMusic();

        public void ResumeMusic() => _service.ResumeMusic();

        public bool IsMusicPlaying => _service.IsMusicPlaying;

        public float MasterVolumeLinear
        {
            get => _service.MasterVolumeLinear;
            set => _service.MasterVolumeLinear = value;
        }

        public float SfxVolumeLinear
        {
            get => _service.SfxVolumeLinear;
            set => _service.SfxVolumeLinear = value;
        }

        public float MusicVolumeLinear
        {
            get => _service.MusicVolumeLinear;
            set => _service.MusicVolumeLinear = value;
        }

        public bool Muted
        {
            get => _service.Muted;
            set => _service.Muted = value;
        }

        public void SetMasterVolume(float linear01) => MasterVolumeLinear = linear01;

        public void SetSfxVolume(float linear01) => SfxVolumeLinear = linear01;

        public void SetMusicVolume(float linear01) => MusicVolumeLinear = linear01;

        public void SetMuted(bool muted) => Muted = muted;
    }
}
