using System.Threading;
using Cysharp.Threading.Tasks;
using ScreenRouter.Infrastructure;
using UnityEngine;

namespace ScreenRouter.Facade
{
    /// <summary>
    /// ScreenRouter Facade implementation.
    /// </summary>
    public sealed class ScreenRouterFacade : IScreenRouterFacade
    {
        private readonly IScreenRouterService _service;

        public string CurrentScreenKey => _service.CurrentScreenKey;
        public bool CanGoBack => _service.CanGoBack;

        public ScreenRouterFacade(IScreenRouterService service)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
        }

        public UniTask<GameObject> ShowScreenAsync(string addressableKey, object payload = null, CancellationToken cancellationToken = default)
        {
            return _service.ShowScreenAsync(addressableKey, payload, cancellationToken);
        }

        public UniTask<GameObject> ReplaceScreenAsync(string addressableKey, object payload = null, CancellationToken cancellationToken = default)
        {
            return _service.ReplaceScreenAsync(addressableKey, payload, cancellationToken);
        }

        public UniTask GoBackAsync(CancellationToken cancellationToken = default)
        {
            return _service.GoBackAsync(cancellationToken);
        }

        public UniTask CloseCurrentAsync(CancellationToken cancellationToken = default)
        {
            return _service.CloseCurrentAsync(cancellationToken);
        }

        public UniTask ClearAsync(CancellationToken cancellationToken = default)
        {
            return _service.ClearAsync(cancellationToken);
        }
    }
}


