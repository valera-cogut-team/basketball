using Basketball.Domain;
using Basketball.Facade;
using Input.Facade;
using LifeCycle.Facade;
using UniRx;
using UnityEngine;

namespace Basketball.Application
{
    /// <summary>
    /// Per-frame pickup / aim / throw rules; presentation only feeds transforms and physics.
    /// </summary>
    public sealed class BasketballInteractionService : IUpdateHandler
    {
        private readonly IInputFacade _input;
        private readonly IBasketballFacade _basketball;
        private readonly BasketballTuningConfig _tuning;

        private Transform _camTx;
        private Rigidbody _ballBody;
        private Transform _ballTx;
        private Vector3 _ballSpawn;
        private float _yaw;
        private float _pitch = 10f;
        private float _dragStartX;
        private float _dragStartY;
        private bool _bound;
        private IBasketballGoalSequenceReset _goalSequenceReset;
        /// <summary>Unscaled time when to move ball to spawn after a score; negative if none pending.</summary>
        private float _scoreRespawnDeadline = -1f;
        /// <summary>Unscaled time when to move ball to spawn after any throw; negative if none pending.</summary>
        private float _throwRespawnDeadline = -1f;
        /// <summary>Unscaled time when score rotation shake ends; negative if inactive.</summary>
        private float _scoreRotShakeEndUnscaled = -1f;

        private readonly CompositeDisposable _scoreSubscription = new CompositeDisposable();

        public BasketballInteractionService(IInputFacade input, IBasketballFacade basketball, BasketballTuningConfig tuning)
        {
            _input = input;
            _basketball = basketball;
            _tuning = tuning ?? BasketballTuningConfig.CreateRuntimeDefault();
            _basketball?.ScoreRx.Subscribe(OnScoreChanged).AddTo(_scoreSubscription);
        }

        public void Teardown()
        {
            _scoreSubscription.Clear();
            _goalSequenceReset = null;
            _scoreRespawnDeadline = -1f;
            _throwRespawnDeadline = -1f;
            _scoreRotShakeEndUnscaled = -1f;
        }

        /// <summary>Light ping-pong rotation on the camera for ~duration; does not move position.</summary>
        public void TriggerScoreCameraRotationShake()
        {
            if (!_bound || _tuning == null || _camTx == null)
                return;
            var d = Mathf.Max(0.02f, _tuning.scoreCameraRotShakeDurationSeconds);
            _scoreRotShakeEndUnscaled = Time.unscaledTime + d;
        }

        private void OnScoreChanged(int score)
        {
            if (score <= 0)
            {
                _scoreRespawnDeadline = -1f;
                _throwRespawnDeadline = -1f;
                return;
            }

            var delay = _tuning.respawnBallAfterScoreSeconds;
            if (delay <= 0f)
            {
                _scoreRespawnDeadline = -1f;
                RespawnBallToInitialSpawn();
                return;
            }

            _scoreRespawnDeadline = Time.unscaledTime + delay;
        }

        public void Bind(Transform camera, Rigidbody ballBody, Transform ballTransform, Vector3 ballSpawn, float yaw, float pitch,
            IBasketballGoalSequenceReset goalSequenceReset = null)
        {
            _camTx = camera;
            _ballBody = ballBody;
            _ballTx = ballTransform;
            _ballSpawn = ballSpawn;
            _yaw = yaw;
            _pitch = pitch;
            _goalSequenceReset = goalSequenceReset;
            _bound = camera != null && ballBody != null && ballTransform != null;
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_bound || _input == null || _camTx == null || _ballBody == null)
                return;

            Look();
            MoveCamera(deltaTime);
            ApplyCameraWorldRotation();
            ProcessScheduledRespawns();
            HandleBall();
        }

