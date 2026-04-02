using System;
using System.Threading;
using Addressables.Facade;
using Basketball.Application;
using Basketball.Facade;
using Basketball.Presentation;
using Core;
using Cysharp.Threading.Tasks;
using Input.Facade;
using LifeCycle.Facade;
using Logger.Facade;
using Pool.Facade;
using Audio.Facade;
using Effects.Facade;
using Storage.Facade;
using UnityEngine;
using Zenject;

namespace Basketball
{
    public sealed class BasketballModule : IModule
    {
        public string Name => "Basketball";
        public string Version => "1.0.0";
        public string[] Dependencies => new[]
            { "Logger", "Storage", "Input", "LifeCycle", "Addressables", "Pool", "Audio", "Effects" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private BasketballGameState _state;
        private BasketballFacade _facade;
        private GameObject _root;
        private BasketballInteractionService _interaction;
        private ILifeCycleFacade _lifeCycle;
        private BasketballTuningConfig _tuning;
        private IAddressablesFacade _addressables;
        private bool _contentReady;

        public void Initialize(IModuleContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Loads tuning from Addressables and binds Zenject contracts. Call after Addressables module is enabled.
        /// </summary>
        public async UniTask PrepareContentAsync(CancellationToken cancellationToken = default)
        {
            if (_contentReady)
                return;

            if (_context == null)
                throw new InvalidOperationException("BasketballModule.Initialize must run before PrepareContentAsync.");

            _addressables = _context.GetModuleFacade<IAddressablesFacade>()
                             ?? throw new InvalidOperationException("IAddressablesFacade is not registered.");

            var logger = _context.GetModuleFacade<ILoggerFacade>();

            try
            {
                _tuning = await _addressables.LoadAssetAsync<BasketballTuningConfig>(BasketballAddressKeys.Config)
                    .AttachExternalCancellation(cancellationToken);
            }
            catch (Exception ex)
            {
                logger?.LogError($"[Basketball] Failed to load '{BasketballAddressKeys.Config}': {ex.Message}", ex);
                _tuning = BasketballTuningConfig.CreateRuntimeDefault();
            }

            if (_tuning == null)
                _tuning = BasketballTuningConfig.CreateRuntimeDefault();

            _context.Container.Bind<BasketballTuningConfig>().FromInstance(_tuning).AsSingle();

            _state = new BasketballGameState();
            var storage = _context.GetModuleFacade<IStorageFacade>();
            if (storage != null)
                _state.BestScore = Mathf.Max(0, storage.GetInt(BasketballPersistenceKeys.BestScore, 0));

            _facade = new BasketballFacade(_state, _tuning, storage);
            _context.Container.Bind<IBasketballFacade>().FromInstance(_facade).AsSingle();
            _context.Container.Bind<IBasketballActions>().FromInstance(_facade).AsSingle();
            _context.Container.Bind<IBasketballState>().FromInstance(_facade).AsSingle();

            _contentReady = true;
        }

        public void Enable()
        {
            if (IsEnabled)
                return;

            if (!_contentReady || _tuning == null || _facade == null || _addressables == null)
            {
                _context?.GetModuleFacade<ILoggerFacade>()
                    ?.LogError("[Basketball] Enable called before PrepareContentAsync completed.");
                return;
            }

            IsEnabled = true;

            var logger = _context.GetModuleFacade<ILoggerFacade>();
            var input = _context.Container.Resolve<IInputFacade>();
            _lifeCycle = _context.GetModuleFacade<ILifeCycleFacade>();
            logger?.LogInfo("BasketballModule enabled");

            _interaction = new BasketballInteractionService(input, _facade, _tuning);
            _lifeCycle?.RegisterUpdateHandler(_interaction);

            var pool = _context.GetModuleFacade<IPoolFacade>();
            var audio = _context.GetModuleFacade<IAudioFacade>();
            var effects = _context.GetModuleFacade<IEffectsFacade>();
            if (pool != null && pool.GetPool<PooledTrajectoryBuffer>(BasketballPoolIds.TrajectoryBuffer) == null)
                pool.CreatePool(BasketballPoolIds.TrajectoryBuffer, () => new PooledTrajectoryBuffer(), 2, 8);

            _root = new GameObject("BasketballWorld");
            UnityEngine.Object.DontDestroyOnLoad(_root);
            var driver = _root.AddComponent<BasketballGameplayRoot>();
            driver.Initialize(_interaction, _facade, logger, _tuning, _addressables, pool, audio, effects, _lifeCycle);
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;
            IsEnabled = false;

            if (_interaction != null && _lifeCycle != null)
            {
                _lifeCycle.UnregisterUpdateHandler(_interaction);
                _interaction.Teardown();
                _interaction = null;
            }

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }
        }

        public void Shutdown()
        {
            Disable();

            if (_addressables != null && _contentReady)
                _addressables.ReleaseAssetAsync(BasketballAddressKeys.Config).Forget();

            _facade = null;
            _state = null;
            _tuning = null;
            _contentReady = false;
            _addressables = null;
        }
    }
}
