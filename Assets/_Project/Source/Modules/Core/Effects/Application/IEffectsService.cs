using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Effects.Application
{
    /// <summary>Spawns prefab instances loaded via Addressables.</summary>
    public interface IEffectsService
    {
        UniTask<GameObject> SpawnAsync(string address, Vector3 worldPosition, Quaternion worldRotation, Transform parent = null);

        UniTask PlayOneShotAsync(string address, Vector3 worldPosition, Quaternion worldRotation, Transform parent = null,
            CancellationToken cancellationToken = default, float fallbackLifetimeSeconds = 2f);

        void Despawn(GameObject instance);

        void Shutdown();
    }
}
