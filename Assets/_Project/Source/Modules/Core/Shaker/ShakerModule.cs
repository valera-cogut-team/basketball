using Core;
using LifeCycle.Facade;
using Logger.Facade;

namespace Shaker
{
    public sealed class ShakerModule : IModule
    {
        public string Name => "Shaker";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "LifeCycle" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IShakerService _service;
        private IShakerFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            var lifeCycle = context.GetModuleFacade<ILifeCycleFacade>()
                            ?? throw new System.InvalidOperationException("ILifeCycleFacade is required before ShakerModule.");

            _service = new ShakerService(lifeCycle);
            context.Container.Bind<IShakerService>().FromInstance(_service).AsSingle();

            _facade = new ShakerFacade(_service);
            context.Container.Bind<IShakerFacade>().FromInstance(_facade).AsSingle();
            context.Container.Bind<IShakerActions>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled)
                return;
            IsEnabled = true;
            _service?.BindLifeCycle();
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("ShakerModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;
            IsEnabled = false;
            _service?.UnbindLifeCycle();
            _service?.SetTarget(null);
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("ShakerModule disabled");
        }

        public void Shutdown()
        {
            if (IsEnabled)
            {
                IsEnabled = false;
                _service?.UnbindLifeCycle();
                _service?.SetTarget(null);
            }

            _service = null;
            _facade = null;
        }
    }
}
