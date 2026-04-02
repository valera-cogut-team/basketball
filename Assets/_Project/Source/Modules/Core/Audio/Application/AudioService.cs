using Logger.Facade;
using UnityEngine;

namespace Audio.Application
{
    public sealed class AudioService : IAudioService
    {
        private readonly ILoggerFacade _logger;
        private GameObject _host;
        private AudioSource _music;
        private AudioSource _sfx;

        private float _master = 1f;
        private float _sfxVolume = 1f;
        private float _musicVol = 1f;
        private bool _muted;

        public AudioService(ILoggerFacade logger)
        {
            _logger = logger;
        }

        public float MasterVolumeLinear
        {
            get => _master;
            set { _master = Mathf.Clamp01(value); ApplySourceVolumes(); }
        }

        public float SfxVolumeLinear
        {
            get => _sfxVolume;
            set { _sfxVolume = Mathf.Clamp01(value); }
        }

        public float MusicVolumeLinear
        {
            get => _musicVol;
            set { _musicVol = Mathf.Clamp01(value); ApplySourceVolumes(); }
        }

        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                ApplySourceVolumes();
            }
        }

        public bool IsMusicPlaying => _music != null && _music.isPlaying;

        private void EnsureHost()
        {
            if (_host != null)
                return;

            _host = new GameObject("AudioModule_Root");
            Object.DontDestroyOnLoad(_host);

            _music = _host.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.loop = true;
            _music.spatialBlend = 0f;
            _music.volume = EffectiveMusicVolume();

            var sfxGo = new GameObject("SFX");
            sfxGo.transform.SetParent(_host.transform, false);
            _sfx = sfxGo.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f;
            _sfx.volume = 1f;

            _logger?.LogInfo("[Audio] Root created (2D music + SFX).");
        }

        private float EffectiveMusicVolume() => _muted ? 0f : (_master * _musicVol);

        private void ApplySourceVolumes()
        {
            if (_music != null)
                _music.volume = EffectiveMusicVolume();
        }

        public void PlaySound2D(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null)
                return;
            EnsureHost();
            var v = (_muted ? 0f : _master * _sfxVolume) * Mathf.Clamp01(volumeScale);
            if (v > 1e-4f)
                _sfx.PlayOneShot(clip, v);
        }

        public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
        {
            if (clip == null)
                return;
            EnsureHost();
            _music.loop = loop;
            _music.clip = clip;
            _music.volume = EffectiveMusicVolume() * Mathf.Clamp01(volume);
            _music.Play();
        }

        public void StopMusic()
        {
            if (_music == null)
                return;
            _music.Stop();
            _music.clip = null;
        }

        public void PauseMusic()
        {
            if (_music != null && _music.isPlaying)
                _music.Pause();
        }

        public void ResumeMusic()
        {
            if (_music != null && _music.clip != null && !_music.isPlaying)
                _music.UnPause();
        }

        public void Shutdown()
        {
            if (_host != null)
            {
                Object.Destroy(_host);
                _host = null;
                _music = null;
                _sfx = null;
            }
        }
    }
}
