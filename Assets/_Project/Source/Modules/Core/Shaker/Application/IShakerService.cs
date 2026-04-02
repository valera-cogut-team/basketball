using UnityEngine;

namespace Shaker
{
    /// <summary>Damp position impulse on a target transform (e.g. camera), driven by LifeCycle LateUpdate.</summary>
    public interface IShakerService
    {
        void SetTarget(Transform target);
        void AddImpulse(float strength = 1f);
        void AddImpulse(Vector3 worldDeltaVelocity);

        /// <summary>Subscribe to LifeCycle LateUpdate (called from module Enable).</summary>
        void BindLifeCycle();

        /// <summary>Unsubscribe and clear velocity (called from module Disable).</summary>
        void UnbindLifeCycle();

        float ImpulseScale { get; set; }
        float MaxVerticalKick { get; set; }
        float DecayPerSecond { get; set; }
    }
}
