using System.Collections;
using UnityEngine;

namespace Code.Wakoz.Utils.GraphicUtils.TransformUtils {

    public class TransformMover : MonoBehaviour {

        private bool _isAnimating;
        private bool _endBeforeTimeEnd;

        public void MoveToPosition(Transform theTransform, Vector3 targetPosition, float duration) {

            if (_isAnimating) {
                //Debug.Log("is already animating, returned.... maybe just override instead?");
                return;
            }

            _isAnimating = true;
            float startTime = Time.time;
            Vector3 startPosition = transform.position;

            StartCoroutine(MoveCoroutine(theTransform, startPosition, targetPosition, startTime, duration));
        }

        public IEnumerator MoveCoroutine(Transform theTransform, Vector3 startPosition, Vector3 targetPosition, float startTime, float duration) {
            
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
        }

        public void EndMove() {
            
            _endBeforeTimeEnd = true;
        }

    }

}