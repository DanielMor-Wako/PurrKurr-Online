using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.Views;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger
{
    [DefaultExecutionOrder(15)]
    public class ObjectDetectionTrigger : DetectionZoneTrigger
    {
        [Tooltip("Sets the max activation count the trigger. 0 = infinite activations")]
        [SerializeField][Min(0)] private int _activationLimit = 0;
        [Tooltip("Sets the View state to active and deactive")]
        [SerializeField] private MultiStateView _state;
        [Tooltip("Min magnitude to register the detection")]
        [SerializeField][Min(0)] private float MinMagnitudeOnDetection = 20;

        private int _activationsCount = 0;

        [Tooltip("Min duration to keep active state alive before auto-changing\nDefault as 0, does not take delay into consideration and waits for Collision Exit")]
        [SerializeField][Min(0)] private float MinSecondsAsActiveState;
        private Coroutine _checkConditionCO;

        protected override void Clean()
        {
            base.Clean();

            OnColliderEntered -= handleColliderEntered;
            OnColliderExited -= handleColliderExited;
        }

        protected override Task Initialize()
        {
            base.Initialize();

            OnColliderEntered += handleColliderEntered;
            OnColliderExited += handleColliderExited;

            UpdateStateView(false);

            return Task.CompletedTask;
        }

        private void handleColliderExited(Collider2D triggeredCollider)
        {
            if (IsUsingCountdownOnActiveState())
            {
                return;
            }

            UpdateStateView(false);
        }

        private void handleColliderEntered(Collider2D triggeredCollider)
        {
            if (HasReachedMaxCount())
            {
                return;
            }
            
            var rigBod = triggeredCollider.attachedRigidbody;
            if (rigBod == null || rigBod != null && rigBod.velocity.magnitude > MinMagnitudeOnDetection)
            {
                _activationsCount++;
                UpdateStateView(true);
            }
            if (rigBod != null)
            {
                Debug.Log($"Impact Mag {(rigBod.velocity.magnitude > MinMagnitudeOnDetection ? "reached" : "unreached")} = {rigBod.velocity.magnitude} > {MinMagnitudeOnDetection}");
            }
        }

        private bool HasReachedMaxCount() => _activationLimit > 0 && _activationsCount >= _activationLimit;

        private void UpdateStateView(bool activeState)
        {
            if (_state == null)
            {
                return;
            }

            _state.ChangeState(activeState ? 0 : !HasReachedMaxCount() ? 1 : 2);

            if (activeState && IsUsingCountdownOnActiveState())
            {
                ClearExistingCO();
                _checkConditionCO = StartCoroutine(ChangeStateToFalseWithDelay());
            }

        }

        private bool IsUsingCountdownOnActiveState()
            => MinSecondsAsActiveState > 0;

        private void ClearExistingCO()
        {
            if (_checkConditionCO != null)
            {
                StopCoroutine(_checkConditionCO);
                _checkConditionCO = null;
            }
        }

        private IEnumerator ChangeStateToFalseWithDelay()
        {
            yield return new WaitForSecondsRealtime(MinSecondsAsActiveState);

            UpdateStateView(false);
        }
    }

}