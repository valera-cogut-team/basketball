using Addressables.Facade;
using Core;
using Logger.Facade;
using ScreenRouter.Facade;
using ScreenRouter.Infrastructure;
using UIRoot.Facade;
using Zenject;

namespace ScreenRouter
{
    /// <summary>
    /// ScreenRouter Module - provides navigation between "screen modules" (each screen is its own module).
    /// </summary>
    public sealed class ScreenRouterModule : IModule
    {
        public string Name => "ScreenRouter";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "Addressables", "UIRoot" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IScreenRouterService _service;
        private IScreenRouterFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            var addressables = context.GetModuleFacade<IAddressablesFacade>();
            var logger = context.GetModuleFacade<ILoggerFacade>();
            var uiRoot = context.GetModuleFacade<IUIRootFacade>();

            // Create service
            _service = new ScreenRouterService(context.Container, uiRoot, addressables, logger, new ScreenRouterConfig());

            // Register in DI container
            context.Container.Bind<IScreenRouterService>().FromInstance(_service).AsSingle();

            // Create facade
            _facade = new ScreenRouterFacade(_service);
            context.Container.Bind<IScreenRouterFacade>().FromInstance(_facade).AsSingle();
            context.Container.Bind<IScreenRouterActions>().FromInstance(_facade).AsSingle();
            context.Container.Bind<IScreenRouterState>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled)
                return;

            IsEnabled = true;

            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("ScreenRouterModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;

            IsEnabled = false;

            var logger = _context.GetModuleFacade<ILoggerFacade>();
            logger?.LogInfo("ScreenRouterModule disabled");
        }

        public void Shutdown()
        {
            Disable();
            _service = null;
            _facade = null;
        }
    }
}


