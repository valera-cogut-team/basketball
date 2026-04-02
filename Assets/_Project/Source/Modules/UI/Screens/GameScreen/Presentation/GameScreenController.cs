using System.Threading;
using Cysharp.Threading.Tasks;
using GameScreen.Application;
using GameScreen.Facade;
using ScreenRouter.Presentation;
using UnityEngine;
using Zenject;

namespace GameScreen.Presentation
{
    /// <summary>
    /// Game screen controller (Unity view) for Screen_Game prefab.
    /// Composes with GameUIModule and optional gameplay modules you register in bootstrap.
    /// </summary>
    public sealed class GameScreenController : MonoBehaviour, IScreenController
    {
        private IGameScreenService _service;
        private IGameScreenFacade _facade;

        [Inject]
        private void Construct(IGameScreenService service, IGameScreenFacade facade)
        {
            _service = service;
            _facade = facade;
        }

        public async UniTask OnShowAsync(object payload, CancellationToken cancellationToken)
        {
            if (_service != null && payload is string tableId)
            {
                await _service.JoinGameAsync(tableId, cancellationToken);
            }
        }

        public async UniTask OnHideAsync(CancellationToken cancellationToken)
        {
            if (_service != null)
            {
                await _service.LeaveGameAsync(cancellationToken);
            }
        }
    }
}
