using Cysharp.Threading.Tasks;
using AppFlow.Domain;
using System.Threading;

namespace AppFlow.Application
{
    public interface IAppFlowService
    {
        UniTask StartAsync(CancellationToken cancellationToken);
        UniTask<bool> GoToAsync(AppFlowState state, object payload, CancellationToken cancellationToken);
        AppFlowState CurrentState { get; }
    }
}

