using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.Utils.CameraUtils
{
    public static class CameraUtils
    {
        public static Vector3 GetCenterPoint(IEnumerable<Transform> targets)
        {
            if (targets == null)
                return Vector3.zero;

            var bounds = new Bounds();
            bool hasTargets = false;

            foreach (var target in targets)
            {
                if (!hasTargets)
                {
                    bounds = new Bounds(target.position, Vector3.zero);
                    hasTargets = true;
                }
                else
                {
                    bounds.Encapsulate(target.position);
                }
            }

            return hasTargets ? bounds.center : Vector3.zero;
        }

        public static void GetGreatestDistance(this IEnumerable<Transform> targets, ref float greatestDistanace) {
            if (targets == null)
                return;

            var bounds = new Bounds();
            bool hasTargets = false;

            foreach (var target in targets) {
                if (!hasTargets) {
                    bounds = new Bounds(target.position, Vector3.zero);
                    hasTargets = true;
                } else {
                    bounds.Encapsulate(target.position);
                }
            }

            greatestDistanace =  hasTargets ? Mathf.Max(bounds.size.x, bounds.size.y) : 0;
        }

        public static float GetGreatestDistance(IEnumerable<Transform> targets)
        {
            if (targets == null)
                return 0;

            var bounds = new Bounds();
            bool hasTargets = false;

            foreach (var target in targets)
            {
                if (!hasTargets)
                {
                    bounds = new Bounds(target.position, Vector3.zero);
                    hasTargets = true;
                }
                else
                {
                    bounds.Encapsulate(target.position);
                }
            }

            return hasTargets ? Mathf.Max(bounds.size.x, bounds.size.y) : 0;
        }
        /// <summary>
        /// Function to get the average position of several points
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static Vector2 GetAveragePoint(ref List<Vector2> positions)
        {
            if (positions == null || positions.Count == 0)
            {
                return Vector2.zero; 
            }

            Vector2 averagePosition = new Vector2(
                positions.Average(v => v.x),
                positions.Average(v => v.y)
            );

            return averagePosition;
        }

        public static Vector2 GetAveragePoint(ref List<Transform> positions)
        {
            if (positions == null || positions.Count == 0)
            {
                return Vector2.zero;
            }

            Vector2 averagePosition = new Vector2(
                positions.Average(v => v.position.x),
                positions.Average(v => v.position.y)
            );

            return averagePosition;
        }
        /// <summary>
        /// Function to percentage offset from the target position to the screen center
        /// Returns vector2 indicated offset by range [-1, 1] when [0, 0] is center
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <param name="cam"></param>
        /// <returns></returns>
        public static Vector3 GetOffsetFromCenter(Vector3 targetPosition, ref Camera cam)
        {
            // Center is 0.5 in viewport between range [0, 1], therefore decreasing 0.5f and
            // Scale back to range [-1, 1] by multiplying the value that range between [-0.5, 0.5] by 2 
            targetPosition.z = cam.transform.position.z;
            var targetOffset = cam.WorldToViewportPoint(targetPosition);
            targetOffset.Set(targetOffset.x - 0.5f, targetOffset.y - 0.5f, 0);
            return targetOffset * 2;
        }

        public static bool AreTargetsCloseOnVertical(IEnumerable<Transform> targets, Transform _mainCharacter, float maxYDistanceToResetYOffset = 5)
        {
            foreach (var target in targets)
            {
                if (target == _mainCharacter.transform)
                    continue;

                if (Mathf.Abs(target.transform.position.y - _mainCharacter.position.y) > maxYDistanceToResetYOffset)
                    return false;
            }

            return true;
        }
    }
}
