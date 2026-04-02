using Core;
using UIPopup.Application;
using UIPopup.Facade;
using Zenject;

namespace UIPopup
{
    /// <summary>
    /// UI Popup Module - provides popup management
    /// </summary>
    public class UIPopupModule : IModule
    {
        public string Name => "UIPopup";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger", "Timer" };
        public bool IsEnabled { get; private set; }
        
        private IModuleContext _context;
        private IPopupService _service;
        private IUIPopupFacade _facade;
        private IUIPopupSignals _signals;
        
        public void Initialize(IModuleContext context)
        {
            _context = context;
            
            var logger = context.GetModuleFacade<Logger.Facade.ILoggerFacade>();
            
            // Create service
            var popupService = new PopupService(logger);
            _service = popupService;
            
            // Register in DI container
            context.Container.Bind<IPopupService>().FromInstance(_service).AsSingle();

            // Signals
            _signals = new UIPopupSignals(popupService);

            // Create facade (API + Signals as a single entry point)
            _facade = new UIPopupFacade(_service, _signals);
            context.Container.Bind<IUIPopupFacade>().FromInstance(_facade).AsSingle();
            context.Container.Bind<IUIPopupActions>().FromInstance(_facade).AsSingle();
        }
        
        public void Enable()
        {
            if (IsEnabled)
                return;
            
            IsEnabled = true;
            
            var logger = _context.GetModuleFacade<Logger.Facade.ILoggerFacade>();
            logger?.LogInfo("UIPopupModule enabled");
        }
        
        public void Disable()
        {
            if (!IsEnabled)
                return;
            
            IsEnabled = false;
            
            var logger = _context.GetModuleFacade<Logger.Facade.ILoggerFacade>();
            logger?.LogInfo("UIPopupModule disabled");
        }
        
        public void Shutdown()
        {
            Disable();
            _service = null;
            _facade = null;
            _signals = null;
        }
    }
}

