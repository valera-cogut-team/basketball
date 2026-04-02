using UnityEngine;

namespace UIRoot.Facade
{
    /// <summary>
    /// Provides stable UI root transforms (no GameObject.Find/string lookups in runtime code).
    /// </summary>
    public interface IUIRootFacade : IUIRootState{
    }
}


