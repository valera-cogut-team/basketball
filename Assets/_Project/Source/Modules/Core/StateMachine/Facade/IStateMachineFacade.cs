namespace StateMachine.Facade
{
    public interface IStateMachineFacade
    {
        IStateMachine<TState> Create<TState>(TState initial);
    }
}
