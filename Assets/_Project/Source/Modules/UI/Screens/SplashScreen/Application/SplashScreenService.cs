using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using AppFlow.Facade;
using Logger.Facade;
using Timer.Facade;

namespace SplashScreen.Application
{
    /// <summary>
    /// Splash screen: short delay via <see cref="ITimerFacade"/> then navigates to game.
    /// </summary>
    public sealed class SplashScreenService : ISplashScreenService
    {
        private readonly ILoggerFacade _logger;
        private readonly ITimerFacade _timer;
        private readonly Func<IAppFlowFacade> _resolveAppFlow;

        public SplashScreenService(ILoggerFacade logger, ITimerFacade timer, Func<IAppFlowFacade> resolveAppFlow)
        {
            _logger = logger;
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _resolveAppFlow = resolveAppFlow ?? throw new ArgumentNullException(nameof(resolveAppFlow));
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInfo("[SplashScreen] Starting initialization...");

            var tcs = new UniTaskCompletionSource();
            var handle = _timer.CreateTimer(0.15f, () => tcs.TrySetResult());

            try
            {
                await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                _timer.CancelTimer(handle);
            }

            _logger?.LogInfo("[SplashScreen] Timer elapsed — opening game");

            var appFlow = _resolveAppFlow.Invoke();
            if (appFlow == null)
            {
                _logger?.LogWarning("[SplashScreen] IAppFlowFacade not available");
                return;
            }

            await appFlow.GoToAsync(AppFlow.Domain.AppFlowState.Game, payload: null, cancellationToken);
        }
    }
}
