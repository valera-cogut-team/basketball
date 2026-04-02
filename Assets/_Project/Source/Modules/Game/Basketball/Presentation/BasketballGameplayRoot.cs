using System;
using Addressables.Facade;
using Basketball.Application;
using Pool.Facade;
using Basketball.Facade;
using Cysharp.Threading.Tasks;
using Logger.Facade;
using LifeCycle.Facade;
using Audio.Facade;
using Effects.Facade;
using UnityEngine;

namespace Basketball.Presentation
{
    /// <summary>
    /// Loads court and ball prefabs via Addressables, instantiates under this root, binds hoop <see cref="BasketballHoopScoreGate"/> from the playfield prefab.
    /// </summary>
    public sealed class BasketballGameplayRoot : MonoBehaviour
    {
        private BasketballInteractionService _interaction;
        private IBasketballFacade _basketball;
        private ILoggerFacade _logger;
        private BasketballTuningConfig _tuning;
        private IAddressablesFacade _addressables;
        private IPoolFacade _pool;
        private IAudioFacade _audio;
        private IEffectsFacade _effects;
        private ILifeCycleFacade _lifeCycle;

        private Rigidbody _ballBody;
        private Transform _ballTx;
        private Transform _camTx;
        private Vector3 _ballSpawn;
        private float _yaw;
        private float _pitch = 10f;

        /// <summary>Optional VFX/spawns from Addressables (null if Effects module disabled).</summary>
        public IEffectsFacade Effects => _effects;

        public void Initialize(
            BasketballInteractionService interaction,
            IBasketballFacade basketball,
            ILoggerFacade logger,
            BasketballTuningConfig tuning,
            IAddressablesFacade addressables,
            IPoolFacade pool,
            IAudioFacade audio,
            IEffectsFacade effects,
            ILifeCycleFacade lifeCycle)
        {
            _interaction = interaction;
            _basketball = basketball;
            _logger = logger;
            _tuning = tuning ?? BasketballTuningConfig.CreateRuntimeDefault();
            _addressables = addressables;
            _pool = pool;
            _audio = audio;
            _effects = effects;
            _lifeCycle = lifeCycle;
            BuildWorldAsync().Forget();
        }

        private async UniTaskVoid BuildWorldAsync()
        {
            if (_addressables == null)
            {
                _logger?.LogError("[Basketball] IAddressablesFacade is null.");
                return;
            }

            if (string.IsNullOrEmpty(_tuning.playFieldAddress) || string.IsNullOrEmpty(_tuning.ballAddress))
            {
                _logger?.LogError("[Basketball] Set playFieldAddress and ballAddress on BasketballTuning.");
                return;
            }

            GameObject fieldPrefab;
            GameObject ballPrefabTemplate;
            try
            {
                fieldPrefab = await _addressables.LoadPrefabAsync(_tuning.playFieldAddress);
                ballPrefabTemplate = await _addressables.LoadPrefabAsync(_tuning.ballAddress);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[Basketball] Addressables load failed: {ex.Message}", ex);
                return;
            }

            if (fieldPrefab == null || ballPrefabTemplate == null)
            {
                _logger?.LogError("[Basketball] Loaded prefab reference is null.");
                return;
            }

            Physics.gravity = new Vector3(0f, -9.81f, 0f);

            var field = UnityEngine.Object.Instantiate(fieldPrefab, transform);
            field.name = "PlayField (instance)";
            field.transform.localPosition = Vector3.zero;
            field.transform.localRotation = Quaternion.identity;
            field.transform.localScale = Vector3.one;

            SetupCameraForPlayField(field);

            var fixedSpawn = ResolveFixedBallSpawn(field.transform);
            var spawn = _tuning.ResolveBallSpawnPosition(_camTx, fixedSpawn);

            var ballGo = UnityEngine.Object.Instantiate(ballPrefabTemplate, spawn, Quaternion.identity, transform);
            ballGo.name = "Ball (instance)";

            try
            {
                await _addressables.ReleaseAssetAsync(_tuning.playFieldAddress);
                await _addressables.ReleaseAssetAsync(_tuning.ballAddress);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"[Basketball] Release prefab handles: {ex.Message}");
            }

