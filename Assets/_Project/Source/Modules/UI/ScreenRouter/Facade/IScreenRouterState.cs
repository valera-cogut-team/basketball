using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ScreenRouter.Facade
{
    /// <summary>
    /// Read-only state/query API for ScreenRouter.
    /// </summary>
    public interface IScreenRouterState
    {
        string CurrentScreenKey { get; }
        bool CanGoBack { get; }
    }
}
