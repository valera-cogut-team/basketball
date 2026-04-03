using AppFlow.Facade;
using Core;
using Logger.Facade;
using Timer.Facade;
using ScreenRouter.Facade;
using SplashScreen.Application;
using SplashScreen.Facade;
using Zenject;

namespace SplashScreen
{
    /// <summary>
    /// SplashScreen Module - initialization screen.
    /// Addressable key: "Screen_Splash"
    /// </summary>
    public sealed class SplashScreenModule : IModule
    {
        public string Name => "SplashScreen";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "ScreenRouter", "Timer" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private ISplashScreenService _service;
        private ISplashScreenFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            var logger = context.GetModuleFacade<ILoggerFacade>();
            var router = context.GetModuleFacade<IScreenRouterFacade>();
            var timer = context.GetModuleFacade<ITimerFacade>();

            _service = new SplashScreenService(logger, timer, () => context.GetModuleFacade<IAppFlowFacade>());
            _facade = new SplashScreenFacade(router);

            context.Container.Bind<ISplashScreenService>().FromInstance(_service).AsSingle();
            context.Container.Bind<ISplashScreenFacade>().FromInstance(_facade).AsSingle();
            context.Container.Bind<ISplashScreenState>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled)
                return;

            IsEnabled = true;
            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("SplashScreenModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;

            IsEnabled = false;
            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("SplashScreenModule disabled");
        }

        public void Shutdown()
        {
            Disable();
            _service = null;
            _facade = null;
        }
    }
}
