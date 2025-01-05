using System;
using TMPro;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.SceneTransition
{
    [Serializable]
    public class SceneTransitionView : View
    {
        [SerializeField]
        public CanvasGroup Fader;

        [SerializeField]
        private TMP_Text _textField;

        public void SetTitle(string newTitle = "")
        {
            if (_textField == null || string.IsNullOrEmpty(newTitle))
            {
                return;
            }

            _textField.SetText(newTitle);
        }

        public void StartTransition(System.Action callback = null)
        {
            FadeTo(1, callback);
        }

        public void EndTransition(System.Action callback = null)
        {
            FadeTo(0, callback);
        }

        private void FadeTo(float targetValue, System.Action callback = null)
        {
            if (Fader == null)
            {
                return;
            }
            Fader.alpha = (targetValue);
            callback?.Invoke();
        }

        protected override void ModelChanged()
        {

        }
    }
}
