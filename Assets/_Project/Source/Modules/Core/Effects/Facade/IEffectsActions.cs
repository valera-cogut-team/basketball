using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Effects.Facade
{
    public interface IEffectsActions
    {
        UniTask<GameObject> SpawnAsync(string address, Vector3 worldPosition, Quaternion worldRotation,
            Transform parent = null);

        UniTask PlayOneShotAsync(string address, Vector3 worldPosition, Quaternion worldRotation, Transform parent = null,
            CancellationToken cancellationToken = default);

        void Despawn(GameObject instance);
    }
}
