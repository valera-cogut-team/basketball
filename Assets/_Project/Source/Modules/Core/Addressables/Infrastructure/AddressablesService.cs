using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

namespace Addressables.Infrastructure
{
    public sealed class AddressablesService : IAddressablesService
    {
        private readonly Dictionary<string, AsyncOperationHandle> _loadedAssets = new Dictionary<string, AsyncOperationHandle>();
        private readonly object _lock = new object();

        public async UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(address))
                throw new System.ArgumentException("Address cannot be null or empty", nameof(address));

            lock (_lock)
            {
                if (_loadedAssets.TryGetValue(address, out var handle))
                {
                    if (handle.IsValid() && handle.IsDone)
                        return handle.Result as T;
                }
            }

            var loadHandle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(address);
            var asset = await loadHandle.ToUniTask();

            lock (_lock)
            {
                _loadedAssets[address] = loadHandle;
            }

            return asset;
        }

        public async UniTask ReleaseAssetAsync(string address)
        {
            lock (_lock)
            {
                if (_loadedAssets.TryGetValue(address, out var handle))
                {
                    if (handle.IsValid())
                        UnityEngine.AddressableAssets.Addressables.Release(handle);
                    _loadedAssets.Remove(address);
                }
            }

            await UniTask.Yield();
        }

        public async UniTask ReleaseAllAsync()
        {
            List<AsyncOperationHandle> handles;
            lock (_lock)
            {
                handles = new List<AsyncOperationHandle>(_loadedAssets.Values);
                _loadedAssets.Clear();
            }

            foreach (var h in handles)
            {
                if (h.IsValid())
                    UnityEngine.AddressableAssets.Addressables.Release(h);
            }

            await UniTask.Yield();
        }

        public bool IsAssetLoaded(string address)
        {
            lock (_lock)
            {
                if (_loadedAssets.TryGetValue(address, out var handle))
                    return handle.IsValid() && handle.IsDone;
                return false;
            }
        }
    }
}

