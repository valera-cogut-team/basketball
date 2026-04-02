using System;
using StateMachine.Application;

namespace StateMachine.Facade
{
    public sealed class StateMachineFacade : IStateMachineFacade
    {
        private readonly StateMachineService _service;

        public StateMachineFacade(StateMachineService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IStateMachine<TState> Create<TState>(TState initial) => _service.Create(initial);
    }
}

