using System;
using System.Collections;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger {

    public abstract class OverlayWindowCondition : MonoBehaviour, IGameEventCondition {

        public void EndCheckingCondition() { }
        public void StartCheckingCondition(IInteractableBody targetBody, Action onConditionMetAction) { }
    }

    public abstract class OverlayWindowCondition<T> : MonoBehaviour, IGameEventCondition {

        [SerializeField] protected T ValueToMatch;

        protected IInteractableBody TargetBody;

        private Action OnConditionMetAction;

        private Coroutine _checkConditionCO;


        public abstract void UpdateCurrentValue();
        public abstract bool HasMetCondition();

        public void StartCheckingCondition(IInteractableBody targetBody, Action conditionMetAction) {

            TargetBody = targetBody;
            OnConditionMetAction = conditionMetAction;

            ClearRunningCO();
            _checkConditionCO = StartCoroutine(CheckConditionPeriodically());
        }

        private void ClearRunningCO() {

            if (_checkConditionCO != null) {
                StopCoroutine(_checkConditionCO);
                _checkConditionCO = null;
            }
        }

        public void EndCheckingCondition() {

            ClearRunningCO();
            OnConditionMetAction = null;
        }

        private IEnumerator CheckConditionPeriodically() {

            var conditionMet = false;

            while (!conditionMet) {
                yield return new WaitForSecondsRealtime(0.2f);
                UpdateCurrentValue();
                conditionMet = HasMetCondition();
            }

            try {
                OnConditionMetAction?.Invoke();
            }
            finally {
                EndCheckingCondition();
            }

        }
    }

}