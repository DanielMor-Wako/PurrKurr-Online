using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.Screens.CameraSystem;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GamePlayUtils
{

    [DefaultExecutionOrder(12)]
    public sealed class UiGuidanceController : SingleController
    {
        [SerializeField] public RectTransform _markerTransform;
        [SerializeField] public RectTransform _markerArrow;

        [Tooltip("Transition refresh rate in seconds, used as interval for re-calculating marker end position")]
        [SerializeField][Min(0.01f)] private float _refreshRateInSeconds = 0.5f;
        [SerializeField][Range(0.01f, 1)] private float _smoothRange = 0.01f;
        
        [Tooltip("Marker offset when target is visible on the screen")]
        [SerializeField] float markerXAxisOffset = 0f;
        [SerializeField] float markerYAxisOffset = 100f;

        private Coroutine _co;
        private Camera _cam;

        private int _screenWidth, _screenHeight = 0;

        private void OnDisable() => Clean();

        protected override void Clean() {

            if (_co != null) {
                StopCoroutine(_co);
                _co = null;
            }
        }

        protected override Task Initialize() {

            if (_markerTransform == null) {
                Debug.Log("GuidanceController is missing markerTransform");
            }

            _screenWidth = Screen.width;
            _screenHeight = Screen.height;

            return Task.CompletedTask;
        }

        public void DeactivateMarker() {

            _markerTransform.gameObject.SetActive(false);

            if (_co != null) {
                StopCoroutine(_co);
                _co = null;
            }
        }

        public void ActivateMarker(ObjectiveMarker marker) {

            _markerTransform.gameObject.SetActive(true);

            SmoothToPos(marker);
        }

        private void SmoothToPos(ObjectiveMarker marker) {

            if (_co != null) {
                StopCoroutine(_co);
                _co = null;
            }

            if (marker == null) {
                return;
            }

            _co = StartCoroutine(SmoothMove(marker));
        }

        private IEnumerator SmoothMove(ObjectiveMarker data) {

            var target = data.MarkerRef;
            var elapsedTime = 0f;

            _cam ??= GetController<GameplayController>().Handlers.GetHandler<CameraHandler>().CameraComponent;

            var screenPoint = (Vector2)_cam.WorldToScreenPoint(target.position);
            var endPos = GetLocalPoint(screenPoint);
            _markerArrow.rotation = GetRotationTowardsPoint((screenPoint - _markerTransform.anchoredPosition).normalized);

            while (elapsedTime < _refreshRateInSeconds) {

                if (_markerTransform == null || target == null) {
                    yield break;
                }

                _markerTransform.anchoredPosition = _markerTransform.anchoredPosition + (endPos - _markerTransform.anchoredPosition) * _smoothRange;

                elapsedTime += Time.unscaledDeltaTime;

                yield return null;

                if (elapsedTime >= _refreshRateInSeconds && _markerTransform != null) {

                    screenPoint = _cam.WorldToScreenPoint(target.position);
                    endPos = GetLocalPoint(screenPoint);
                _markerArrow.rotation = GetRotationTowardsPoint((screenPoint - _markerTransform.anchoredPosition).normalized);
                    elapsedTime = 0f;
                }
            }

            _co = null;
        }

        private Quaternion GetRotationTowardsPoint(Vector2 normalizedDirection) {

            var angleInDegrees = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, 0, angleInDegrees);
        }

        private Vector2 GetLocalPoint(Vector2 screenPoint) {

            var size = _markerTransform.rect.width;
            var halfSize = size * 0.5f;

            var leftEdge = halfSize;
            var rightEdge = _screenWidth - halfSize;
            var bottomEdge = size;
            var topEdge = _screenHeight - size;

            var xOffset = 0f;
            var yOffset = 0f;

            if (screenPoint.x >= leftEdge && screenPoint.x <= rightEdge
                && screenPoint.y >= bottomEdge && screenPoint.y <= topEdge) {

                xOffset = markerXAxisOffset;
                yOffset = markerYAxisOffset;
            }

            screenPoint.x = Mathf.Clamp(screenPoint.x + xOffset, leftEdge, rightEdge);
            screenPoint.y = Mathf.Clamp(screenPoint.y + yOffset, bottomEdge, topEdge);

            return screenPoint;
        }

    }
}