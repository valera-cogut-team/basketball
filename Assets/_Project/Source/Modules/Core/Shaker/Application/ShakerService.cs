using System;
using LifeCycle.Facade;
using UnityEngine;

namespace Shaker
{
    public sealed class ShakerService : IShakerService, ILateUpdateHandler
    {
        private readonly ILifeCycleFacade _lifeCycle;
        private Transform _target;
        private Vector3 _velocity;
        private bool _bound;

        public ShakerService(ILifeCycleFacade lifeCycle)
        {
            _lifeCycle = lifeCycle ?? throw new ArgumentNullException(nameof(lifeCycle));
        }

        public float ImpulseScale { get; set; } = 0.05f;
        public float MaxVerticalKick { get; set; } = 0.035f;
        public float DecayPerSecond { get; set; } = 16f;

        public void SetTarget(Transform target) => _target = target;

        public void AddImpulse(float strength = 1f)
        {
            var s = Mathf.Max(0f, strength);
            _velocity += UnityEngine.Random.insideUnitSphere * (ImpulseScale * s);
            _velocity.y = Mathf.Min(_velocity.y, MaxVerticalKick);
        }

        public void AddImpulse(Vector3 worldDeltaVelocity)
        {
            _velocity += worldDeltaVelocity;
            _velocity.y = Mathf.Min(_velocity.y, MaxVerticalKick);
        }

        public void BindLifeCycle()
        {
            if (_bound)
                return;
            _lifeCycle.RegisterLateUpdateHandler(this);
            _bound = true;
        }

        public void UnbindLifeCycle()
        {
            if (!_bound)
                return;
            _lifeCycle.UnregisterLateUpdateHandler(this);
            _bound = false;
            _velocity = Vector3.zero;
        }

        public void OnLateUpdate(float deltaTime)
        {
            if (_target == null || _velocity.sqrMagnitude < 1e-8f)
                return;
            _target.position += _velocity;
            _velocity = Vector3.Lerp(_velocity, Vector3.zero, deltaTime * DecayPerSecond);
        }
    }
}
