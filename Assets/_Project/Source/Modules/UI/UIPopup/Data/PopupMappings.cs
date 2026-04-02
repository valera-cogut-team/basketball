using UIPopup.Domain;

namespace UIPopup.Data
{
    internal static class PopupMappings
    {
        public static Facade.PopupData ToFacade(PopupData domain)
        {
            return new Facade.PopupData
            {
                Id = domain.Id,
                AddressableKey = domain.AddressableKey,
                Title = domain.Title,
                Message = domain.Message,
                Priority = (Facade.PopupPriority)domain.Priority,
                Type = (Facade.PopupType)domain.Type,
                State = (Facade.PopupState)domain.State
            };
        }
    }
}

