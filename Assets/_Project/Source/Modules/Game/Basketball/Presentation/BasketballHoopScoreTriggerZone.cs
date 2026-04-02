using UnityEngine;

namespace Basketball.Presentation
{
    /// <summary>
    /// Hang on each prefab trigger collider (upper / lower). Parent should carry <see cref="BasketballHoopScoreGate"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BasketballHoopScoreTriggerZone : MonoBehaviour
    {
        [SerializeField] private BasketballHoopScoreGate _gate;
        [SerializeField] private bool _isUpper;

        private void Awake()
        {
            if (_gate == null)
                _gate = GetComponentInParent<BasketballHoopScoreGate>();
        }

        private void OnTriggerEnter(Collider other) => _gate?.NotifyZoneEnter(_isUpper, other);
    }
}
