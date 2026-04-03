using Core;
using Logger.Facade;
using Storage.Application;
using Storage.Facade;
using Zenject;

namespace Storage
{
    public sealed class StorageModule : IModule
    {
        public string Name => "Storage";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IStorageService _service;
        private IStorageFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;
            var logger = context.GetModuleFacade<ILoggerFacade>();
            _service = new StorageService(logger);
            context.Container.Bind<IStorageService>().FromInstance(_service).AsSingle();
            _facade = new StorageFacade(_service);
            context.Container.Bind<IStorageFacade>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled)
                return;
            IsEnabled = true;
            _context?.GetModuleFacade<ILoggerFacade>()?.LogInfo("StorageModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;
            IsEnabled = false;
            _context?.GetModuleFacade<ILoggerFacade>()?.LogInfo("StorageModule disabled");
        }

        public void Shutdown()
        {
            Disable();
            _service?.Save();
            _service = null;
            _facade = null;
        }
    }
}
