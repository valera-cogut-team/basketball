using Basketball.Application;
using Basketball.Facade;
using Logger.Facade;
using LifeCycle.Facade;
using UnityEngine;

namespace Basketball.Presentation
{
    /// <summary>
    /// Counts a basket when the ball enters the <b>upper</b> hoop trigger, then the <b>lower</b> net trigger (see prefab + <see cref="BasketballHoopScoreTriggerZone"/>).
    /// Flush runs on the next fixed tick via <see cref="ILifeCycleFacade"/>.
    /// </summary>
    public sealed class BasketballHoopScoreGate : MonoBehaviour, IBasketballGoalSequenceReset, IFixedUpdateHandler
    {
        private const string LogTag = "[Basketball][HoopGate]";

        private IBasketballFacade _basketball;
        private ILoggerFacade _logger;
        private ILifeCycleFacade _lifeCycle;
        private bool _lifecycleRegistered;
        private Rigidbody _ballBody;

        private bool _upperThisStep;
        private bool _lowerThisStep;
        private bool _latchedUpper;
        private bool _flushQueued;

        public void Initialize(IBasketballFacade basketball, ILoggerFacade logger, Rigidbody ballBody, ILifeCycleFacade lifeCycle)
        {
            if (_lifecycleRegistered && _lifeCycle != null)
                _lifeCycle.UnregisterFixedUpdateHandler(this);
            _lifecycleRegistered = false;

            _basketball = basketball;
            _logger = logger;
            _ballBody = ballBody;
            _lifeCycle = lifeCycle;

            if (_lifeCycle != null)
            {
                _lifeCycle.RegisterFixedUpdateHandler(this);
                _lifecycleRegistered = true;
            }
            else
                _logger?.LogError($"{LogTag} ILifeCycleFacade is null; hoop score flush is disabled.");

            _logger?.LogInfo($"{LogTag} Gate bound on '{gameObject.name}' ballId={ballBody?.GetInstanceID()} ballLayer={ballBody?.gameObject.layer}");
        }

        private void OnDestroy()
        {
            if (_lifecycleRegistered && _lifeCycle != null)
                _lifeCycle.UnregisterFixedUpdateHandler(this);
            _lifecycleRegistered = false;
        }

        public void ResetGoalSequence()
        {
            _logger?.LogDebug($"{LogTag} ResetGoalSequence (state cleared).");
            _flushQueued = false;
            _upperThisStep = false;
            _lowerThisStep = false;
            _latchedUpper = false;
        }

        internal void NotifyZoneEnter(bool isUpper, Collider other)
        {
            var zone = isUpper ? "UPPER" : "LOWER";
            if (other == null)
            {
                _logger?.LogWarning($"{LogTag} OnTriggerEnter {zone}: collider is null.");
                return;
            }

            _logger?.LogInfo($"{LogTag} {zone} OnTriggerEnter (raw): collider='{other.name}' root='{other.transform.root.name}' layer={other.gameObject.layer} attachRb={(other.attachedRigidbody != null ? other.attachedRigidbody.name : "null")}");

            if (_basketball == null || _ballBody == null)
            {
                _logger?.LogWarning($"{LogTag} OnTriggerEnter {zone}: gate not ready (basketball={_basketball != null}, ballBody={_ballBody != null}).");
                return;
            }

            if (!TryResolveBallRb(other, out var rb, out var rejectReason))
            {
                _logger?.LogInfo($"{LogTag} {zone}: not our ball — {rejectReason}");
                return;
            }

            if (isUpper)
            {
                _latchedUpper = true;
                _upperThisStep = true;
                _logger?.LogInfo($"{LogTag} Upper trigger HIT our ball. Latched upper; queue flush. rb='{rb.name}' pos={rb.position}");
            }
            else
            {
                _lowerThisStep = true;
                _logger?.LogInfo($"{LogTag} Lower trigger HIT our ball. lowerThisStep; latchedUpper={_latchedUpper}; queue flush. rb='{rb.name}' pos={rb.position}");
            }

            _flushQueued = true;
        }

        public void OnFixedUpdate(float fixedDeltaTime)
        {
            if (!_flushQueued)
                return;
            _flushQueued = false;

            var up = _upperThisStep;
            var low = _lowerThisStep;
            var latched = _latchedUpper;
            var bothSameStep = up && low;
            var lowerWithLatch = low && latched;

            _upperThisStep = false;
            _lowerThisStep = false;

            _logger?.LogInfo($"{LogTag} Flush: upperThisStep={up} lowerThisStep={low} latchedUpper={latched} → bothSame={bothSameStep} lowerWithLatch={lowerWithLatch}");

            if (!bothSameStep && !lowerWithLatch)
            {
                _logger?.LogInfo($"{LogTag} No score this flush (need upper→lower sequence).");
                return;
            }

            _latchedUpper = false;
            var scored = _basketball != null && _basketball.NotifyBasketMade();
            if (scored)
                _logger?.LogInfo($"{LogTag} NotifyBasketMade = TRUE — basket counted.");
            else
                _logger?.LogWarning($"{LogTag} NotifyBasketMade = FALSE (cooldown, rules, or state). Current score={_basketball?.Score}, last frame conditions were met.");
        }

        private bool TryResolveBallRb(Collider other, out Rigidbody rb, out string rejectReason)
        {
            rb = other.attachedRigidbody != null
                ? other.attachedRigidbody
                : other.GetComponentInParent<Rigidbody>();
            if (rb == null)
            {
                rejectReason = "no Rigidbody on collider or parents";
                return false;
            }

            if (rb != _ballBody)
            {
                rejectReason = $"Rigidbody mismatch (expected instanceId={_ballBody.GetInstanceID()}, got instanceId={rb.GetInstanceID()}, name='{rb.name}')";
                return false;
            }

            rejectReason = null;
            return true;
        }
    }
}
