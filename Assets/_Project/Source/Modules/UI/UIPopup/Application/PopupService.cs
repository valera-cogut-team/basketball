using System;
using System.Collections.Generic;
using Core.Observable;
using UIPopup.Data;
using UIPopup.Domain;

namespace UIPopup.Application
{
    /// <summary>
    /// Popup service - orchestrates popup management
    /// </summary>
    public class PopupService : IPopupService, IPopupSignalsSource
    {
        // Data-oriented priority queues: O(1) enqueue, O(1) pick next by scanning priorities.
        // Priorities are small and fixed (enum), so this avoids per-enqueue ToList/OrderBy allocations.
        private readonly Queue<PopupData>[] _queuesByPriority =
        {
            new Queue<PopupData>(), // Low
            new Queue<PopupData>(), // Normal
            new Queue<PopupData>(), // High
            new Queue<PopupData>()  // Critical
        };
        private readonly Dictionary<string, PopupData> _activePopups = new Dictionary<string, PopupData>();
        private readonly Logger.Facade.ILoggerFacade _logger;
        private readonly object _lock = new object();

        private readonly SafeStream<Facade.PopupQueuedEvent> _popupQueuedStream;
        private readonly SafeStream<Facade.PopupShownEvent> _popupShownStream;
        private readonly SafeStream<Facade.PopupHidingEvent> _popupHidingStream;
        private readonly SafeStream<Facade.PopupClosedEvent> _popupClosedStream;
        
        public IObservable<Facade.PopupQueuedEvent> PopupQueuedStream => _popupQueuedStream;
        public IObservable<Facade.PopupShownEvent> PopupShownStream => _popupShownStream;
        public IObservable<Facade.PopupHidingEvent> PopupHidingStream => _popupHidingStream;
        public IObservable<Facade.PopupClosedEvent> PopupClosedStream => _popupClosedStream;

        public PopupService(Logger.Facade.ILoggerFacade logger)
        {
            _logger = logger;

            _popupQueuedStream = new SafeStream<Facade.PopupQueuedEvent>(ex => _logger?.LogError($"UIPopup observer error ({nameof(Facade.PopupQueuedEvent)}): {ex.Message}", ex));
            _popupShownStream = new SafeStream<Facade.PopupShownEvent>(ex => _logger?.LogError($"UIPopup observer error ({nameof(Facade.PopupShownEvent)}): {ex.Message}", ex));
            _popupHidingStream = new SafeStream<Facade.PopupHidingEvent>(ex => _logger?.LogError($"UIPopup observer error ({nameof(Facade.PopupHidingEvent)}): {ex.Message}", ex));
            _popupClosedStream = new SafeStream<Facade.PopupClosedEvent>(ex => _logger?.LogError($"UIPopup observer error ({nameof(Facade.PopupClosedEvent)}): {ex.Message}", ex));
        }
        
        /// <summary>
        /// Show popup
        /// </summary>
        public string ShowPopup(PopupData popup)
        {
            if (string.IsNullOrEmpty(popup.Id))
            {
                popup.Id = Guid.NewGuid().ToString();
            }
            
            popup.State = PopupState.Showing;
            
            lock (_lock)
            {
                EnqueueByPriority(popup);
            }
            
            _popupQueuedStream.Publish(new Facade.PopupQueuedEvent(PopupMappings.ToFacade(popup)));
            ProcessQueue();
            
            return popup.Id;
        }
        
        /// <summary>
        /// Hide popup
        /// </summary>
        public void HidePopup(string popupId)
        {
            lock (_lock)
            {
                if (_activePopups.TryGetValue(popupId, out var popup))
                {
                    popup.State = PopupState.Hiding;
                    _activePopups[popupId] = popup;
                    _popupHidingStream.Publish(new Facade.PopupHidingEvent(PopupMappings.ToFacade(popup)));
                }
            }
        }
        
        /// <summary>
        /// Close popup
        /// </summary>
        public void ClosePopup(string popupId)
        {
            lock (_lock)
            {
                if (_activePopups.Remove(popupId))
                {
                    _popupClosedStream.Publish(new Facade.PopupClosedEvent(popupId));
                    ProcessQueue();
                }
            }
        }
        
        private void ProcessQueue()
        {
            lock (_lock)
            {
                if (!HasQueuedPopups())
                    return;
                
                // Only show one popup at a time (unless critical priority)
                var canShow = _activePopups.Count == 0 || HasActiveCriticalPopup();
                
                if (canShow && TryDequeueNext(out var popup))
                {
                    popup.State = PopupState.Visible;
                    _activePopups[popup.Id] = popup;
                    _popupShownStream.Publish(new Facade.PopupShownEvent(PopupMappings.ToFacade(popup)));
                }
            }
        }

        private void EnqueueByPriority(PopupData popup)
        {
            var idx = (int)popup.Priority;
            if (idx < 0) idx = 0;
            if (idx >= _queuesByPriority.Length) idx = _queuesByPriority.Length - 1;
            _queuesByPriority[idx].Enqueue(popup);
        }

        private bool HasQueuedPopups()
        {
            for (int i = _queuesByPriority.Length - 1; i >= 0; i--)
            {
                if (_queuesByPriority[i].Count > 0)
                    return true;
            }
            return false;
        }

        private bool TryDequeueNext(out PopupData popup)
        {
            for (int i = _queuesByPriority.Length - 1; i >= 0; i--)
            {
                if (_queuesByPriority[i].Count > 0)
                {
                    popup = _queuesByPriority[i].Dequeue();
                    return true;
                }
            }

            popup = default;
            return false;
        }

        private bool HasActiveCriticalPopup()
        {
            foreach (var p in _activePopups.Values)
            {
                if (p.Priority == PopupPriority.Critical)
                    return true;
            }
            return false;
        }

        // Mapping is in Data layer (PopupMappings).
    }
}

