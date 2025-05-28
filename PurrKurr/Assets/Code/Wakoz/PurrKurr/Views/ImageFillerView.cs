using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Views
{
    public class ImageFillerView : View
    {
        [SerializeField][Min(0.01f)] private float _transitionSpeed = 1;

        [SerializeField] private Image _image;

        private Coroutine _fillerCoroutine;
        private Func<float, float, Action, IEnumerator> _smoothFillDelegate;

        public Image ImageTarget => _image;

        /// <summary>
        /// Starts filling the image to the target value.
        /// </summary>
        public void StartTransition(float targetValue = 1, Action callback = null) 
            => FillTo(targetValue, callback);

        /// <summary>
        /// Starts emptying the image to the target value.
        /// </summary>
        public void EndTransition(float targetValue = 0, Action callback = null) 
            => FillTo(targetValue, callback);

        /// <summary>
        /// Fills the image to the specified target value.
        /// </summary>
        private void FillTo(float targetValue, Action callback = null)
        {
            if (_fillerCoroutine != null && gameObject != null)
            {
                StopCoroutine(_fillerCoroutine);
            }

            targetValue = Mathf.Clamp01(targetValue);
            _smoothFillDelegate ??= SmoothFill;
            _fillerCoroutine = StartCoroutine(_smoothFillDelegate(targetValue, _transitionSpeed, callback));
        }

        /// <summary>
        /// Smoothly fills or empties the image over time.
        /// </summary>
        private IEnumerator SmoothFill(float targetFill, float smoothSpeed = 1f, Action callback = null)
        {
            if (_image != null)
            {
                float startFill = _image.fillAmount;
                float elapsedTime = 0f;

                while (!Mathf.Approximately(_image.fillAmount, targetFill))
                {
                    elapsedTime += Time.deltaTime * smoothSpeed;
                    _image.fillAmount = Mathf.Lerp(startFill, targetFill, elapsedTime);

                    yield return null;
                }

                _image.fillAmount = targetFill;
            }

            callback?.Invoke();
        }

        /// <summary>
        /// Cleans up the coroutine and delegate when the view is destroyed or reset.
        /// </summary>
        protected override void Clean()
        {
            if (_fillerCoroutine != null && gameObject != null)
            {
                StopCoroutine(_fillerCoroutine);
            }

            _fillerCoroutine = null;
            _smoothFillDelegate = null;
        }

        protected override void ModelChanged() { }
    }
}
