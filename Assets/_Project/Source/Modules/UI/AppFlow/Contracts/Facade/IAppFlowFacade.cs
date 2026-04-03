using Cysharp.Threading.Tasks;
using AppFlow.Domain;
using System.Threading;

namespace AppFlow.Facade
{
    public interface IAppFlowFacade
    {
        UniTask StartAsync(CancellationToken cancellationToken = default);
        UniTask<bool> GoToAsync(AppFlowState state, object payload = null, CancellationToken cancellationToken = default);
        AppFlowState GetCurrentState();
    }
}
