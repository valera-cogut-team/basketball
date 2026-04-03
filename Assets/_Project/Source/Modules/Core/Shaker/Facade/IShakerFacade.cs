using UnityEngine;

namespace Shaker
{
    public interface IShakerFacade
    {
        float ImpulseScale { get; set; }
        float MaxVerticalKick { get; set; }
        float DecayPerSecond { get; set; }

        void SetTarget(Transform target);
        void AddImpulse(float strength = 1f);
        void AddImpulse(Vector3 worldDeltaVelocity);
    }
}
