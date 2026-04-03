using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameScreen.Facade
{
    /// <summary>Game screen — public API.</summary>
    public interface IGameScreenFacade : IGameScreenState
    {
        UniTask<GameObject> ShowAsync(object payload = null, CancellationToken cancellationToken = default);
    }
}
