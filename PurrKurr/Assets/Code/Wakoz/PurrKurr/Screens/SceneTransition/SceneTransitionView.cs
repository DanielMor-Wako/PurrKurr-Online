using Code.Wakoz.PurrKurr.Views;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.SceneTransition
{
    [Serializable]
    public class SceneTransitionView : View
    {
        [SerializeField] private TMP_Text _textField;
        [SerializeField] private CanvasGroupFader _canvasGroupFader;
        [SerializeField] private RectTransformScaler _transformScaler;

        [Header("Blink Delay")]
        [SerializeField] [Min(0)] private float _preDelayBeforeBlinking = 0.2f;
        [Header("Blink Rate - Random between Min Max")]
        [SerializeField] [Min(0)] private float _randomMinBlink = 1f;
        [SerializeField] [Min(0)] private float _randomMaxBlink = 3f;

        private Coroutine _transformScalerCoroutine;
        private Func<float, float, IEnumerator> _transformScalerDelegate;

        public void StartTransition(Action callback = null, string newTitle = "")
        {
            SetTitle(newTitle);
            SetNoShade();
            SetEyesClosed();
            StartBlinking();
            _canvasGroupFader.StartTransition(callback);
        }

        public void EndTransition(Action callback = null)
        {
            _canvasGroupFader.EndTransition(callback);

            _transformScaler.SetTransitionDuration(_transformScaler.TransitionDuration);
            CloseEyes();
            if (_transformScalerCoroutine != null)
            {
                StopCoroutine(_transformScalerCoroutine);
            }
        }

        private void StartBlinking()
        {
            if (_transformScalerCoroutine != null)
            {
                StopCoroutine(_transformScalerCoroutine);
            }

            _transformScalerDelegate ??= RepeatBlinking;
            _transformScalerCoroutine = StartCoroutine(_transformScalerDelegate(_randomMinBlink, _randomMaxBlink));
        }

        private IEnumerator RepeatBlinking(float minRandom, float maxRandom)
        {
            var blinkDuration = _transformScaler.TransitionDuration;

            yield return new WaitForSeconds(_preDelayBeforeBlinking);

            _transformScaler.SetTransitionDuration(0.5f);
            OpenEyes();

            yield return new WaitForSeconds(1);

            _transformScaler.SetTransitionDuration(blinkDuration);

            while (true)
            {
                OpenEyes();

                float waitTimeClose = UnityEngine.Random.Range(minRandom, maxRandom);
                yield return new WaitForSeconds(waitTimeClose);

                CloseEyes();

                yield return new WaitForSeconds(blinkDuration);
            }
        }

        private void OpenEyes()
        {
            _transformScaler.StartTransition();
        }

        private void CloseEyes()
        {
            _transformScaler.EndTransition();
        }

        private void SetNoShade()
        {
            _canvasGroupFader.CanvasTarget.alpha = 0f;
        }

        private void SetEyesClosed()
        {
            _transformScaler.TargetRectTransform.localScale = _transformScaler.MinScale;
        }

        private void SetTitle(string newTitle = "")
        {
            _textField?.SetText(newTitle);
        }

        protected override void Clean()
        {
            if (_transformScalerCoroutine != null)
            {
                StopCoroutine(_transformScalerCoroutine);
            }

            _transformScalerCoroutine = null;
            _transformScalerDelegate = null;
        }

        protected override void ModelChanged() { }
    }
}
