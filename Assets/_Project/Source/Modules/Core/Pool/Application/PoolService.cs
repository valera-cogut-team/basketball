using System;
using System.Collections.Generic;
using Pool.Domain;

namespace Pool.Application
{
    public sealed class PoolService : IPoolService
    {
        private readonly Dictionary<string, object> _pools = new Dictionary<string, object>();
        private readonly object _lock = new object();

        public IObjectPool<T> CreatePool<T>(string id, Func<T> factory, int initialSize = 10, int maxSize = 100)
            where T : class, IPoolable
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Pool ID cannot be null or empty", nameof(id));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            lock (_lock)
            {
                if (_pools.ContainsKey(id))
                    throw new InvalidOperationException($"Pool with ID '{id}' already exists");
                var pool = new ObjectPool<T>(factory, initialSize, maxSize);
                _pools[id] = pool;
                return pool;
            }
        }

        public IObjectPool<T> GetPool<T>(string id) where T : class, IPoolable
        {
            lock (_lock)
            {
                if (_pools.TryGetValue(id, out var pool))
                    return pool as IObjectPool<T>;
                return null;
            }
        }

        public void RemovePool(string id)
        {
            lock (_lock)
            {
                _pools.Remove(id);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pools.Clear();
            }
        }
    }

    public interface IPoolService
    {
        IObjectPool<T> CreatePool<T>(string id, Func<T> factory, int initialSize = 10, int maxSize = 100) where T : class, IPoolable;
        IObjectPool<T> GetPool<T>(string id) where T : class, IPoolable;
        void RemovePool(string id);
        void Clear();
    }

    public interface IObjectPool<T> where T : class, IPoolable
    {
        T Get();
        void Return(T item);
        void Clear();
        int ActiveCount { get; }
        int InactiveCount { get; }
    }

    internal sealed class ObjectPool<T> : IObjectPool<T> where T : class, IPoolable
    {
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly Func<T> _factory;
        private readonly int _maxSize;
        private int _activeCount;

        public int ActiveCount => _activeCount;
        public int InactiveCount => _pool.Count;

        public ObjectPool(Func<T> factory, int initialSize, int maxSize)
        {
            _factory = factory;
            _maxSize = Math.Max(0, maxSize);

            var warm = Math.Max(0, initialSize);
            for (var i = 0; i < warm; i++)
                _pool.Enqueue(_factory());
        }

        public T Get()
        {
            T item = _pool.Count > 0 ? _pool.Dequeue() : _factory();
            _activeCount++;
            return item;
        }

        public void Return(T item)
        {
            if (item == null)
                return;

            item.Reset();

            if (_pool.Count < _maxSize)
                _pool.Enqueue(item);

            _activeCount = Math.Max(0, _activeCount - 1);
        }

        public void Clear()
        {
            _pool.Clear();
            _activeCount = 0;
        }
    }
}