            _ballSpawn = fixedSpawn;
            _ballTx = ballGo.transform;
            _ballBody = ballGo.GetComponent<Rigidbody>();
            if (_ballBody == null)
                _ballBody = ballGo.AddComponent<Rigidbody>();
            ApplyBallPhysicsFromTuning();

            var sphereCol = ballGo.GetComponent<SphereCollider>();
            if (sphereCol != null)
                sphereCol.material = RubberBall();

            var hoopWorld = ResolveHoopAxisWorld(field.transform);
            var hoopGate = SetupHoopGoalScoring(field);

            _interaction.Bind(_camTx, _ballBody, _ballTx, _ballSpawn, _yaw, _pitch, hoopGate);

            var trajectory = gameObject.AddComponent<BasketballThrowTrajectoryLine>();
            trajectory.Initialize(_interaction, _basketball, _tuning, _pool, _lifeCycle);

            var hud = gameObject.AddComponent<BasketballHud>();
            hud.Initialize(_basketball);

            var feedback = gameObject.AddComponent<BasketballScoreFeedback>();
            feedback.Initialize(_basketball, _audio, _interaction, _addressables, _logger, _pool, hoopWorld,
                Quaternion.identity);
        }

        private Vector3 ResolveFixedBallSpawn(Transform playFieldRoot)
        {
            if (!string.IsNullOrEmpty(_tuning.ballSpawnChildName))
            {
                var spawnTx = FindChildRecursive(playFieldRoot, _tuning.ballSpawnChildName);
                if (spawnTx != null)
                    return spawnTx.position;
            }

            return _tuning.ballSpawnWorldPosition;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child.name == childName)
                    return child;
                var found = FindChildRecursive(child, childName);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>World-space point along the net (prefer <c>Net</c> under <c>Ring</c>) used as the “down the net” axis target.</summary>
        private static Vector3 ResolveHoopAxisWorld(Transform playFieldRoot)
        {
            var ring = FindChildRecursive(playFieldRoot, "Ring");
            if (ring != null)
            {
                foreach (var t in ring.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.IndexOf("net", StringComparison.OrdinalIgnoreCase) >= 0)
                        return t.position;
                }

                return ring.position;
            }

            return playFieldRoot.position;
        }

        /// <summary>Binds <see cref="BasketballHoopScoreGate"/> from PlayField prefab (triggers + <see cref="BasketballHoopScoreTriggerZone"/> are authoring-time).</summary>
        private BasketballHoopScoreGate SetupHoopGoalScoring(GameObject field)
        {
            var gate = field.GetComponentInChildren<BasketballHoopScoreGate>(true);
            if (gate == null)
            {
                _logger?.LogWarning(
                    "[Basketball] No BasketballHoopScoreGate on PlayField — add it under the hoop (see PlayField prefab).");
                return null;
            }

            gate.Initialize(_basketball, _logger, _ballBody, _lifeCycle);
            return gate;
        }

        private void ApplyBallPhysicsFromTuning()
        {
            _ballBody.mass = _tuning.ballMass;
            _ballBody.linearDamping = _tuning.ballLinearDamping;
            _ballBody.angularDamping = _tuning.ballAngularDamping;
            _ballBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        /// <summary>Uses the camera on the court prefab; does not spawn a fallback camera.</summary>
        private void SetupCameraForPlayField(GameObject field)
        {
            var courtCam = field.GetComponentInChildren<Camera>(true);
            if (courtCam != null)
            {
                _camTx = courtCam.transform;
                _yaw = _camTx.eulerAngles.y;
                _pitch = _camTx.eulerAngles.x;
                if (_pitch > 90f) _pitch -= 360f;
                return;
            }

            if (Camera.main != null)
            {
                _camTx = Camera.main.transform;
                _yaw = _camTx.eulerAngles.y;
                _pitch = _camTx.eulerAngles.x;
                if (_pitch > 90f) _pitch -= 360f;
                return;
            }

            _logger?.LogError(
                "[Basketball] No Camera under PlayField and no Camera.main — add a Camera to the PlayField prefab (or tag one as MainCamera).");
        }

        private PhysicsMaterial RubberBall()
        {
            var m = new PhysicsMaterial("BallRubber")
            {
                bounciness = _tuning.ballBounciness,
                dynamicFriction = 0.45f,
                staticFriction = 0.52f,
                frictionCombine = PhysicsMaterialCombine.Average,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };
            return m;
        }
    }
}
