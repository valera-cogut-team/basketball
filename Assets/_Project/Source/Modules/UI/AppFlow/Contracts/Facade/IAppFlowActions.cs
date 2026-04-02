using Cysharp.Threading.Tasks;
using AppFlow.Domain;
using System.Threading;

namespace AppFlow.Facade
{
    /// <summary>
    /// Commands/use-cases for AppFlow.
    /// </summary>
    public interface IAppFlowActions
    {
        UniTask StartAsync(CancellationToken cancellationToken = default);
        UniTask<bool> GoToAsync(AppFlowState state, object payload = null, CancellationToken cancellationToken = default);
        AppFlowState GetCurrentState();
    }
}
