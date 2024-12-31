using Code.Wakoz.PurrKurr.Screens.CameraComponents;
using System.Collections.Generic;
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
