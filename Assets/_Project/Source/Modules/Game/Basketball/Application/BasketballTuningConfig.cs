using UnityEngine;

namespace Basketball.Application
{
    /// <summary>
    /// Designer-tunable gameplay parameters (rules, throw curve, physics, score volume).
    /// </summary>
    [CreateAssetMenu(fileName = "BasketballTuning", menuName = "Basketball/Tuning Config")]
    public sealed class BasketballTuningConfig : ScriptableObject
    {
        [Header("Rules")]
        [Min(0.05f)] public float scoreCooldownSeconds = 0.65f;
        [Tooltip("After a made basket, return the ball to the initial spawn point after this delay (unscaled time). 0 = on the next frame.")]
        [Min(0f)] public float respawnBallAfterScoreSeconds = 1.6f;
        [Tooltip("After every throw (basket or miss), return the ball to spawn after this unscaled delay so you don't have to walk to it. 0 = off.")]
        [Min(0f)] public float respawnBallAfterThrowSeconds = 4f;
        [Min(0.1f)] public float pickupRadius = 2.9f;
        [Tooltip("Unused (swipe controls power). Kept for older serialized assets.")]
        [Min(0.05f)] public float maxHoldChargeSeconds = 1.25f;
        [Min(0f)] public float minThrowForce = 5.5f;
        [Min(0f)] public float maxThrowForce = 18f;

        [Header("Throw curve")]
        [Tooltip("Maps normalized swipe strength (0–1) to throw weight before min/max force.")]
        public AnimationCurve throwChargeWeight = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [Tooltip("Blend camera forward with horizontal (XZ) forward — lower = more arc (follows camera pitch); too high = flat line drive into backboard.")]
        [Range(0f, 1f)] public float throwPlanarForwardBlend = 0.32f;
        [Tooltip("Extra world-up added before normalize; higher = higher lob / parabola.")]
        [Min(0f)] public float throwVerticalLift = 0.48f;

        [Header("Swipe throw (mobile-style)")]
        [Tooltip("Vertical swipe (up, fraction of screen height) for max throw speed / power. Larger = longer upward drag for full power.")]
        [Min(0.05f)] public float swipeFullPowerScreenFraction = 0.52f;
        [Tooltip("Minimum useful vertical swipe for scaling (same units as full power).")]
        [Min(0.001f)] public float swipeMinScreenFraction = 0.01f;
        [Tooltip("Exponent on normalized vertical swipe before throwChargeWeight (higher = softer weak flicks).")]
        [Min(0.5f)] public float swipeStrengthGamma = 1.65f;
        [Tooltip("If total swipe is below this (height-normalized magnitude), treat as tap along base aim.")]
        [Min(0f)] public float swipeDeadzoneScreenFraction = 0.006f;
        [Tooltip("Yaw (degrees) when horizontal swipe equals one screen height (dx/h). Clamped internally.")]
        [Min(0f)] public float swipeHorizontalMaxYawDegrees = 38f;
        [Tooltip("Extra world-up mixed into aim from vertical swipe (0–1 strength); higher = taller arc for the same horizontal aim.")]
        [Min(0f)] public float swipeVerticalArcLift = 0.42f;

        [Header("Throw aim preview")]
        public bool showThrowTrajectory = true;
        [Min(8)] public int trajectoryPreviewSegments = 40;
        [Tooltip("How many seconds of flight to draw (ballistic, gravity only).")]
        [Min(0.15f)] public float trajectoryPreviewDuration = 1.65f;
        [Min(0.002f)] public float trajectoryLineWidth = 0.035f;
        public Color trajectoryColor = new Color(1f, 0.92f, 0.35f, 0.72f);
        public Color trajectoryColorMaxCharge = new Color(1f, 0.45f, 0.12f, 0.85f);

        [Header("Camera / carry")]
        [Min(0.1f)] public float holdDistance = 2.35f;
        [Min(0f)] public float cameraMoveSpeed = 6f;
        [Min(0f)] public float mouseSensitivity = 2.8f;

        [Header("Score feedback (camera rotation)")]
        [Tooltip("Made basket: light rotation wobble duration (unscaled). Position is unchanged; rotation returns to aim (yaw/pitch).")]
        [Min(0.02f)] public float scoreCameraRotShakeDurationSeconds = 0.36f;
        [Tooltip("Peak pitch offset in degrees (code also applies a soft fade at start/end).")]
        [Min(0f)] public float scoreCameraRotShakePitchAmplitude = 0.4f;
        [Tooltip("Peak yaw offset in degrees (quarter-phase from pitch).")]
        [Min(0f)] public float scoreCameraRotShakeYawAmplitude = 0.28f;
        [Tooltip("Sine cycles over the duration. 0.5 = one gentle lobe; 1 = full wave. Higher = busier.")]
        [Min(0.5f)] public float scoreCameraRotShakeCycles = 0.5f;

        [Header("Ball spawn / respawn")]
        [Tooltip("Place the ball in front of the camera at start and after out-of-bounds reset (no walking to pick up).")]
        public bool spawnBallInFrontOfCamera = true;
        [Tooltip("Distance along blended forward from camera.")]
        [Min(0.2f)] public float ballSpawnForwardDistance = 2.15f;
        [Tooltip("Added to camera Y (negative = lower, near floor from eye height).")]
        public float ballSpawnHeightOffset = -1.05f;
        [Tooltip("Blend forward with horizontal forward — higher keeps the ball on the court plane when looking down.")]
        [Range(0f, 1f)] public float ballSpawnPlanarForwardBlend = 0.88f;

