using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Addressables.Facade
{
    public interface IAddressablesFacade
    {
        UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object;
        UniTask<GameObject> LoadPrefabAsync(string address);
        UniTask ReleaseAssetAsync(string address);
        bool IsAssetLoaded(string address);
    }
}
