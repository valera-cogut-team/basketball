using System;
using Core.Observable;
using Logger.Facade;
using StateMachine.Facade;

namespace StateMachine.Application
{
    public sealed class StateMachineService
    {
        private readonly ILoggerFacade _logger;

        public StateMachineService(ILoggerFacade logger)
        {
            _logger = logger;
        }

        public IStateMachine<TState> Create<TState>(TState initial) => new StateMachine<TState>(initial, _logger);

        private sealed class StateMachine<TState> : IStateMachine<TState>
        {
            private readonly object _lock = new object();
            private readonly SafeStream<StateChanged<TState>> _stream;
            private readonly ILoggerFacade _logger;
            private TState _current;

            public StateMachine(TState initial, ILoggerFacade logger)
            {
                _current = initial;
                _logger = logger;
                _stream = new SafeStream<StateChanged<TState>>(ex => _logger?.LogError($"StateMachine stream error: {ex.Message}", ex));
            }

            public TState Current { get { lock (_lock) return _current; } }
            public IObservable<StateChanged<TState>> StateChangedStream => _stream;

            public bool TryTransition(TState next)
            {
                StateChanged<TState> evt;
                lock (_lock)
                {
                    // If equals, treat as no-op.
                    if (Equals(_current, next))
                        return false;
                    var prev = _current;
                    _current = next;
                    evt = new StateChanged<TState>(prev, next);
                }
                _stream.Publish(evt);
                return true;
            }
        }
    }
}

