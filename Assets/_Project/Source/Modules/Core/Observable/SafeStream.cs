using System;
using System.Buffers;
using System.Collections.Generic;

namespace Core.Observable
{
    /// <summary>
    /// Safe observable stream:
    /// - IObservable&lt;T&gt; surface (System)
    /// - thread-safe subscribe/unsubscribe/publish
    /// - publish is allocation-free (ArrayPool snapshot)
    /// - observer exceptions are caught and forwarded to onObserverError (publisher never crashes)
    /// </summary>
    public sealed class SafeStream<T> : IObservable<T>, IDisposable
    {
        private readonly Action<Exception> _onObserverError;
        private readonly object _lock = new object();
        private List<IObserver<T>> _observers;
        private bool _disposed;

        public SafeStream(Action<Exception> onObserverError = null)
        {
            _onObserverError = onObserverError;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException(nameof(observer));

            lock (_lock)
            {
                if (_disposed)
                    return EmptyDisposable.Instance;

                _observers ??= new List<IObserver<T>>(4);
                _observers.Add(observer);
                return new Subscription(this, observer);
            }
        }

        public void Publish(T evt)
        {
            IObserver<T>[] snapshot = null;
            var count = 0;

            lock (_lock)
            {
                if (_disposed || _observers == null)
                    return;

                count = _observers.Count;
                if (count == 0)
                    return;

                snapshot = ArrayPool<IObserver<T>>.Shared.Rent(count);
                for (var i = 0; i < count; i++)
                    snapshot[i] = _observers[i];
            }

            try
            {
                for (var i = 0; i < count; i++)
                {
                    try
                    {
                        snapshot[i].OnNext(evt);
                    }
                    catch (Exception ex)
                    {
                        _onObserverError?.Invoke(ex);
                    }
                }
            }
            finally
            {
                ArrayPool<IObserver<T>>.Shared.Return(snapshot, clearArray: true);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _disposed = true;
                _observers?.Clear();
                _observers = null;
            }
        }

        private void Unsubscribe(IObserver<T> observer)
        {
            lock (_lock)
            {
                if (_disposed || _observers == null)
                    return;

                _observers.Remove(observer);
                if (_observers.Count == 0)
                    _observers = null;
            }
        }

        private sealed class Subscription : IDisposable
        {
            private SafeStream<T> _owner;
            private IObserver<T> _observer;

            public Subscription(SafeStream<T> owner, IObserver<T> observer)
            {
                _owner = owner;
                _observer = observer;
            }

            public void Dispose()
            {
                var owner = _owner;
                var observer = _observer;
                _owner = null;
                _observer = null;

                owner?.Unsubscribe(observer);
            }
        }

        private sealed class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Instance = new EmptyDisposable();
            private EmptyDisposable() { }
            public void Dispose() { }
        }
    }
}
