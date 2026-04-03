using Core;
using Logger.Facade;
using Pool.Application;
using Pool.Facade;
using Zenject;

namespace Pool
{
    public sealed class PoolModule : IModule
    {
        public string Name => "Pool";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private PoolService _service;
        private IPoolFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            _service = new PoolService();
            _facade = new PoolFacade(_service);

            context.Container.Bind<PoolService>().FromInstance(_service).AsSingle();
            context.Container.Bind<IPoolFacade>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled) return;
            IsEnabled = true;
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("PoolModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("PoolModule disabled");
        }

        public void Shutdown()
        {
            Disable();
            _service?.Clear();
            _service = null;
            _facade = null;
        }
    }
}

