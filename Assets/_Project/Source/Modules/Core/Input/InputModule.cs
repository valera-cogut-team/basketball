using Core;
using Input.Application;
using Input.Facade;
using Input.Infrastructure;
using Logger.Facade;
using Zenject;

namespace Input
{
    public sealed class InputModule : IModule
    {
        public string Name => "Input";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger" };
        public bool IsEnabled { get; private set; }

        private IModuleContext _context;
        private UnityInputSystemSampler _sampler;
        private InputService _service;
        private IInputFacade _facade;

        public void Initialize(IModuleContext context)
        {
            _context = context;

            _sampler = new UnityInputSystemSampler();
            context.Container.Bind<IInputSampler>().FromInstance(_sampler).AsSingle();

            _service = new InputService(_sampler);
            context.Container.Bind<InputService>().FromInstance(_service).AsSingle();

            _facade = new InputFacade(_service);
            context.Container.Bind<IInputFacade>().FromInstance(_facade).AsSingle();
        }

        public void Enable()
        {
            if (IsEnabled) return;
            IsEnabled = true;
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("InputModule enabled");
        }

        public void Disable()
        {
            if (!IsEnabled) return;
            IsEnabled = false;
            _context.GetModuleFacade<ILoggerFacade>()?.LogInfo("InputModule disabled");
        }

        public void Shutdown()
        {
            Disable();
            _facade = null;
            _service = null;
            _sampler?.Dispose();
            _sampler = null;
        }
    }
}
