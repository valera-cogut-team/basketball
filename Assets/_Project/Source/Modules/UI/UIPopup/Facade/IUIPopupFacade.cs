using Core;

namespace UIPopup.Facade
{
    /// <summary>
    /// UI Popup Facade — single entry point for the popup system.
    /// </summary>
    public interface IUIPopupFacade : IUIPopupActions, IHasSignals<IUIPopupSignals>{
    }
}

