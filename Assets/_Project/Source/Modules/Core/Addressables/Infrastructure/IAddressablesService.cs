using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Addressables.Infrastructure
{
    public interface IAddressablesService
    {
        UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object;
        UniTask ReleaseAssetAsync(string address);
        UniTask ReleaseAllAsync();
        bool IsAssetLoaded(string address);
    }
}

