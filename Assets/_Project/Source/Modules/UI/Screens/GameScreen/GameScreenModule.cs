using Core;
using GameScreen.Application;
using GameScreen.Facade;
using Logger.Facade;
using ScreenRouter.Facade;
using Zenject;

namespace GameScreen
{
    /// <summary>
    /// Game screen — local session only for Basketball audition.
    /// </summary>
    public sealed class GameScreenModule : IModule
    {
        public string Name => "GameScreen";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "ScreenRouter" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IGameScreenService _service;
        private IGameScreenFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            var logger = context.GetModuleFacade<ILoggerFacade>();
            var router = context.GetModuleFacade<IScreenRouterFacade>();

            _service = new GameScreenService(logger);
            _facade = new GameScreenFacade(router);

            context.Container.Bind<IGameScreenService>().FromInstance(_service).AsSingle();
            context.Container.Bind<IGameScreenFacade>().FromInstance(_facade).AsSingle();
            context.Container.Bind<IGameScreenActions>().FromInstance(_facade).AsSingle();
            context.Container.Bind<IGameScreenState>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled)
                return;

            IsEnabled = true;
            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("GameScreenModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;

            IsEnabled = false;
            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("GameScreenModule disabled");
        }

        public void Shutdown()
        {
            Disable();
            _service = null;
            _facade = null;
        }
    }
}
