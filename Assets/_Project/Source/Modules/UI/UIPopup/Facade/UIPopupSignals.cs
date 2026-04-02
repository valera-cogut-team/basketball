using System;
using UIPopup.Application;

namespace UIPopup.Facade
{
    public sealed class UIPopupSignals : IUIPopupSignals
    {
        private readonly IPopupSignalsSource _source;

        public UIPopupSignals(IPopupSignalsSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public IObservable<PopupQueuedEvent> PopupQueuedStream => _source.PopupQueuedStream;
        public IObservable<PopupShownEvent> PopupShownStream => _source.PopupShownStream;
        public IObservable<PopupHidingEvent> PopupHidingStream => _source.PopupHidingStream;
        public IObservable<PopupClosedEvent> PopupClosedStream => _source.PopupClosedStream;
    }
}


