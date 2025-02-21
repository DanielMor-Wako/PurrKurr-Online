using System;
using System.Collections;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Views
{
    public class RectTransformScalerView : View
    {
        [Tooltip("Animation curve for scaling")]
        [SerializeField] private AnimationCurve _scaleCurve;
        
        [Tooltip("Target RectTransform to scale")]
        [SerializeField] private RectTransform _targetRectTransform;

        [Tooltip("Default Transition duration, used when no specific value is set during Start & End Functions")]
        [SerializeField][Min(0.01f)] private float _defaultTransitionDuration = 0.08f;

        [Tooltip("Min Scale values")]
        [SerializeField] private Vector3 _minScale = Vector3.zero;

        [Tooltip("Max Scale values")]
        [SerializeField] private Vector3 _maxScale = Vector3.one;

        private Coroutine _scalerCoroutine;
        private Func<Vector3, Action, IEnumerator> _smoothScaleDelegate;
        private float _transitionDuration;

        public float TransitionDuration => _defaultTransitionDuration;
        public RectTransform TargetRectTransform => _targetRectTransform;
        public Vector3 MinScale => _minScale;
        public Vector3 MaxScale => _maxScale;

        public void SetTransitionDuration(float newDuration)
        {
            _transitionDuration = 
                newDuration >= _defaultTransitionDuration ? newDuration : _defaultTransitionDuration;
        }

        public void StartTransition(Action callback = null) 
            => ScaleTo(_maxScale, callback);

        public void EndTransition(Action callback = null) 
            => ScaleTo(_minScale, callback);

        private void ScaleTo(Vector3 targetScale, Action callback = null)
        {
            if (_transitionDuration == 0)
            {
                SetTransitionDuration(_defaultTransitionDuration);
            } 

            if (_scalerCoroutine != null)
            {
                StopCoroutine(_scalerCoroutine);
            }

            _smoothScaleDelegate ??= SmoothScale;
            _scalerCoroutine = StartCoroutine(_smoothScaleDelegate(targetScale, callback));
        }

        private IEnumerator SmoothScale(Vector3 targetScale, Action callback = null)
        {
            if (_targetRectTransform != null)
            {
                var startScale = _targetRectTransform.localScale;
                float elapsedTime = 0f;
                float t = 0;
                float curveValue;

                while (!Mathf.Approximately(_targetRectTransform.localScale.y, targetScale.y))
                {
                    elapsedTime += Time.deltaTime;
                    t = Mathf.Clamp01(elapsedTime / _transitionDuration); 
                    curveValue = _scaleCurve.Evaluate(t);

                    _targetRectTransform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
                    
                    yield return null;
                }

                _targetRectTransform.localScale = targetScale;
            }

            callback?.Invoke();
        }
        protected override void Clean()
        {
            if (_scalerCoroutine != null && gameObject != null)
            {
                StopCoroutine(_scalerCoroutine);
            }

            _scalerCoroutine = null;
            _smoothScaleDelegate = null;
        }
        protected override void ModelChanged() { }
    }
}
