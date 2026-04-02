using System.Threading;
using Cysharp.Threading.Tasks;

namespace GameScreen.Application
{
    public interface IGameScreenService
    {
        UniTask JoinGameAsync(string tableId, CancellationToken cancellationToken);
        UniTask LeaveGameAsync(CancellationToken cancellationToken);
    }
}
