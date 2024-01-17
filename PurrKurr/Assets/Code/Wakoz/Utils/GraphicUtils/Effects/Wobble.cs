using UnityEngine;

namespace Code.Wakoz.Utils.GraphicUtils.Effects {

    [RequireComponent(typeof(RectTransform))]
    public class Wobble : MonoBehaviour {

        [SerializeField] private float _time = 1;
        [SerializeField] private float _goalAmplitude = 1;

        private RectTransform _rectTransform;
        private float _currentAmplitude;
        private Vector2 _orig;

        void Start() {
            _rectTransform = transform as RectTransform;
            _orig = _rectTransform.sizeDelta;
        }

        void Update() {
            var amp = 1 + (Mathf.Sin(Time.time / _time) * (_goalAmplitude - 1));
            _rectTransform.sizeDelta = _orig * amp;
        }

    }
}
