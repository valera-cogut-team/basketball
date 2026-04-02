using UIPopup.Application;

namespace UIPopup.Facade
{
    /// <summary>
    /// UI Popup Facade implementation
    /// </summary>
    public class UIPopupFacade : IUIPopupFacade
    {
        private readonly IPopupService _service;

        public IUIPopupSignals Signals { get; }
        
        public UIPopupFacade(IPopupService service, IUIPopupSignals signals)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
            Signals = signals ?? throw new System.ArgumentNullException(nameof(signals));
        }
        
        public string ShowPopup(PopupData popup)
        {
            // Map Facade DTO to Domain model for Service
            var domainPopup = new Domain.PopupData
            {
                Id = popup.Id,
                AddressableKey = popup.AddressableKey,
                Title = popup.Title,
                Message = popup.Message,
                Priority = (Domain.PopupPriority)popup.Priority,
                Type = (Domain.PopupType)popup.Type,
                State = (Domain.PopupState)popup.State
            };
            
            return _service.ShowPopup(domainPopup);
        }
        
        public void HidePopup(string popupId)
        {
            _service.HidePopup(popupId);
        }
        
        public void ClosePopup(string popupId)
        {
            _service.ClosePopup(popupId);
        }
    }
}

