using UnityEngine;
using LifeCycle.Facade;

namespace LifeCycle.Infrastructure
{
    public class LifeCycleMonoBehaviour : MonoBehaviour
    {
        private ILifeCycleFacade _facade;
        private float _lastUpdateTime;

        public void Initialize(ILifeCycleFacade facade)
        {
            _facade = facade;
            _lastUpdateTime = Time.time;
        }

        private void Update()
        {
            var t = Time.time;
            _facade?.TickUpdate(t - _lastUpdateTime);
            _lastUpdateTime = t;
        }

        private void LateUpdate() { _facade?.TickLateUpdate(Time.deltaTime); }
        private void FixedUpdate() { _facade?.TickFixedUpdate(Time.fixedDeltaTime); }
    }
}
