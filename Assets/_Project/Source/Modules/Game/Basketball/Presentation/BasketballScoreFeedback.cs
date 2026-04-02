using System;
using Basketball.Application;
using Basketball.Facade;
using Addressables.Facade;
using Audio.Facade;
using Cysharp.Threading.Tasks;
using Logger.Facade;
using Pool.Facade;
using Pool.Application;
using UniRx;
using UnityEngine;

namespace Basketball.Presentation
{
    /// <summary>
    /// Score hook: swish + crowd cheer (Addressables) + pooled hit VFX (Pool + Addressables) + camera wobble via <see cref="BasketballInteractionService"/>.
    /// </summary>
    public sealed class BasketballScoreFeedback : MonoBehaviour
    {
        private static readonly string[] ApplauseAddresses =
        {
            BasketballAddressKeys.ApplauseCheerShort1,
            BasketballAddressKeys.ApplauseCheerShort2,
        };

        private static readonly string[] ScoreVfxAddresses =
        {
            BasketballAddressKeys.VfxScoreHitBasic,
            BasketballAddressKeys.VfxScoreHitBasic2,
            BasketballAddressKeys.VfxScoreHitBasic7,
            BasketballAddressKeys.VfxScoreHitLightningBlue,
            BasketballAddressKeys.VfxScoreHitMagic2,
        };

        private IBasketballFacade _facade;
        private IAudioFacade _audioFacade;
        private IAddressablesFacade _addressables;
        private IPoolFacade _pool;
        private ILoggerFacade _logger;
        private BasketballInteractionService _interaction;
        private Vector3 _scoreVfxWorldPosition;
        private Quaternion _scoreVfxWorldRotation;
        private AudioSource _fallbackSource;
        private static AudioClip _swishClip;
        private bool _destroyed;

        private bool _scoreVfxPoolsReady;
        private bool _scoreVfxPoolsSetupInFlight;
        private UniTask _scoreVfxPoolsSetupTask;
        private string _lastScoreVfxAddress;
        private CompositeDisposable _scoreSubscription;

        public void Initialize(IBasketballFacade facade, IAudioFacade audio, BasketballInteractionService interaction,
            IAddressablesFacade addressables, ILoggerFacade logger, IPoolFacade pool,
            Vector3 scoreVfxWorldPosition, Quaternion scoreVfxWorldRotation)
        {
            _facade = facade;
            _audioFacade = audio;
            _interaction = interaction;
            _addressables = addressables;
            _logger = logger;
            _pool = pool;
            _scoreVfxWorldPosition = scoreVfxWorldPosition;
            _scoreVfxWorldRotation = scoreVfxWorldRotation;

            _scoreSubscription?.Dispose();
            _scoreSubscription = new CompositeDisposable();
            _facade?.ScoreRx.Subscribe(OnScoreChanged).AddTo(_scoreSubscription);

            if (_swishClip == null)
                _swishClip = CreateSwishClip();

            if (_audioFacade == null)
            {
                _fallbackSource = GetComponent<AudioSource>();
                if (_fallbackSource == null)
                    _fallbackSource = gameObject.AddComponent<AudioSource>();
                _fallbackSource.playOnAwake = false;
                _fallbackSource.spatialBlend = 0f;
            }

            WarmApplauseClipsAsync().Forget();
            EnsureScoreVfxPoolsAsync().Forget();
        }

        private void OnDestroy()
        {
            _destroyed = true;
            _scoreSubscription?.Dispose();
            _scoreSubscription = null;

            ReleaseScoreVfxPools();
        }

        private static string ScoreHitPoolId(string address) => "Basketball.PooledScoreHitVfx." + address;

        private void ReleaseScoreVfxPools()
        {
            if (_pool == null)
                return;
            foreach (var address in ScoreVfxAddresses)
            {
                try
                {
                    _pool.RemovePool(ScoreHitPoolId(address));
                }
                catch
                {
                    // ignore
                }
            }

            _scoreVfxPoolsReady = false;
            _scoreVfxPoolsSetupInFlight = false;
            _scoreVfxPoolsSetupTask = default;
        }

        /// <summary>Preload cheer clips so the first basket does not wait on I/O.</summary>
        private async UniTaskVoid WarmApplauseClipsAsync()
        {
            if (_addressables == null)
                return;
            foreach (var address in ApplauseAddresses)
            {
                if (_destroyed)
                    return;
                try
                {
                    await _addressables.LoadAssetAsync<AudioClip>(address);
                }
                catch (System.Exception ex)
                {
                    _logger?.LogWarning($"[Basketball] Applause preload '{address}': {ex.Message}");
                }
            }
        }

        private UniTask EnsureScoreVfxPoolsAsync()
        {
            if (_scoreVfxPoolsReady || _destroyed)
                return UniTask.CompletedTask;
            if (_pool == null || _addressables == null)
                return UniTask.CompletedTask;

            if (!_scoreVfxPoolsSetupInFlight)
            {
                _scoreVfxPoolsSetupInFlight = true;
                _scoreVfxPoolsSetupTask = SetupScoreVfxPoolsInnerAsync();
            }

            return _scoreVfxPoolsSetupTask;
        }

