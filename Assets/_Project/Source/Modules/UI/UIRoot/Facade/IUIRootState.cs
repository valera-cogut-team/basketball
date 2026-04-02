using UnityEngine;

namespace UIRoot.Facade
{
    /// <summary>
    /// Read-only state/query API for UIRoot.
    /// </summary>
    public interface IUIRootState
    {
        Transform CanvasRoot { get; }
        Transform ScreensRoot { get; }
        Transform PopupsRoot { get; }
    }
}
