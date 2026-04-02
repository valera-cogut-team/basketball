using System;
using UnityEngine;

namespace Shaker
{
    public sealed class ShakerFacade : IShakerFacade
    {
        private readonly IShakerService _service;

        public ShakerFacade(IShakerService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public void SetTarget(Transform target) => _service.SetTarget(target);

        public void AddImpulse(float strength = 1f) => _service.AddImpulse(strength);

        public void AddImpulse(Vector3 worldDeltaVelocity) => _service.AddImpulse(worldDeltaVelocity);

        public float ImpulseScale
        {
            get => _service.ImpulseScale;
            set => _service.ImpulseScale = value;
        }

        public float MaxVerticalKick
        {
            get => _service.MaxVerticalKick;
            set => _service.MaxVerticalKick = value;
        }

        public float DecayPerSecond
        {
            get => _service.DecayPerSecond;
            set => _service.DecayPerSecond = value;
        }
    }
}
