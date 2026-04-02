using UIPopup.Domain;

namespace UIPopup.Application
{
    /// <summary>
    /// Interface for popup service
    /// </summary>
    public interface IPopupService
    {
        /// <summary>
        /// Show popup
        /// </summary>
        string ShowPopup(PopupData popup);

        /// <summary>
        /// Hide popup
        /// </summary>
        void HidePopup(string popupId);

        /// <summary>
        /// Close popup
        /// </summary>
        void ClosePopup(string popupId);
    }
}

