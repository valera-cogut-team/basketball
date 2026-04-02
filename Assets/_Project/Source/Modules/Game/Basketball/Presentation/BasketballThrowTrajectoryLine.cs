using Basketball.Application;
using Basketball.Domain;
using Basketball.Facade;
using LifeCycle.Facade;
using Pool.Application;
using Pool.Facade;
using UnityEngine;

namespace Basketball.Presentation
{
    /// <summary>
    /// Draws a ballistic arc for the current held throw (optional via <see cref="BasketballTuningConfig.showThrowTrajectory"/>).
    /// Point buffer is borrowed from <see cref="IPoolFacade"/> pool <see cref="BasketballPoolIds.TrajectoryBuffer"/>.
    /// </summary>
    public sealed class BasketballThrowTrajectoryLine : MonoBehaviour, ILateUpdateHandler
    {
        private BasketballInteractionService _interaction;
        private IBasketballFacade _basketball;
        private BasketballTuningConfig _tuning;
        private IObjectPool<PooledTrajectoryBuffer> _bufferPool;
        private PooledTrajectoryBuffer _borrowed;
        private ILifeCycleFacade _lifeCycle;

        private LineRenderer _line;

        public void Initialize(
            BasketballInteractionService interaction,
            IBasketballFacade basketball,
            BasketballTuningConfig tuning,
            IPoolFacade poolFacade,
            ILifeCycleFacade lifeCycle)
        {
            _interaction = interaction;
            _basketball = basketball;
            _tuning = tuning ?? BasketballTuningConfig.CreateRuntimeDefault();
            _bufferPool = poolFacade?.GetPool<PooledTrajectoryBuffer>(BasketballPoolIds.TrajectoryBuffer);

            if (_lifeCycle != null)
                _lifeCycle.UnregisterLateUpdateHandler(this);
            _lifeCycle = lifeCycle;
            _lifeCycle?.RegisterLateUpdateHandler(this);

            var go = new GameObject("ThrowTrajectory");
            go.transform.SetParent(transform, false);
            _line = go.AddComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.numCornerVertices = 4;
            _line.numCapVertices = 3;
            _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _line.receiveShadows = false;
            _line.sortingOrder = 50;

            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null)
                sh = Shader.Find("Unlit/Color");
            if (sh == null)
                sh = Shader.Find("Sprites/Default");
            if (sh != null)
                _line.material = new Material(sh);

            _line.textureMode = LineTextureMode.Stretch;
        }

        public void OnLateUpdate(float deltaTime)
        {
            if (_line == null || _interaction == null || _basketball == null || _tuning == null)
                return;

            if (!_tuning.showThrowTrajectory || _basketball.Phase != BasketballBallPhase.Held)
            {
                _line.enabled = false;
                return;
            }

            if (!_interaction.TryGetTrajectoryPreview(out var origin, out var velocity))
            {
                _line.enabled = false;
                return;
            }

            var segments = Mathf.Min(_tuning.trajectoryPreviewSegments, PooledTrajectoryBuffer.Capacity - 1);
            var duration = _tuning.trajectoryPreviewDuration;
            if (!TryBorrowBuffer(out var points))
                return;

            var g = Physics.gravity;
            for (var i = 0; i <= segments; i++)
            {
                var t = duration * (i / (float)segments);
                points[i] = origin + velocity * t + 0.5f * g * (t * t);
            }

            _line.enabled = true;
            _line.positionCount = segments + 1;
            _line.SetPositions(points);
            _line.startWidth = _tuning.trajectoryLineWidth;
            _line.endWidth = _tuning.trajectoryLineWidth * 0.65f;
            var charge = _basketball.AimCharge01;
            _line.startColor = Color.Lerp(_tuning.trajectoryColor, _tuning.trajectoryColorMaxCharge, charge);
            _line.endColor = new Color(_line.startColor.r, _line.startColor.g, _line.startColor.b, _line.startColor.a * 0.35f);
        }

        private bool TryBorrowBuffer(out Vector3[] points)
        {
            points = null;
            if (_bufferPool == null)
                return false;
            if (_borrowed == null)
                _borrowed = _bufferPool.Get();
            if (_borrowed == null)
                return false;
            points = _borrowed.Points;
            return true;
        }

        private void OnDestroy()
        {
            _lifeCycle?.UnregisterLateUpdateHandler(this);
            _lifeCycle = null;
            if (_borrowed != null && _bufferPool != null)
            {
                _bufferPool.Return(_borrowed);
                _borrowed = null;
            }
        }
    }
}
