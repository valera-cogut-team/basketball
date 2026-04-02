using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Addressables.Facade;
using Logger.Facade;
using ScreenRouter.Presentation;
using UIRoot.Facade;
using UnityEngine;
using Zenject;

namespace ScreenRouter.Infrastructure
{
    /// <summary>
    /// Screen router service - orchestrates screen navigation using Addressables prefabs.
    /// Unity-facing orchestration: instantiation, activation, controller lifecycle calls.
    /// </summary>
    public sealed class ScreenRouterService : IScreenRouterService
    {
        private readonly IAddressablesFacade _addressables;
        private readonly ILoggerFacade _logger;
        private readonly ScreenRouterConfig _config;
        private readonly DiContainer _container;
        private readonly IUIRootFacade _uiRoot;

        private readonly object _lock = new object();
        private readonly Stack<ScreenEntry> _stack = new Stack<ScreenEntry>();

        private Transform _screensRoot;

        public string CurrentScreenKey
        {
            get
            {
                lock (_lock)
                {
                    return _stack.Count == 0 ? null : _stack.Peek().AddressableKey;
                }
            }
        }

        public bool CanGoBack
        {
            get
            {
                lock (_lock)
                {
                    return _stack.Count > 1;
                }
            }
        }

        public ScreenRouterService(DiContainer container, IUIRootFacade uiRoot, IAddressablesFacade addressables, ILoggerFacade logger, ScreenRouterConfig config = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _uiRoot = uiRoot ?? throw new ArgumentNullException(nameof(uiRoot));
            _addressables = addressables ?? throw new ArgumentNullException(nameof(addressables));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? new ScreenRouterConfig();
        }

        public async UniTask<GameObject> ShowScreenAsync(string addressableKey, object payload, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(addressableKey))
                throw new ArgumentException("Addressable key cannot be null or empty.", nameof(addressableKey));

            EnsureRoot();

            ScreenEntry current;
            lock (_lock)
            {
                current = _stack.Count > 0 ? _stack.Peek() : default;
            }

            if (current.Instance != null)
            {
                await HideEntryAsync(current, destroy: !_config.KeepPreviousScreensInMemory, cancellationToken);
                if (!_config.KeepPreviousScreensInMemory)
                {
                    lock (_lock)
                    {
                        _stack.Pop();
                    }
                }
            }

            var entry = await CreateEntryAsync(addressableKey, payload, cancellationToken);
            lock (_lock)
            {
                _stack.Push(entry);
            }

            return entry.Instance;
        }

        public async UniTask<GameObject> ReplaceScreenAsync(string addressableKey, object payload, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(addressableKey))
                throw new ArgumentException("Addressable key cannot be null or empty.", nameof(addressableKey));

            EnsureRoot();

            ScreenEntry current;
            lock (_lock)
            {
                current = _stack.Count > 0 ? _stack.Pop() : default;
            }

            if (current.Instance != null)
            {
                await HideEntryAsync(current, destroy: true, cancellationToken);
            }

            var entry = await CreateEntryAsync(addressableKey, payload, cancellationToken);
            lock (_lock)
            {
                _stack.Push(entry);
            }
            return entry.Instance;
        }

        public async UniTask GoBackAsync(CancellationToken cancellationToken)
        {
            ScreenEntry current;
            ScreenEntry previous;

            lock (_lock)
            {
                if (_stack.Count <= 1)
                    return;

                current = _stack.Pop();
                previous = _stack.Peek();
            }

            await HideEntryAsync(current, destroy: true, cancellationToken);

            if (previous.Instance != null)
            {
                previous.Instance.SetActive(true);
                await ShowEntryAsync(previous, payload: null, cancellationToken);
            }
        }

        public async UniTask CloseCurrentAsync(CancellationToken cancellationToken)
        {
            ScreenEntry current;
            ScreenEntry previous;

            lock (_lock)
            {
                if (_stack.Count == 0)
                    return;

                current = _stack.Pop();
                previous = _stack.Count > 0 ? _stack.Peek() : default;
            }

            await HideEntryAsync(current, destroy: true, cancellationToken);

            if (previous.Instance != null)
            {
                previous.Instance.SetActive(true);
                await ShowEntryAsync(previous, payload: null, cancellationToken);
            }
        }

        public async UniTask ClearAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                ScreenEntry entry;
                lock (_lock)
                {
                    if (_stack.Count == 0)
                        break;
                    entry = _stack.Pop();
                }

                await HideEntryAsync(entry, destroy: true, cancellationToken);
            }
        }

        private void EnsureRoot()
        {
            if (_screensRoot != null)
                return;

            if (_config.ScreensParent != null)
            {
                _screensRoot = _config.ScreensParent;
                return;
            }

            // Root is provided by UIRootModule (no string lookups).
            _screensRoot = _uiRoot.ScreensRoot;
        }

        private async UniTask<ScreenEntry> CreateEntryAsync(string addressableKey, object payload, CancellationToken cancellationToken)
        {
            _logger.LogInfo($"[ScreenRouter] Loading screen prefab: {addressableKey}");
            var prefab = await _addressables.LoadPrefabAsync(addressableKey);
            cancellationToken.ThrowIfCancellationRequested();

            // Instantiate via Zenject so [Inject] works on screen components.
            var instance = _container.InstantiatePrefab(prefab, _screensRoot);
            instance.name = $"{prefab.name} (Screen)";
            instance.SetActive(true);

            var controller = instance.GetComponentInChildren<MonoBehaviour>(true) as IScreenController;
            var entry = new ScreenEntry(addressableKey, instance, controller);

            await ShowEntryAsync(entry, payload, cancellationToken);
            return entry;
        }

        private async UniTask ShowEntryAsync(ScreenEntry entry, object payload, CancellationToken cancellationToken)
        {
            if (entry.Controller == null)
                return;

            try
            {
                await entry.Controller.OnShowAsync(payload, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ScreenRouter] Screen OnShowAsync failed for '{entry.AddressableKey}': {ex.Message}", ex);
            }
        }

        private async UniTask HideEntryAsync(ScreenEntry entry, bool destroy, CancellationToken cancellationToken)
        {
            if (entry.Instance == null)
                return;

            if (entry.Controller != null)
            {
                try
                {
                    await entry.Controller.OnHideAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[ScreenRouter] Screen OnHideAsync failed for '{entry.AddressableKey}': {ex.Message}", ex);
                }
            }

            if (!destroy)
            {
                entry.Instance.SetActive(false);
                return;
            }

            UnityEngine.Object.Destroy(entry.Instance);

            // Optional: release prefab handle to reduce memory.
            // Note: if you keep screens in memory, releasing here may force reload later.
            await _addressables.ReleaseAssetAsync(entry.AddressableKey);
        }

        private readonly struct ScreenEntry
        {
            public string AddressableKey { get; }
            public GameObject Instance { get; }
            public IScreenController Controller { get; }

            public ScreenEntry(string addressableKey, GameObject instance, IScreenController controller)
            {
                AddressableKey = addressableKey;
                Instance = instance;
                Controller = controller;
            }
        }
    }
}

