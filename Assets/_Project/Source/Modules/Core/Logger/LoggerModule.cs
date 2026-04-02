using Core;
using Logger.Application;
using Logger.Facade;
using Logger.Infrastructure;
using Zenject;

namespace Logger
{
    public sealed class LoggerModule : IModule
    {
        public string Name => "Logger";
        public string Version => "1.0.0";
        public string[] Dependencies => new string[0];
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private ILoggerService _service;
        private ILoggerFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;
            _service = new LoggerService();
            _service.AddSink(new UnityDebugSink());
            context.Container.Bind<ILoggerService>().FromInstance(_service).AsSingle();
            _facade = new LoggerFacade(_service);
            context.Container.Bind<ILoggerFacade>().FromInstance(_facade).AsSingle();
            context.Container.Bind<ILoggerActions>().FromInstance(_facade).AsSingle();
        }

        public void Enable() => IsEnabled = true;
        public void Disable() => IsEnabled = false;

        public void Shutdown()
        {
            Disable();
            _service?.Flush();
            _service = null;
            _facade = null;
        }
    }
}
