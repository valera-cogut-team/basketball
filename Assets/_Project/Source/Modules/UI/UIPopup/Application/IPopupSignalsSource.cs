using System;
using UIPopup.Facade;

namespace UIPopup.Application
{
    /// <summary>
    /// Internal signal source for UIPopup module (no EventBus dependency).
    /// </summary>
    public interface IPopupSignalsSource
    {
        IObservable<PopupQueuedEvent> PopupQueuedStream { get; }
        IObservable<PopupShownEvent> PopupShownStream { get; }
        IObservable<PopupHidingEvent> PopupHidingStream { get; }
        IObservable<PopupClosedEvent> PopupClosedStream { get; }
    }
}


