using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameScreen.Facade
{
    /// <summary>
    /// Commands/use-cases available for Game screen.
    /// </summary>
    public interface IGameScreenActions
    {
        UniTask<GameObject> ShowAsync(object payload = null, CancellationToken cancellationToken = default);
    }
}
