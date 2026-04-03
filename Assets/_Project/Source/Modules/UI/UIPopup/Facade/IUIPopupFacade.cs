using Core;

namespace UIPopup.Facade
{
    /// <summary>UI Popup Facade — single entry point for the popup system.</summary>
    public interface IUIPopupFacade : IHasSignals<IUIPopupSignals>
    {
        string ShowPopup(PopupData popup);
        void HidePopup(string popupId);
        void ClosePopup(string popupId);
    }
}
