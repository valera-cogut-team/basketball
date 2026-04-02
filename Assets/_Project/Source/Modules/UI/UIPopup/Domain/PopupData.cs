namespace UIPopup.Domain
{
    /// <summary>
    /// Popup data structure (data-oriented design)
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
    /// Popup type enumeration
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
    /// Popup priority enumeration
    /// </summary>
    public enum PopupPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
    
    /// <summary>
    /// Popup state enumeration
    /// </summary>
    public enum PopupState
    {
        Hidden = 0,
        Showing = 1,
        Visible = 2,
        Hiding = 3
    }
}

