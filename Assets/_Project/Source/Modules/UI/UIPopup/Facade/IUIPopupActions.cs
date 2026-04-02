using Core;

namespace UIPopup.Facade
{
    /// <summary>
    /// Commands/use-cases for UIPopup.
    /// </summary>
    public interface IUIPopupActions
    {
        string ShowPopup(PopupData popup);
        void HidePopup(string popupId);
        void ClosePopup(string popupId);
    }
}
