using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Views;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GamePlayUtils {

    [DefaultExecutionOrder(12)]
    public sealed class GamePlayUtils : SingleController {

        [SerializeField] private MultiStateView _cursor;
        [SerializeField] private MultiStateView _angle;
        [SerializeField] private LineRenderer _trajectory;

        [Header("Slomo Settings")]
        [SerializeField][Range(0.1f, 1f)] public float _defaultTimeScale = 1f;
        [SerializeField] private bool _affectTimeScale = true;
        [SerializeField][Min(0)] public float _transitionDuration = 0.2f;
        [SerializeField][Range(0.075f, 1f)] public float _slomoTimeScale = 0.5f;

        private float _timeScaleTarget;
        private Coroutine _timeScaleTransitionCoroutine;

        protected override void Clean() { }

        protected override Task Initialize() {

            Time.timeScale = _defaultTimeScale;

            return Task.CompletedTask;
        }

        public void ToggleSlomoGameAssist() => _affectTimeScale = !_affectTimeScale;

        public void ActivateUtil(Definitions.ActionType actionType, Vector2 position, Quaternion quaternion, bool hasHitData, Vector3[] linePoints) => 
            UpdateUtilByActionType(actionType, true, position, quaternion, hasHitData, linePoints);

        public void DeactivateUtil(Definitions.ActionType actionType) =>
            UpdateUtilByActionType(actionType, false, Vector2.zero, Quaternion.identity);

        private void UpdateUtilByActionType(Definitions.ActionType actionType, bool isActive, Vector2 position, Quaternion quaternion, bool hasHitData = false, Vector3[] linePoints = null) {

            TimeScaleTransition(_affectTimeScale && isActive);

            switch (actionType) {

                case Definitions.ActionType.Rope:
                    UpdateUtil(_cursor?.transform, isActive, position, quaternion);
                    UpdateUtilState(_cursor, hasHitData);
                    break;

                case Definitions.ActionType.Projectile:
                    UpdateUtil(_angle?.transform, isActive, position, quaternion);
                    UpdateUtilState(_angle, hasHitData);
                    break;

                case Definitions.ActionType.Jump:
                    UpdateUtil(_trajectory?.transform, isActive, position, quaternion);
                    if (linePoints != null && _trajectory != null) {
                        _trajectory.positionCount = linePoints.Length;
                        _trajectory.SetPositions(linePoints);
                    }
                    break;

                default:
                    break;
            }

        }

        private void UpdateUtil(Transform utilTransform, bool isActive, Vector2 position, Quaternion quaternion) {
            
            if (utilTransform == null) {
                return;
            }

            if (isActive) {
                utilTransform.transform.SetPositionAndRotation(position, quaternion);
            }

            utilTransform.gameObject.SetActive(isActive);
        }

        private void UpdateUtilState(MultiStateView utilState, bool isActive) {

            if (utilState == null) {
                return;
            }

            utilState.ChangeState(isActive ? 1 : 0);
        }

        private void TimeScaleTransition(bool slomoActive) {

            _timeScaleTarget = slomoActive ? _slomoTimeScale : _defaultTimeScale;

            if (_timeScaleTransitionCoroutine != null) {
                StopCoroutine(_timeScaleTransitionCoroutine);
                _timeScaleTransitionCoroutine = null;
            }
            
            _timeScaleTransitionCoroutine = StartCoroutine(TransitionTimeScale(_timeScaleTarget));
        }

        private IEnumerator TransitionTimeScale(float timeScaleTarget) {

            float elapsedTime = 0f;
            float initialTimeScale = Time.timeScale;
            float target = timeScaleTarget;

            while (elapsedTime < _transitionDuration) {
                Time.timeScale = Mathf.Lerp(initialTimeScale, target, elapsedTime / _transitionDuration);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            Time.timeScale = timeScaleTarget;
            _timeScaleTransitionCoroutine = null;
        }
    }
}