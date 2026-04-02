using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using AppFlow.Domain;
using Logger.Facade;
using SplashScreen.Facade;
using GameScreen.Facade;
using StateMachine.Facade;
using UniRx;

namespace AppFlow.Application
{
    public sealed class AppFlowService : IAppFlowService, IDisposable
    {
        private readonly ILoggerFacade _logger;
        private readonly IStateMachineFacade _stateMachineFacade;
        private readonly ISplashScreenFacade _splash;
        private readonly IGameScreenFacade _game;

        private readonly object _lock = new object();
        private IStateMachine<AppFlowState> _sm;
        private CancellationToken _startupToken;
        private IDisposable _stateSubscription;

        public AppFlowState CurrentState
        {
            get
            {
                lock (_lock)
                {
                    return _sm == null ? default : _sm.Current;
                }
            }
        }

        public AppFlowService(
            ILoggerFacade logger,
            IStateMachineFacade stateMachineFacade,
            ISplashScreenFacade splash,
            IGameScreenFacade game)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stateMachineFacade = stateMachineFacade ?? throw new ArgumentNullException(nameof(stateMachineFacade));
            _splash = splash;
            _game = game;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _stateSubscription?.Dispose();
                _stateSubscription = null;
                _sm = null;
            }
        }

        public UniTask StartAsync(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (_sm != null)
                    return UniTask.CompletedTask;

                _startupToken = cancellationToken;
                _sm = _stateMachineFacade.Create(AppFlowState.Splash);
                _stateSubscription = _sm.StateChangedStream.Subscribe(c => HandleEnter(c.To));
            }

            HandleEnter(AppFlowState.Splash);
            return UniTask.CompletedTask;
        }

        public UniTask<bool> GoToAsync(AppFlowState state, object payload, CancellationToken cancellationToken)
        {
            IStateMachine<AppFlowState> sm;
            lock (_lock)
            {
                sm = _sm;
            }

            if (sm == null)
                return UniTask.FromResult(false);

            if (sm.TryTransition(state))
                return UniTask.FromResult(true);

            if (Equals(sm.Current, state))
                return UniTask.FromResult(true);

            return UniTask.FromResult(false);
        }

        private void HandleEnter(AppFlowState state)
        {
            switch (state)
            {
                case AppFlowState.Splash:
                    _logger.LogInfo("[AppFlow] Enter Splash");
                    _splash?.ShowAsync(payload: null, cancellationToken: _startupToken).Forget();
                    break;
                case AppFlowState.Game:
                    _logger.LogInfo("[AppFlow] Enter Game");
                    _game?.ShowAsync(payload: null, cancellationToken: _startupToken).Forget();
                    break;
                default:
                    _logger.LogWarning($"[AppFlow] State '{state}' is not wired in this build — opening Game");
                    _game?.ShowAsync(payload: null, cancellationToken: _startupToken).Forget();
                    break;
            }
        }
    }
}
