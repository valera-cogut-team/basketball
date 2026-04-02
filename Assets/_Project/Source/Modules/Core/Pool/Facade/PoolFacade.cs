using System;
using Pool.Application;
using Pool.Domain;

namespace Pool.Facade
{
    public sealed class PoolFacade : IPoolFacade
    {
        private readonly PoolService _service;

        public PoolFacade(PoolService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IObjectPool<T> CreatePool<T>(string id, Func<T> factory, int initialSize = 10, int maxSize = 100) where T : class, IPoolable
            => _service.CreatePool(id, factory, initialSize, maxSize);

        public IObjectPool<T> GetPool<T>(string id) where T : class, IPoolable
            => _service.GetPool<T>(id);

        public void RemovePool(string id) => _service.RemovePool(id);
    }
}

