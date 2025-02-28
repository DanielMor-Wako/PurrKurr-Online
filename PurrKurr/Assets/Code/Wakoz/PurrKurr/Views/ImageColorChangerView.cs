using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Wakoz.PurrKurr.Views
{
    public class ImageColorChangerView : View
    {
        [SerializeField] private Image _image;
        [SerializeField][Min(0.01f)] private float _transitionSpeed = 1f;
        [SerializeField] private Color _startColor = new Color(1, 1, 1, 0.5f);
        [SerializeField] private Color _endColor = new Color(0, 0, 0, 0);

        private Material _originalMaterial;
        private Coroutine _colorChangeCoroutine;
        private Func<Color, Color, float, IEnumerator> _smoothColorChangeDelegate;

        private Color _targetColor;

        public Image TargetImage => _image;

        public void StartTransition(Action callback = null)
            => ChangeColor(_startColor, callback);

        public void EndTransition(Action callback = null)
            => ChangeColor(_endColor, callback);

        /// <summary>
        /// Transition sprite color to the target color over time.
        /// </summary>
        /// <param name="targetColor">The target color to transition to.</param>
        /// <param name="callback">An optional callback to be invoked when the transition is complete.</param>
        public void ChangeColor(Color targetColor, Action callback = null)
        {
            _targetColor = targetColor;
            
            if (_colorChangeCoroutine != null)
            {
                StopCoroutine(_colorChangeCoroutine);
            }

            _smoothColorChangeDelegate ??= SmoothColorChange;
            _colorChangeCoroutine = StartCoroutine(_smoothColorChangeDelegate(_image.color, _targetColor, _transitionSpeed));

            callback?.Invoke();
        }

        private IEnumerator SmoothColorChange(Color startColor, Color targetColor, float smoothSpeed)
        {
            _originalMaterial ??= _image.material;
            if (_originalMaterial == null)
            {
                Debug.LogError("Image ref is not set");
                yield break;
            }
            float elapsedTime = 0f;
            _image.material = new Material(_originalMaterial);

            while (!Mathf.Approximately(_image.color.r, targetColor.r) ||
                   !Mathf.Approximately(_image.color.g, targetColor.g) ||
                   !Mathf.Approximately(_image.color.b, targetColor.b) ||
                   !Mathf.Approximately(_image.color.a, targetColor.a))
            {
                elapsedTime += Time.deltaTime * smoothSpeed;
                _image.color = Color.Lerp(startColor, targetColor, elapsedTime);

                yield return null;
            }

            _image.color = targetColor;
        }

        protected override void Clean()
        {
            if (_colorChangeCoroutine != null)
            {
                StopCoroutine(_colorChangeCoroutine);
            }

            _colorChangeCoroutine = null;
            _smoothColorChangeDelegate = null;
        }

        protected override void ModelChanged() { }
    }
}
