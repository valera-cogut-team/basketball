using System.Threading;
using Cysharp.Threading.Tasks;
using ScreenRouter.Facade;
using UnityEngine;

namespace SplashScreen.Facade
{
    public sealed class SplashScreenFacade : ISplashScreenFacade
    {
        public string AddressableKey => "Screen_Splash";

        private readonly IScreenRouterFacade _router;

        public SplashScreenFacade(IScreenRouterFacade router)
        {
            _router = router ?? throw new System.ArgumentNullException(nameof(router));
        }

        public UniTask<GameObject> ShowAsync(object payload = null, CancellationToken cancellationToken = default)
        {
            return _router.ShowScreenAsync(AddressableKey, payload, cancellationToken);
        }
    }
}
