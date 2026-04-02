using Pool.Application;
using Pool.Domain;
using System;

namespace Pool.Facade
{
    public interface IPoolActions
    {
        IObjectPool<T> CreatePool<T>(string id, Func<T> factory, int initialSize = 10, int maxSize = 100) where T : class, IPoolable;
        IObjectPool<T> GetPool<T>(string id) where T : class, IPoolable;
        void RemovePool(string id);
    }
}

