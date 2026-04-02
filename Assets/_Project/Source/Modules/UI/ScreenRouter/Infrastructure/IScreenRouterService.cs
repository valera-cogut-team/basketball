using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ScreenRouter.Infrastructure
{
    /// <summary>
    /// Internal service interface for screen routing.
    /// Unity-facing by design (GameObject lifecycle, screen prefab instantiation).
    /// </summary>
    public interface IScreenRouterService
    {
        string CurrentScreenKey { get; }
        bool CanGoBack { get; }

        UniTask<GameObject> ShowScreenAsync(string addressableKey, object payload, CancellationToken cancellationToken);
        UniTask<GameObject> ReplaceScreenAsync(string addressableKey, object payload, CancellationToken cancellationToken);
        UniTask GoBackAsync(CancellationToken cancellationToken);
        UniTask CloseCurrentAsync(CancellationToken cancellationToken);
        UniTask ClearAsync(CancellationToken cancellationToken);
    }
}

