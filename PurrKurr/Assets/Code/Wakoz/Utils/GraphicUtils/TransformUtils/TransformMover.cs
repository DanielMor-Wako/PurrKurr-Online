using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.Utils.GraphicUtils.TransformUtils {

    public class TransformMover : MonoBehaviour {

        private IEnumerator _moveAction;

        private bool _isAnimating;
        private bool _endBeforeTimeEnd;

        public bool IsAnimating() => _isAnimating;

        public void MoveToPosition(Transform theTransform, Vector3 targetPosition, float duration, Func<Task> actionOnEnd = null) {

            if (_moveAction != null) {
                StopCoroutine(_moveAction);
            }

            _isAnimating = true;
            float startTime = Time.time;
            Vector3 startPosition = transform.position;

            _moveAction = MoveCoroutine(theTransform, startPosition, targetPosition, startTime, duration, actionOnEnd);

            StartCoroutine(_moveAction);
        }

        public IEnumerator MoveCoroutine(Transform theTransform, Vector3 startPosition, Vector3 targetPosition, float startTime, float duration, Func<Task> actionOnEnd) {
            
            var endTime = startTime + duration;
            var percentage = 0f;
            _endBeforeTimeEnd = false;
            
            while (Time.time < endTime && !_endBeforeTimeEnd)
            {
                percentage = (Time.time - startTime) / duration;
                //Debug.Log($"animating % {percentage}");
                Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, percentage);
                theTransform.position = currentPosition;

                yield return null;
            }

            if (!_endBeforeTimeEnd) {
                theTransform.position = targetPosition;
            }
            
            _isAnimating = false;

            if (actionOnEnd == null) {
               yield break;
            }

            try {
                actionOnEnd();
            }
            finally { }
        }

        public void EndMove() {
            
            _endBeforeTimeEnd = true;
        }

    }

}