        private void Look()
        {
            if (!_input.GetButton("Fire2"))
                return;
            var mx = _input.GetAxis("Mouse X");
            var my = _input.GetAxis("Mouse Y");
            _yaw += mx * _tuning.mouseSensitivity;
            _pitch -= my * _tuning.mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, -35f, 58f);
        }

        private void ApplyCameraWorldRotation()
        {
            if (_camTx == null)
                return;
            EvaluateScoreRotationShake(out var dp, out var dy);
            _camTx.rotation = Quaternion.Euler(_pitch + dp, _yaw + dy, 0f);
        }

        private void EvaluateScoreRotationShake(out float dPitch, out float dYaw)
        {
            dPitch = dYaw = 0f;
            if (_scoreRotShakeEndUnscaled < 0f || _tuning == null)
                return;
            var now = Time.unscaledTime;
            if (now >= _scoreRotShakeEndUnscaled)
            {
                _scoreRotShakeEndUnscaled = -1f;
                return;
            }

            var dur = Mathf.Max(0.02f, _tuning.scoreCameraRotShakeDurationSeconds);
            var u = Mathf.Clamp01(1f - (_scoreRotShakeEndUnscaled - now) / dur);
            var cycles = Mathf.Max(0.5f, _tuning.scoreCameraRotShakeCycles);
            var phase = 2f * Mathf.PI * cycles * u;
            var soft = Mathf.Sin(Mathf.PI * u);
            dPitch = _tuning.scoreCameraRotShakePitchAmplitude * Mathf.Sin(phase) * soft;
            dYaw = _tuning.scoreCameraRotShakeYawAmplitude * Mathf.Cos(phase) * soft;
        }

        private void MoveCamera(float deltaTime)
        {
            var planar = Quaternion.Euler(0f, _yaw, 0f);
            var h = _input.GetAxis("Horizontal");
            var v = _input.GetAxis("Vertical");
            var move = planar * Vector3.right * h + planar * Vector3.forward * v;
            if (move.sqrMagnitude > 0.01f)
                _camTx.position += move.normalized * (_tuning.cameraMoveSpeed * deltaTime);
        }

        private void HandleBall()
        {
            var cam = _camTx;
            var phase = _basketball.Phase;
            var pickupNear = Vector3.Distance(cam.position, _ballTx.position) < _tuning.pickupRadius;

            if (phase == BasketballBallPhase.Free || phase == BasketballBallPhase.InFlight)
            {
                if (pickupNear && _input.GetButtonDown("Fire1"))
                {
                    _scoreRespawnDeadline = -1f;
                    _throwRespawnDeadline = -1f;
                    _basketball.SetPhase(BasketballBallPhase.Held);
                    _ballBody.isKinematic = false;
                    _ballBody.linearVelocity = Vector3.zero;
                    _ballBody.angularVelocity = Vector3.zero;
                    _ballBody.isKinematic = true;
                    _input.GetPointerPosition(out _dragStartX, out _dragStartY);
                }
            }

            if (_basketball.Phase == BasketballBallPhase.Held)
            {
                _ballTx.position = cam.position + cam.forward * _tuning.holdDistance;

                var held = _input.GetButton("Fire1");
                if (held)
                {
                    TryComputeSwipeVelocityFromDragEnd(out _, out var previewCharge);
                    _basketball.SetAimCharge(previewCharge);
                }

                if (_input.GetButtonUp("Fire1"))
                {
                    TryComputeSwipeVelocityFromDragEnd(out var vel, out _);
                    _ballBody.isKinematic = false;
                    _ballBody.linearVelocity = vel;
                    _ballBody.angularVelocity = Vector3.zero;
                    _basketball.SetPhase(BasketballBallPhase.InFlight);
                    _basketball.SetAimCharge(0f);
                    var throwDelay = _tuning.respawnBallAfterThrowSeconds;
                    if (throwDelay > 0f)
                        _throwRespawnDeadline = Time.unscaledTime + throwDelay;
                    else
                        _throwRespawnDeadline = -1f;
                }
            }

            if (_ballTx.position.y < -2f || _ballTx.position.magnitude > 120f)
                ResetBall();
        }

        private float EvaluateChargeWeight(float normalized01)
        {
            var curve = _tuning.throwChargeWeight;
            if (curve == null || curve.length == 0)
                return Mathf.Clamp01(normalized01);
            return Mathf.Clamp01(curve.Evaluate(Mathf.Clamp01(normalized01)));
        }

        private void GetPointerDeltaFromDragStart(out float dx, out float dy)
        {
            _input.GetPointerPosition(out var px, out var py);
            dx = px - _dragStartX;
            dy = py - _dragStartY;
        }

        private float ScreenHeight => Mathf.Max(Screen.height, 1);

        /// <summary>
        /// Vertical swipe → speed and arc height; horizontal swipe → yaw around world up (left/right).
        /// </summary>
        private void TryComputeSwipeVelocityFromDragEnd(out Vector3 velocity, out float charge01)
        {
            GetPointerDeltaFromDragStart(out var dx, out var dy);
            var invH = 1f / ScreenHeight;
            var nx = dx * invH;
            var ny = dy * invH;
            var mag = Mathf.Sqrt(nx * nx + ny * ny);

            var baseDir = ComputeThrowDirection();

            if (mag < _tuning.swipeDeadzoneScreenFraction)
            {
                charge01 = 0f;
                velocity = baseDir * _tuning.minThrowForce;
                return;
            }

            var up = Mathf.Max(0f, ny);
            var wRawVert = up <= 0f
                ? 0f
                : Mathf.InverseLerp(_tuning.swipeMinScreenFraction, _tuning.swipeFullPowerScreenFraction, up);
            var gamma = Mathf.Max(0.5f, _tuning.swipeStrengthGamma);
            charge01 = EvaluateChargeWeight(Mathf.Pow(Mathf.Clamp01(wRawVert), gamma));

            var yaw = Mathf.Clamp(nx, -1.15f, 1.15f) * _tuning.swipeHorizontalMaxYawDegrees;
            var dirYawed = Quaternion.AngleAxis(yaw, Vector3.up) * baseDir;
            var vert01 = Mathf.Clamp01(wRawVert);
            var dir = (dirYawed + Vector3.up * (_tuning.swipeVerticalArcLift * vert01)).normalized;

            var speed = Mathf.Lerp(_tuning.minThrowForce, _tuning.maxThrowForce, charge01);
            velocity = dir * speed;
        }

        /// <summary>Ballistic initial velocity matching the next throw while the ball is held (for preview).</summary>
        public bool TryGetTrajectoryPreview(out Vector3 origin, out Vector3 velocity)
        {
            origin = default;
            velocity = default;
            if (!_bound || _ballTx == null || _camTx == null)
                return false;
            if (_basketball.Phase != BasketballBallPhase.Held)
                return false;

            origin = _ballTx.position;
            if (!_input.GetButton("Fire1"))
            {
                velocity = ComputeThrowDirection() * _tuning.minThrowForce;
                return true;
            }

            TryComputeSwipeVelocityFromDragEnd(out velocity, out _);
            return true;
        }

        private Vector3 ComputeThrowDirection()
        {
            var f = _camTx.forward;
            var planar = Vector3.ProjectOnPlane(f, Vector3.up);
            if (planar.sqrMagnitude < 1e-6f)
                planar = Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward;
            planar.Normalize();
            var blended = Vector3.Lerp(f, planar, _tuning.throwPlanarForwardBlend);
            if (blended.sqrMagnitude < 1e-6f)
                blended = f;
            blended.Normalize();
            var dir = blended + Vector3.up * _tuning.throwVerticalLift;
            return dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.forward;
        }

        private void ProcessScheduledRespawns()
        {
            var now = Time.unscaledTime;
            var scoreDue = _scoreRespawnDeadline >= 0f && now >= _scoreRespawnDeadline;
            var throwDue = _throwRespawnDeadline >= 0f && now >= _throwRespawnDeadline;
            if (!scoreDue && !throwDue)
                return;
            RespawnBallToInitialSpawn();
        }

        private void RespawnBallToInitialSpawn()
        {
            if (!_bound || _ballBody == null || _ballTx == null || _basketball == null || _camTx == null)
                return;

            _scoreRespawnDeadline = -1f;
            _throwRespawnDeadline = -1f;
            _goalSequenceReset?.ResetGoalSequence();

            _ballBody.isKinematic = false;
            _ballTx.position = _tuning.ResolveBallSpawnPosition(_camTx, _ballSpawn);
            _ballBody.linearVelocity = Vector3.zero;
            _ballBody.angularVelocity = Vector3.zero;
            _basketball.SetPhase(BasketballBallPhase.Free);
        }

        private void ResetBall()
        {
            RespawnBallToInitialSpawn();
        }
    }
}
