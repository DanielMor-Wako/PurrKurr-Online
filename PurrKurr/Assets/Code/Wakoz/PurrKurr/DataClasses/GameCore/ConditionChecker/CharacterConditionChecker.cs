using System;
using System.Collections;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.ConditionChecker
{
    public abstract class CharacterConditionChecker : MonoBehaviour, IConditionChecker
    {
        public void EndCheck() { }
        public void StartCheck(IInteractableBody targetBody, Action onConditionMetAction) { }
    }

    public abstract class CharacterConditionChecker<T> : MonoBehaviour, IConditionChecker
    {
        [SerializeField] protected T ValueToMatch;

        [Tooltip("Reduces the check process by certain value.\n0 = no limit")]
        [SerializeField][Min(0.01f)] protected float CheckRateLimiter = 0.2f;

        private protected IInteractableBody TargetBody;

        private protected Action OnConditionMetAction;

        private Coroutine _checkConditionCO;

        public abstract void UpdateCurrentValue();
        public abstract bool HasMetCondition();

        public void StartCheck(IInteractableBody targetBody, Action conditionMetAction)
        {
            TargetBody = targetBody;
            OnConditionMetAction = conditionMetAction;

            ClearRunningCO();
            _checkConditionCO = StartCoroutine(CheckConditionPeriodically());
        }

        private void ClearRunningCO()
        {
            if (_checkConditionCO != null)
            {
                StopCoroutine(_checkConditionCO);
                _checkConditionCO = null;
            }
        }

        public void EndCheck()
        {
            ClearRunningCO();
            OnConditionMetAction = null;
        }

        private IEnumerator CheckConditionPeriodically()
        {
            var conditionMet = false;

            while (!conditionMet)
            {
                yield return new WaitForSecondsRealtime(CheckRateLimiter);

                UpdateCurrentValue();
                conditionMet = HasMetCondition();
            }

            try
            {
                OnConditionMetAction?.Invoke();
            }
            finally
            {
                EndCheck();
            }

        }
    }

}