using UnityEngine;

namespace Shaker
{
    public interface IShakerActions
    {
        void SetTarget(Transform target);
        void AddImpulse(float strength = 1f);
        void AddImpulse(Vector3 worldDeltaVelocity);
    }
}
