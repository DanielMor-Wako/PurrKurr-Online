using System;
using System.Collections;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Views
{
    public class CanvasGroupFaderView : View
    {
        [SerializeField] [Min(0.01f)] private float _transitionSpeed = 1;

        [SerializeField] private CanvasGroup _canvasGroup;

        private Coroutine _faderCoroutine;
        private Func<float, float, Action, IEnumerator> _smoothFadeDelegate;

        public CanvasGroup CanvasTarget => _canvasGroup;

        public void StartTransition(Action callback = null)
        {
            FadeTo(1, callback);
        }

        public void EndTransition(Action callback = null)
        {
            FadeTo(0, callback);
        }

        private void FadeTo(float targetValue, Action callback = null)
        {
            if (_faderCoroutine != null && gameObject != null)
            {
                StopCoroutine(_faderCoroutine);
            }

            _smoothFadeDelegate ??= SmoothFade;
            _faderCoroutine = StartCoroutine(_smoothFadeDelegate(targetValue, _transitionSpeed, callback));
        }

        private IEnumerator SmoothFade(float targetAlpha, float smoothSpeed = 1f, Action callback = null)
        {
            if (_canvasGroup != null)
            {
                float startAlpha = _canvasGroup.alpha;
                float elapsedTime = 0f;
                _canvasGroup.gameObject.SetActive(true);

                while (!Mathf.Approximately(_canvasGroup.alpha, targetAlpha))
                {
                    elapsedTime += Time.deltaTime * smoothSpeed;
                    _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime);

                    yield return null;
                }

                _canvasGroup.alpha = targetAlpha;
                _canvasGroup.gameObject.SetActive(targetAlpha > 0);
            }

            callback?.Invoke();
        }

        protected override void Clean()
        {
            if (_faderCoroutine != null && gameObject != null)
            {
                StopCoroutine(_faderCoroutine);
            }
            
            _faderCoroutine = null;
            _smoothFadeDelegate = null;
        }
        protected override void ModelChanged() { }
    }
}
