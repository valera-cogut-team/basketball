using System;

namespace StateMachine.Facade
{
    public interface IStateMachine<TState>
    {
        TState Current { get; }
        IObservable<StateChanged<TState>> StateChangedStream { get; }
        bool TryTransition(TState next);
    }
}

