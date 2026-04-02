using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ScreenRouter.Facade
{
    /// <summary>
    /// ScreenRouter Facade - public API for screen navigation.
    /// </summary>
    public interface IScreenRouterFacade : IScreenRouterActions, IScreenRouterState
    {
    }
}


