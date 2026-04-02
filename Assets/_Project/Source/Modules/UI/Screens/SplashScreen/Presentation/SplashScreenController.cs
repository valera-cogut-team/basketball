using System.Threading;
using Cysharp.Threading.Tasks;
using SplashScreen.Application;
using SplashScreen.Facade;
using ScreenRouter.Presentation;
using UnityEngine;
using Zenject;

namespace SplashScreen.Presentation
{
    /// <summary>
    /// Splash screen controller (Unity view) for Screen_Splash prefab.
    /// </summary>
    public sealed class SplashScreenController : MonoBehaviour, IScreenController
    {
        private ISplashScreenService _service;
        private ISplashScreenFacade _facade;

        [Inject]
        private void Construct(ISplashScreenService service, ISplashScreenFacade facade)
        {
            _service = service;
            _facade = facade;
        }

        public async UniTask OnShowAsync(object payload, CancellationToken cancellationToken)
        {
            if (_service != null)
            {
                await _service.InitializeAsync(cancellationToken);
            }
        }

        public UniTask OnHideAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
