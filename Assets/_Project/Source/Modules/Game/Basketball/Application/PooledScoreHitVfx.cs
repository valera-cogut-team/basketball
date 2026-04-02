using Pool.Domain;
using UnityEngine;

namespace Basketball.Application
{
    /// <summary>Pooled instance of a score celebration hit VFX prefab (particle hierarchy).</summary>
    public sealed class PooledScoreHitVfx : IPoolable
    {
        public GameObject Instance { get; }

        public PooledScoreHitVfx(GameObject instance)
        {
            Instance = instance;
        }

        public void Reset()
        {
            if (Instance == null)
                return;
            foreach (var ps in Instance.GetComponentsInChildren<ParticleSystem>(true))
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Instance.SetActive(false);
        }
    }
}
