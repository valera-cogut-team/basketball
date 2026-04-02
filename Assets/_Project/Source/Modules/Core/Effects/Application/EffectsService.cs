using System;
using System.Threading;
using Addressables.Facade;
using Cysharp.Threading.Tasks;
using Logger.Facade;
using UnityEngine;

namespace Effects.Application
{
    public sealed class EffectsService : IEffectsService
    {
        private readonly IAddressablesFacade _addressables;
        private readonly ILoggerFacade _logger;
        private GameObject _host;

        public EffectsService(IAddressablesFacade addressables, ILoggerFacade logger)
        {
            _addressables = addressables ?? throw new ArgumentNullException(nameof(addressables));
            _logger = logger;
        }

        private void EnsureHost()
        {
            if (_host != null)
                return;

            _host = new GameObject("EffectsModule_Root");
            UnityEngine.Object.DontDestroyOnLoad(_host);
            _logger?.LogInfo("[Effects] Root created.");
        }

        public async UniTask<GameObject> SpawnAsync(string address, Vector3 worldPosition, Quaternion worldRotation,
            Transform parent = null)
        {
            if (string.IsNullOrEmpty(address))
            {
                _logger?.LogWarning("[Effects] SpawnAsync: address is empty.");
                return null;
            }

            GameObject prefab;
            try
            {
                prefab = await _addressables.LoadPrefabAsync(address);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[Effects] LoadPrefabAsync failed for '{address}': {ex.Message}", ex);
                return null;
            }

            if (prefab == null)
            {
                _logger?.LogWarning($"[Effects] Prefab at '{address}' is null.");
                return null;
            }

            EnsureHost();
            var p = parent != null ? parent : _host.transform;
            var instance = UnityEngine.Object.Instantiate(prefab, worldPosition, worldRotation, p);
            instance.name = $"{prefab.name} (instance)";
            return instance;
        }

        public async UniTask PlayOneShotAsync(string address, Vector3 worldPosition, Quaternion worldRotation,
            Transform parent = null, CancellationToken cancellationToken = default, float fallbackLifetimeSeconds = 2f)
        {
            var instance = await SpawnAsync(address, worldPosition, worldRotation, parent);
            if (instance == null)
                return;

            var ttl = EstimateOneShotLifetime(instance, fallbackLifetimeSeconds);
            try
            {
                await UniTask.WaitForSeconds(ttl, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Cancelled: tear down immediately.
            }

            if (instance != null)
                UnityEngine.Object.Destroy(instance);
        }

        public void Despawn(GameObject instance)
        {
            if (instance != null)
                UnityEngine.Object.Destroy(instance);
        }

        public void Shutdown()
        {
            if (_host != null)
            {
                UnityEngine.Object.Destroy(_host);
                _host = null;
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
    }
}
