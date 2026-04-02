using System.Threading;
using Cysharp.Threading.Tasks;
using ScreenRouter.Facade;
using UnityEngine;

namespace GameScreen.Facade
{
    public sealed class GameScreenFacade : IGameScreenFacade
    {
        public string AddressableKey => "Screen_Game";

        private readonly IScreenRouterFacade _router;

        public GameScreenFacade(IScreenRouterFacade router)
        {
            _router = router ?? throw new System.ArgumentNullException(nameof(router));
        }

        public UniTask<GameObject> ShowAsync(object payload = null, CancellationToken cancellationToken = default)
        {
            return _router.ShowScreenAsync(AddressableKey, payload, cancellationToken);
        }
    }
}
