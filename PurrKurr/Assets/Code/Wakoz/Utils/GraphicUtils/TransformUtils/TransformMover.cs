using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.Utils.GraphicUtils.TransformUtils {

    public class TransformMover : MonoBehaviour {

        private bool _isAnimating;
        private bool _endBeforeTimeEnd;

        public bool IsAnimating() => _isAnimating;

        public void MoveToPosition(Transform theTransform, Vector3 targetPosition, float duration, Func<Task> actionOnEnd = null) {

            if (_isAnimating) {
                //Debug.Log("is already animating, returned.... maybe just override instead?");
                return;
            }

            _isAnimating = true;
            float startTime = Time.time;
            Vector3 startPosition = transform.position;

            StartCoroutine(MoveCoroutine(theTransform, startPosition, targetPosition, startTime, duration, actionOnEnd));
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
                // Ensure the transform reaches the final position exactly
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