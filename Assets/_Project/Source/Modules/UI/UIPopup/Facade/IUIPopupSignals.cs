using System;

namespace UIPopup.Facade
{
    public interface IUIPopupSignals
    {
        IObservable<PopupQueuedEvent> PopupQueuedStream { get; }
        IObservable<PopupShownEvent> PopupShownStream { get; }
        IObservable<PopupHidingEvent> PopupHidingStream { get; }
        IObservable<PopupClosedEvent> PopupClosedStream { get; }
    }
}