        [Header("Legacy score checks (unused with two-trigger scoring in prefab)")]
        [Min(0f)] public float scoreMinVelocitySqr = 0.02f;
        [Tooltip("Unused when hoop uses upper+lower triggers.")]
        [Range(0f, 1f)] public float scoreDownwardDot = 0.28f;
        [Tooltip("Unused when hoop uses upper+lower triggers.")]
        [Min(0.05f)] public float scoreOpeningHorizontalRadius = 0.22f;

        [Header("Ball / materials")]
        [Min(0.01f)] public float ballMass = 0.58f;
        [Min(0f)] public float ballLinearDamping = 0.18f;
        [Min(0f)] public float ballAngularDamping = 0.08f;
        [Range(0f, 1f)] public float ballBounciness = 0.48f;
        [Range(0f, 1f)] public float rimBounciness = 0.35f;
        [Range(0f, 1f)] public float floorBounciness = 0.08f;

        [Header("Addressables")]
        [Tooltip("Address of PlayField prefab (default local Addressables group).")]
        public string playFieldAddress = BasketballAddressKeys.PlayField;
        [Tooltip("Address of Ball prefab.")]
        public string ballAddress = BasketballAddressKeys.Ball;
        [Tooltip("World-space ball spawn when play field has no BallSpawn child.")]
        public Vector3 ballSpawnWorldPosition = new Vector3(0f, 1.35f, -3.5f);
        [Tooltip("If set, looks under PlayField for this child transform and uses its position as ball spawn.")]
        public string ballSpawnChildName = "BallSpawn";

        /// <summary>Runtime fallback when no asset is assigned (audition / tests).</summary>
        public static BasketballTuningConfig CreateRuntimeDefault()
        {
            var c = CreateInstance<BasketballTuningConfig>();
            c.hideFlags = HideFlags.HideAndDontSave;
            if (c.throwChargeWeight == null || c.throwChargeWeight.keys.Length == 0)
                c.throwChargeWeight = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            c.throwPlanarForwardBlend = 0.32f;
            c.throwVerticalLift = 0.48f;
            c.swipeFullPowerScreenFraction = 0.52f;
            c.swipeMinScreenFraction = 0.01f;
            c.swipeStrengthGamma = 1.65f;
            c.swipeDeadzoneScreenFraction = 0.006f;
            c.swipeHorizontalMaxYawDegrees = 38f;
            c.swipeVerticalArcLift = 0.42f;
            c.showThrowTrajectory = true;
            c.trajectoryPreviewSegments = 40;
            c.trajectoryPreviewDuration = 1.65f;
            c.trajectoryLineWidth = 0.035f;
            c.trajectoryColor = new Color(1f, 0.92f, 0.35f, 0.72f);
            c.trajectoryColorMaxCharge = new Color(1f, 0.45f, 0.12f, 0.85f);
            c.pickupRadius = 2.9f;
            c.respawnBallAfterScoreSeconds = 1.6f;
            c.respawnBallAfterThrowSeconds = 4f;
            c.spawnBallInFrontOfCamera = true;
            c.ballSpawnForwardDistance = 2.15f;
            c.ballSpawnHeightOffset = -1.05f;
            c.ballSpawnPlanarForwardBlend = 0.88f;
            c.scoreMinVelocitySqr = 0.02f;
            c.scoreDownwardDot = 0.28f;
            c.scoreOpeningHorizontalRadius = 0.22f;
            c.scoreCameraRotShakeDurationSeconds = 0.36f;
            c.scoreCameraRotShakePitchAmplitude = 0.4f;
            c.scoreCameraRotShakeYawAmplitude = 0.28f;
            c.scoreCameraRotShakeCycles = 0.5f;
            return c;
        }

        /// <summary>Spawn point: in front of camera or fixed level-design position.</summary>
        public Vector3 ResolveBallSpawnPosition(Transform camera, Vector3 fixedLevelSpawn)
        {
            if (spawnBallInFrontOfCamera && camera != null)
                return GetBallSpawnInFrontOfCamera(camera);
            return fixedLevelSpawn;
        }

        /// <summary>Position in front of the camera (planar blend + height offset).</summary>
        public Vector3 GetBallSpawnInFrontOfCamera(Transform camera)
        {
            if (camera == null)
                return Vector3.zero;

            var f = camera.forward;
            var planar = Vector3.ProjectOnPlane(f, Vector3.up);
            if (planar.sqrMagnitude < 1e-5f)
                planar = Quaternion.Euler(0f, camera.eulerAngles.y, 0f) * Vector3.forward;
            planar.Normalize();
            var dir = Vector3.Lerp(f, planar, ballSpawnPlanarForwardBlend);
            if (dir.sqrMagnitude < 1e-5f)
                dir = f;
            dir.Normalize();

            return camera.position + dir * ballSpawnForwardDistance + Vector3.up * ballSpawnHeightOffset;
        }
    }
}
