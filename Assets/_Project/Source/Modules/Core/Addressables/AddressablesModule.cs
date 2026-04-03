using Cysharp.Threading.Tasks;
using Addressables.Facade;
using Addressables.Infrastructure;
using Core;
using Logger.Facade;
using Zenject;

namespace Addressables
{
    public sealed class AddressablesModule : IModule
    {
        public string Name => "Addressables";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private IAddressablesService _service;
        private IAddressablesFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            _service = new AddressablesService();
            context.Container.Bind<IAddressablesService>().FromInstance(_service).AsSingle();

            _facade = new AddressablesFacade(_service, context);
            context.Container.Bind<IAddressablesFacade>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled) return;
            IsEnabled = true;
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("AddressablesModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("AddressablesModule disabled");
        }

        public void Shutdown()
        {
            Disable();
            _service?.ReleaseAllAsync().Forget();
            _service = null;
            _facade = null;
        }
    }
}

