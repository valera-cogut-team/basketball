using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SplashScreen.Facade
{
    /// <summary>Splash screen — public API.</summary>
    public interface ISplashScreenFacade : ISplashScreenState
    {
        UniTask<GameObject> ShowAsync(object payload = null, CancellationToken cancellationToken = default);
    }
}
