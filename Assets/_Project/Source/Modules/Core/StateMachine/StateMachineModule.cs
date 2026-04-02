using Core;
using Logger.Facade;
using StateMachine.Application;
using StateMachine.Facade;
using Zenject;

namespace StateMachine
{
    public sealed class StateMachineModule : IModule
    {
        public string Name => "StateMachine";
        public string Version => "1.0.0";
        public string[] Dependencies => new[] { "Logger" };
        public bool IsEnabled { get; private set; }

        private StateMachineService _service;
        private IStateMachineFacade _facade;

        public void Initialize(IModuleContext context)
        {
            var logger = context.GetModuleFacade<ILoggerFacade>();
            _service = new StateMachineService(logger);
            _facade = new StateMachineFacade(_service);

            context.Container.Bind<StateMachineService>().FromInstance(_service).AsSingle();
            context.Container.Bind<IStateMachineFacade>().FromInstance(_facade).AsSingle();
            context.Container.Bind<IStateMachineActions>().FromInstance(_facade).AsSingle();
        }

        public void Enable() => IsEnabled = true;
        public void Disable() => IsEnabled = false;

        public void Shutdown()
        {
            Disable();
            _service = null;
            _facade = null;
        }
    }
}

