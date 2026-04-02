using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ScreenRouter.Facade
{
    /// <summary>
    /// Commands/use-cases for ScreenRouter.
    /// </summary>
    public interface IScreenRouterActions
    {
        UniTask<GameObject> ShowScreenAsync(string addressableKey, object payload = null, CancellationToken cancellationToken = default);
        UniTask<GameObject> ReplaceScreenAsync(string addressableKey, object payload = null, CancellationToken cancellationToken = default);
        UniTask GoBackAsync(CancellationToken cancellationToken = default);
        UniTask CloseCurrentAsync(CancellationToken cancellationToken = default);
        UniTask ClearAsync(CancellationToken cancellationToken = default);
    }
}
