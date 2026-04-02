namespace StateMachine.Facade
{
    public readonly struct StateChanged<TState>
    {
        public TState From { get; }
        public TState To { get; }

        public StateChanged(TState from, TState to)
        {
            From = from;
            To = to;
        }
    }
}

