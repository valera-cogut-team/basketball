using System.Threading;
using Cysharp.Threading.Tasks;
using Logger.Facade;

namespace GameScreen.Application
{
    /// <summary>
    /// Local gameplay session only (no network/backend) — audition build.
    /// </summary>
    public sealed class GameScreenService : IGameScreenService
    {
        private readonly ILoggerFacade _logger;

        public GameScreenService(ILoggerFacade logger)
        {
            _logger = logger;
        }

        public UniTask JoinGameAsync(string tableId, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(tableId))
                _logger?.LogInfo($"[GameScreen] Local session (ignored table id: {tableId})");
            return UniTask.CompletedTask;
        }

        public UniTask LeaveGameAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInfo("[GameScreen] Leave game (audition: no menu transition)");
            return UniTask.CompletedTask;
        }
    }
}
