using System.Threading;
using Cysharp.Threading.Tasks;

namespace ScreenRouter.Presentation
{
    /// <summary>
    /// Optional screen lifecycle hooks implemented by a MonoBehaviour on a screen prefab root (or child).
    /// </summary>
    public interface IScreenController
    {
        /// <summary>
        /// Called after the screen is instantiated and set active.
        /// </summary>
        UniTask OnShowAsync(object payload, CancellationToken cancellationToken);

        /// <summary>
        /// Called before the screen is hidden or destroyed.
        /// </summary>
        UniTask OnHideAsync(CancellationToken cancellationToken);
    }
}


