using System;
using System.Collections;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Views
{
    public class TransformPositionerView : View
    {
        [Tooltip("Animation curve for scaling")]
        [SerializeField] private AnimationCurve _curve;

        [Tooltip("Target RectTransform to position")]
        [SerializeField] private Transform _targetTransform;

        [Tooltip("Default Transition duration, used when no specific value is set during Start & End Functions")]
        [SerializeField][Min(0.01f)] private readonly float DefaultMinTransitionDuration = 0.08f;

        [Tooltip("Min position values")]
        [SerializeField] private Vector3 _startPos = Vector3.zero;

        [Tooltip("Max position values")]
        [SerializeField] private Vector3 _endPos = Vector3.one;

        private Coroutine _coroutine;
        private Func<Vector3, Action, IEnumerator> _smoothDelegate;
        [Min(0.08f)][SerializeField] private float _transitionDuration;

        private void Awake()
        {
            if (_transitionDuration <= DefaultMinTransitionDuration) {
                SetTransitionDuration(DefaultMinTransitionDuration);
            }
        }

        public void SetTransitionDuration(float newDuration)
        {
            _transitionDuration =
                newDuration >= DefaultMinTransitionDuration ? newDuration : DefaultMinTransitionDuration;
        }

        public void SetEndPosition(Transform target)
        {
            _endPos = target.localPosition;
        }

        public void MoveToEndPosition(float percentageClamp01 = 0)
        {
            var targetPosition =
                (_endPos - _startPos) * Mathf.Clamp01(percentageClamp01) + _startPos;

            MoveToPosition(targetPosition);
        }

        public void MoveToPosition(Vector3 targetPosition, Action callback = null)
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
            }

            _smoothDelegate ??= SmoothPosition;
            _coroutine = StartCoroutine(_smoothDelegate(targetPosition, callback));
        }

        private IEnumerator SmoothPosition(Vector3 targetPosition, Action callback = null)
        {
            if (_targetTransform != null)
            {
                var startPosition = _targetTransform.localPosition;
                float elapsedTime = 0f;
                float t = 0;
                float curveValue;

                while (!Mathf.Approximately(_targetTransform.localPosition.y, targetPosition.y))
                {
                    elapsedTime += Time.deltaTime;
                    t = Mathf.Clamp01(elapsedTime / _transitionDuration);
                    curveValue = _curve.Evaluate(t);

                    _targetTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);

                    yield return null;
                }

                _targetTransform.localPosition = targetPosition;
            }

            callback?.Invoke();
        }
        protected override void Clean()
        {
            if (_coroutine != null && gameObject != null)
            {
                StopCoroutine(_coroutine);
            }

            _coroutine = null;
            _smoothDelegate = null;
        }
        protected override void ModelChanged() { }
    }
}
