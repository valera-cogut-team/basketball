namespace UIPopup.Facade
{
    /// <summary>
    /// Popup data structure (moved from Domain to Facade for Presentation layer access).
    /// </summary>
    public struct PopupData
    {
        public string Id { get; set; }
        /// <summary>
        /// Addressables key for popup prefab (optional but recommended for modular popups).
        /// Example: "Popup_ConfirmDialog"
        /// </summary>
        public string AddressableKey { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public PopupPriority Priority { get; set; }
        public PopupType Type { get; set; }
        public PopupState State { get; set; }
    }
    
    /// <summary>
    /// Popup type enumeration (moved from Domain to Facade for Presentation layer access).
    /// </summary>
    public enum PopupType
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Confirm = 3,
        Custom = 4
    }
    
    /// <summary>
    /// Popup priority enumeration (moved from Domain to Facade for Presentation layer access).
    /// </summary>
    public enum PopupPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
    
    /// <summary>
    /// Popup state enumeration (moved from Domain to Facade for Presentation layer access).
    /// </summary>
    public enum PopupState
    {
        Hidden = 0,
        Showing = 1,
        Visible = 2,
        Hiding = 3
    }

    /// <summary>
    /// UIPopup public event DTOs (used by Signals and consumers).
    /// Keep these types in Facade so other modules don't depend on UIPopup.Application.
    /// </summary>
    public readonly struct PopupQueuedEvent
    {
        public PopupData Popup { get; }

        public PopupQueuedEvent(PopupData popup) => Popup = popup;
    }

    public readonly struct PopupShownEvent
    {
        public PopupData Popup { get; }

        public PopupShownEvent(PopupData popup) => Popup = popup;
    }

    public readonly struct PopupHidingEvent
    {
        public PopupData Popup { get; }

        public PopupHidingEvent(PopupData popup) => Popup = popup;
    }

    public readonly struct PopupClosedEvent
    {
        public string PopupId { get; }

        public PopupClosedEvent(string popupId) => PopupId = popupId;
    }
}

