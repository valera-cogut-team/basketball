using System;
using System.Buffers;
using System.Collections.Generic;
using LifeCycle.Facade;
using Logger.Facade;

namespace LifeCycle.Application
{
    public class LifeCycleService : ILifeCycleService
    {
        private readonly List<IUpdateHandler> _updateHandlers = new List<IUpdateHandler>();
        private readonly List<ILateUpdateHandler> _lateUpdateHandlers = new List<ILateUpdateHandler>();
        private readonly List<IFixedUpdateHandler> _fixedUpdateHandlers = new List<IFixedUpdateHandler>();
        private readonly object _lock = new object();
        private readonly ILoggerFacade _logger;

        public LifeCycleService(ILoggerFacade logger) { _logger = logger ?? throw new ArgumentNullException(nameof(logger)); }

        public void RegisterUpdateHandler(IUpdateHandler handler) { if (handler == null) return; lock (_lock) { if (!_updateHandlers.Contains(handler)) _updateHandlers.Add(handler); } }
        public void UnregisterUpdateHandler(IUpdateHandler handler) { lock (_lock) _updateHandlers.Remove(handler); }
        public void RegisterLateUpdateHandler(ILateUpdateHandler handler) { if (handler == null) return; lock (_lock) { if (!_lateUpdateHandlers.Contains(handler)) _lateUpdateHandlers.Add(handler); } }
        public void UnregisterLateUpdateHandler(ILateUpdateHandler handler) { lock (_lock) _lateUpdateHandlers.Remove(handler); }
        public void RegisterFixedUpdateHandler(IFixedUpdateHandler handler) { if (handler == null) return; lock (_lock) { if (!_fixedUpdateHandlers.Contains(handler)) _fixedUpdateHandlers.Add(handler); } }
        public void UnregisterFixedUpdateHandler(IFixedUpdateHandler handler) { lock (_lock) _fixedUpdateHandlers.Remove(handler); }

        public void OnUpdate(float deltaTime)
        {
            IUpdateHandler[] buffer = null; var count = 0;
            lock (_lock) { count = _updateHandlers.Count; if (count == 0) return; buffer = ArrayPool<IUpdateHandler>.Shared.Rent(count); _updateHandlers.CopyTo(buffer); }
            try { for (var i = 0; i < count; i++) try { buffer[i].OnUpdate(deltaTime); } catch (Exception ex) { _logger.LogError($"Update handler error: {ex.Message}", ex); } }
            finally { ArrayPool<IUpdateHandler>.Shared.Return(buffer, clearArray: true); }
        }

        public void OnLateUpdate(float deltaTime)
        {
            ILateUpdateHandler[] buffer = null; var count = 0;
            lock (_lock) { count = _lateUpdateHandlers.Count; if (count == 0) return; buffer = ArrayPool<ILateUpdateHandler>.Shared.Rent(count); _lateUpdateHandlers.CopyTo(buffer); }
            try { for (var i = 0; i < count; i++) try { buffer[i].OnLateUpdate(deltaTime); } catch (Exception ex) { _logger.LogError($"LateUpdate handler error: {ex.Message}", ex); } }
            finally { ArrayPool<ILateUpdateHandler>.Shared.Return(buffer, clearArray: true); }
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            IFixedUpdateHandler[] buffer = null; var count = 0;
            lock (_lock) { count = _fixedUpdateHandlers.Count; if (count == 0) return; buffer = ArrayPool<IFixedUpdateHandler>.Shared.Rent(count); _fixedUpdateHandlers.CopyTo(buffer); }
            try { for (var i = 0; i < count; i++) try { buffer[i].OnFixedUpdate(fixedDeltaTime); } catch (Exception ex) { _logger.LogError($"FixedUpdate handler error: {ex.Message}", ex); } }
            finally { ArrayPool<IFixedUpdateHandler>.Shared.Return(buffer, clearArray: true); }
        }

        public void Clear() { lock (_lock) { _updateHandlers.Clear(); _lateUpdateHandlers.Clear(); _fixedUpdateHandlers.Clear(); } }
    }
}
