using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using AppFlow.Application;
using AppFlow.Domain;

namespace AppFlow.Facade
{
    public sealed class AppFlowFacade : IAppFlowFacade
    {
        private readonly IAppFlowService _service;

        public AppFlowFacade(IAppFlowService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public UniTask StartAsync(CancellationToken cancellationToken = default)
        {
            return _service.StartAsync(cancellationToken);
        }

        public UniTask<bool> GoToAsync(AppFlowState state, object payload = null, CancellationToken cancellationToken = default)
        {
            return _service.GoToAsync(state, payload, cancellationToken);
        }

        public AppFlowState GetCurrentState()
        {
            return _service.CurrentState;
        }
    }
}

