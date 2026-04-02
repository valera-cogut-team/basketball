using Addressables.Facade;
using Core;
using Effects.Application;
using Effects.Facade;
using Logger.Facade;

namespace Effects
{
    public sealed class EffectsModule : IModule
    {
        public string Name => "Effects";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "Addressables" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IEffectsService _service;
        private IEffectsFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            var addressables = context.GetModuleFacade<IAddressablesFacade>()
                               ?? throw new System.InvalidOperationException("IAddressablesFacade is required before EffectsModule.");
            var logger = context.GetModuleFacade<ILoggerFacade>();

            _service = new EffectsService(addressables, logger);
            context.Container.Bind<IEffectsService>().FromInstance(_service).AsSingle();

            _facade = new EffectsFacade(_service);
            context.Container.Bind<IEffectsFacade>().FromInstance(_facade).AsSingle();
            context.Container.Bind<IEffectsActions>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled)
                return;
            IsEnabled = true;
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("EffectsModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;
            IsEnabled = false;
            _service?.Shutdown();
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("EffectsModule disabled");
        }

        public void Shutdown()
        {
            if (IsEnabled)
            {
                IsEnabled = false;
                _service?.Shutdown();
            }

            _service = null;
            _facade = null;
        }
    }
}
