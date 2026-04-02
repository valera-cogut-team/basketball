namespace StateMachine.Facade
{
    public interface IStateMachineActions
    {
        IStateMachine<TState> Create<TState>(TState initial);
    }
}

