using Core;
using Timer.Application;
using Timer.Facade;
using Logger.Facade;
using Zenject;

namespace Timer
{
    public class TimerModule : IModule
    {
        public string Name => "Timer";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "LifeCycle" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private ITimerService _service;
        private ITimerFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;
            var logger = context.GetModuleFacade<ILoggerFacade>();
            _service = new TimerService(logger);
            context.Container.Bind<ITimerService>().FromInstance(_service).AsSingle();
            _facade = new TimerFacade(_service, context);
            context.Container.Bind<ITimerFacade>().FromInstance(_facade).AsSingle();
        }

        public void Enable() { if (IsEnabled) return; IsEnabled = true; _facade?.Enable(); _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("TimerModule enabled"); }
        public void Disable() { if (!IsEnabled) return; IsEnabled = false; _facade?.Disable(); _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("TimerModule disabled"); }

        public void Shutdown() { Disable(); _service?.Clear(); _service = null; _facade = null; }
    }
}
