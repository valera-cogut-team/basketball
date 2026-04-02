using Pool.Domain;
using UnityEngine;

namespace Basketball.Application
{
    /// <summary>Reusable scratch buffer for trajectory polyline (see <see cref="BasketballPoolIds.TrajectoryBuffer"/>).</summary>
    public sealed class PooledTrajectoryBuffer : IPoolable
    {
        public const int Capacity = 128;
        public readonly Vector3[] Points = new Vector3[Capacity];

        public void Reset()
        {
        }
    }
}
