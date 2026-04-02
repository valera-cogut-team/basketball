using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SplashScreen.Facade
{
    /// <summary>
    /// Commands/use-cases available for Splash screen.
    /// </summary>
    public interface ISplashScreenActions
    {
        UniTask<GameObject> ShowAsync(object payload = null, CancellationToken cancellationToken = default);
    }
}