        private async UniTask SetupScoreVfxPoolsInnerAsync()
        {
            try
            {
                var host = transform;
                foreach (var address in ScoreVfxAddresses)
                {
                    if (_destroyed)
                        return;

                    GameObject prefab;
                    try
                    {
                        prefab = await _addressables.LoadPrefabAsync(address);
                    }
                    catch (System.Exception ex)
                    {
                        _logger?.LogWarning($"[Basketball] Score VFX load '{address}': {ex.Message}");
                        continue;
                    }

                    if (prefab == null)
                        continue;

                    var id = ScoreHitPoolId(address);
                    if (_pool.GetPool<PooledScoreHitVfx>(id) != null)
                        continue;

                    var template = prefab;
                    _pool.CreatePool(id, () =>
                    {
                        var go = UnityEngine.Object.Instantiate(template, host);
                        go.name = $"{template.name} (pooled)";
                        go.SetActive(false);
                        return new PooledScoreHitVfx(go);
                    }, 0, 8);
                }

                if (!_destroyed)
                    _scoreVfxPoolsReady = true;
            }
            finally
            {
                _scoreVfxPoolsSetupInFlight = false;
            }
        }

        private void OnScoreChanged(int score)
        {
            if (score <= 0)
                return;

            if (_swishClip != null)
            {
                if (_audioFacade != null)
                    _audioFacade.PlaySound2D(_swishClip, 0.55f);
                else if (_fallbackSource != null)
                    _fallbackSource.PlayOneShot(_swishClip, 0.55f);
            }

            PlayRandomApplauseAsync().Forget();
            PlayRandomScoreVfxAsync().Forget();

            _interaction?.TriggerScoreCameraRotationShake();
        }

        private async UniTaskVoid PlayRandomApplauseAsync()
        {
            if (_audioFacade == null || _addressables == null)
                return;

            var address = ApplauseAddresses[UnityEngine.Random.Range(0, ApplauseAddresses.Length)];
            AudioClip clip;
            try
            {
                clip = await _addressables.LoadAssetAsync<AudioClip>(address);
            }
            catch (System.Exception ex)
            {
                _logger?.LogWarning($"[Basketball] Applause load '{address}': {ex.Message}");
                return;
            }

            if (_destroyed || clip == null)
                return;

            _audioFacade.PlaySound2D(clip, 0.9f);
        }

        private async UniTaskVoid PlayRandomScoreVfxAsync()
        {
            if (_pool == null || _addressables == null)
                return;

            try
            {
                await EnsureScoreVfxPoolsAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"[Basketball] Score VFX pools setup: {ex.Message}");
                return;
            }

            if (_destroyed || !_scoreVfxPoolsReady)
                return;

            if (!TryPickRandomScoreVfxAddress(out var address, out var pool))
            {
                _logger?.LogWarning("[Basketball] Score VFX: no pooled prefabs available (check Addressables / pool setup).");
                return;
            }

            PooledScoreHitVfx item;
            try
            {
                item = pool.Get();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"[Basketball] Score VFX pool Get: {ex.Message}");
                return;
            }

            var root = item.Instance;
            if (root == null)
            {
                try
                {
                    pool.Return(item);
                }
                catch
                {
                    // ignore
                }

                return;
            }

            root.transform.SetPositionAndRotation(_scoreVfxWorldPosition, _scoreVfxWorldRotation);
            root.SetActive(true);
            PlayParticleSystems(root);

            var ttl = EstimateOneShotLifetime(root, 2f);
            try
            {
                await UniTask.WaitForSeconds(ttl);
            }
            catch
            {
                // match EffectsService (scaled time)
            }

            if (_destroyed)
                return;

            try
            {
                pool.Return(item);
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Picks an address that has a live pool. Uses <see cref="System.Random"/> so choice is correct even if this
        /// continuation runs off the main thread; avoids repeating the same effect when several variants exist.
        /// </summary>
        private bool TryPickRandomScoreVfxAddress(out string address, out IObjectPool<PooledScoreHitVfx> pool)
        {
            address = null;
            pool = null;

            var n = 0;
            var candidates = new string[ScoreVfxAddresses.Length];
            var pools = new IObjectPool<PooledScoreHitVfx>[ScoreVfxAddresses.Length];

            foreach (var a in ScoreVfxAddresses)
            {
                var p = _pool.GetPool<PooledScoreHitVfx>(ScoreHitPoolId(a));
                if (p == null)
                    continue;
                candidates[n] = a;
                pools[n] = p;
                n++;
            }

            if (n == 0)
                return false;

            var rng = new System.Random(
                unchecked(Environment.TickCount * 397 ^ GetInstanceID() ^ (Guid.NewGuid().GetHashCode())));

            var idx = 0;
            if (n == 1)
            {
                idx = 0;
            }
            else
            {
                for (var attempt = 0; attempt < 12; attempt++)
                {
                    idx = rng.Next(0, n);
                    if (candidates[idx] != _lastScoreVfxAddress)
                        break;
                }
            }

            _lastScoreVfxAddress = candidates[idx];
            address = candidates[idx];
            pool = pools[idx];
            return true;
        }

        private static void PlayParticleSystems(GameObject root)
        {
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.Clear(true);
                ps.Play(true);
            }
        }

        private static float EstimateOneShotLifetime(GameObject go, float fallbackSeconds)
        {
            var systems = go.GetComponentsInChildren<ParticleSystem>(true);
            if (systems == null || systems.Length == 0)
                return Mathf.Max(fallbackSeconds, 0.05f);

            var maxEnd = 0f;
            foreach (var ps in systems)
            {
                var main = ps.main;
                if (main.loop)
                    return Mathf.Max(fallbackSeconds, 5f);

                var life = Mathf.Max(main.startLifetime.constant, main.startLifetime.constantMax);
                maxEnd = Mathf.Max(maxEnd, main.duration + life);
            }

            return Mathf.Max(maxEnd, 0.05f);
        }

        private static AudioClip CreateSwishClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.12f;
            var n = (int)(sampleRate * duration);
            var samples = new float[n];
            const float freq = 720f;
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = Mathf.Exp(-t * 38f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.35f;
            }

            var clip = AudioClip.Create("BasketballSwish", n, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
