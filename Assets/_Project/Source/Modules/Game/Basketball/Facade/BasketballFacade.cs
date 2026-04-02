using System;
using Basketball.Application;
using Basketball.Domain;
using Storage.Facade;
using UniRx;
using UnityEngine;

namespace Basketball.Facade
{
    public sealed class BasketballFacade : IBasketballFacade
    {
        private readonly BasketballGameState _state;
        private readonly BasketballTuningConfig _tuning;
        private readonly IStorageFacade _storage;

        private readonly ReactiveProperty<int> _scoreRx;
        private readonly ReactiveProperty<int> _bestScoreRx;
        private readonly ReactiveProperty<BasketballBallPhase> _phaseRx;
        private readonly ReactiveProperty<float> _aimChargeRx;

        public BasketballFacade(BasketballGameState state, BasketballTuningConfig tuning, IStorageFacade storage = null)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _tuning = tuning ?? BasketballTuningConfig.CreateRuntimeDefault();
            _storage = storage;

            _scoreRx = new ReactiveProperty<int>(_state.Score);
            _bestScoreRx = new ReactiveProperty<int>(_state.BestScore);
            _phaseRx = new ReactiveProperty<BasketballBallPhase>(_state.Phase);
            _aimChargeRx = new ReactiveProperty<float>(_state.AimCharge01);
        }

        public IReadOnlyReactiveProperty<int> ScoreRx => _scoreRx;
        public IReadOnlyReactiveProperty<int> BestScoreRx => _bestScoreRx;
        public IReadOnlyReactiveProperty<BasketballBallPhase> PhaseRx => _phaseRx;
        public IReadOnlyReactiveProperty<float> AimChargeRx => _aimChargeRx;

        public BasketballBallPhase Phase => _state.Phase;
        public int Score => _state.Score;
        public int BestScore => _state.BestScore;
        public float AimCharge01 => _state.AimCharge01;

        public bool NotifyBasketMade()
        {
            var previousBest = _state.BestScore;
            if (!BasketballRules.TryRegisterScore(_state, _tuning))
                return false;
            PushScoreState();
            if (_storage != null && _state.BestScore > previousBest)
            {
                _storage.SetInt(BasketballPersistenceKeys.BestScore, _state.BestScore);
                _storage.Save();
            }

            return true;
        }

        public void SetPhase(BasketballBallPhase phase)
        {
            if (_state.Phase == phase)
                return;
            BasketballRules.SetPhase(_state, phase);
            _phaseRx.Value = _state.Phase;
        }

        public void SetAimCharge(float charge01)
        {
            var v = Mathf.Clamp01(charge01);
            if (Mathf.Approximately(_state.AimCharge01, v))
                return;
            _state.AimCharge01 = v;
            _aimChargeRx.Value = v;
        }

        public void ResetRun()
        {
            _state.Score = 0;
            _state.Phase = BasketballBallPhase.Free;
            _state.AimCharge01 = 0f;
            _state.LastScoreUnscaledTime = 0f;
            PushAllReactiveFromState();
        }

        private void PushScoreState()
        {
            _scoreRx.Value = _state.Score;
            _bestScoreRx.Value = _state.BestScore;
        }

        private void PushAllReactiveFromState()
        {
            _scoreRx.Value = _state.Score;
            _bestScoreRx.Value = _state.BestScore;
            _phaseRx.Value = _state.Phase;
            _aimChargeRx.Value = _state.AimCharge01;
        }
    }
}
