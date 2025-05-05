using Code.Wakoz.PurrKurr.Screens.CameraSystem;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Wakoz.PurrKurr.DataClasses.GamePlayUtils
{
    [DefaultExecutionOrder(12)]
    public sealed class UiIconsMoverController : SingleController
    {
        [SerializeField] public RectTransform _markerTransform;

        [SerializeField] private AnimationCurve _movementCurve;
        [SerializeField][Range(0.01f, 5)] private float _durationInSeconds = 1f;
        [FormerlySerializedAs("_alphaCurve")][SerializeField] private AnimationCurve _scaleCurve;

        private int _corooutineCount;
        private Camera _cam;

        private Dictionary<ScreenEdge, Vector2> _screenEdges = new();
        public enum ScreenEdge : byte {
            TopCenter = 0, TopRightCorner = 1, RightCenter = 2, BottomRightCorner = 3, BottomCenter = 4, BottomLeftCorner = 5, LeftCenter = 6, TopLeftCorner = 7
        }

        private void OnDisable() => Clean();

        protected override void Clean() {

            if (_corooutineCount > 0) {
                _corooutineCount = 0;
                StopAllCoroutines();
            }
        }

        protected override Task Initialize() {

            _corooutineCount = 0;
            return Task.CompletedTask;
        }

        public void DeactivateIcon(RectTransform icon) {

            Destroy(icon.gameObject);
            _corooutineCount--;
        }

        public void ActivateIcon(Transform target, ScreenEdge screenEdge) {

            if (_screenEdges == null || _screenEdges.Count == 0) {
                DefineEdges();
            }

            if (!_screenEdges.TryGetValue(screenEdge, out var endPosition))
                return;

            ActivateIcon(target, endPosition);
        }

        public void ActivateIcon(Transform target, Transform endTarget) {

            if (target == null || endTarget == null)
                return;

            _cam ??= GetController<GameplayController>().Handlers.GetHandler<CameraHandler>().CameraComponent;
            
            var endTargetPosition = (Vector2)_cam.WorldToScreenPoint(endTarget.position);

            ActivateIcon(target, endTargetPosition);
        }

        public void ActivateIcon(Transform target, Vector2 screenPosition) {

            RectTransform clone = Instantiate(_markerTransform, transform);

            _cam ??= GetController<GameplayController>().Handlers.GetHandler<CameraHandler>().CameraComponent;

            var screenPoint = (Vector2)_cam.WorldToScreenPoint(target.position);

            clone.gameObject.SetActive(true);
            clone.anchoredPosition = screenPoint;

            SmoothToPos(clone, screenPosition);
        }

        private void DefineEdges() {

            var iconSize = _markerTransform.rect.width * .5f;

            var width = Screen.width;
            var height = Screen.height;
            var centerWidth = width * 0.5f;
            var centerHeight = height * 0.5f;

            _screenEdges = new();

            MapEdge(ScreenEdge.TopLeftCorner, new Vector2(-iconSize, height + iconSize));
            MapEdge(ScreenEdge.TopRightCorner, new Vector2(width + iconSize, height + iconSize));
            MapEdge(ScreenEdge.BottomLeftCorner, new Vector2(-iconSize, -iconSize));
            MapEdge(ScreenEdge.BottomRightCorner, new Vector2(width + iconSize, -iconSize));
            MapEdge(ScreenEdge.TopCenter, new Vector2(centerWidth, height + iconSize));
            MapEdge(ScreenEdge.RightCenter, new Vector2(width + iconSize, centerHeight));
            MapEdge(ScreenEdge.BottomCenter, new Vector2(centerWidth, -iconSize));
            MapEdge(ScreenEdge.LeftCenter, new Vector2(-iconSize, centerHeight));
        }

        private void MapEdge(ScreenEdge edgeType, Vector2 screenPosition) 
            => _screenEdges[edgeType] = screenPosition;

        private void SmoothToPos(RectTransform marker, Vector2 endPosition) {

            if (marker == null) {
                return;
            }

            _corooutineCount++;
            _ = StartCoroutine(SmoothMove(marker, endPosition));
        }

        private IEnumerator SmoothMove(RectTransform target, Vector2 endPosition) {

            var elapsedTime = 0f;
            var posCurve = 0f;
            var targetAlpha = target.GetComponent<CanvasGroup>();

            var baseScale = new Vector3(0, 0, 1);
            var addScale = new Vector3(1, 1, 0);

            _cam ??= GetController<GameplayController>().Handlers.GetHandler<CameraHandler>().CameraComponent;

            while (elapsedTime < _durationInSeconds) {

                if (target == null) {
                    yield break;
                }

                var percent = elapsedTime / _durationInSeconds;

                posCurve = _movementCurve.Evaluate(percent);
                target.anchoredPosition = Vector3.Lerp(target.anchoredPosition, endPosition, posCurve);

                targetAlpha.transform.localScale = baseScale + addScale * _scaleCurve.Evaluate(percent);

                elapsedTime += Time.unscaledDeltaTime;

                yield return null;
            }

            DeactivateIcon(target);
        }
    }
}