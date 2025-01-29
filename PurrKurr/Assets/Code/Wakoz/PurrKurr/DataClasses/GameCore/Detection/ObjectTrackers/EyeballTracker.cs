using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection
{
    public class EyeballTracker : DetectionZoneTracker
    {
        [Tooltip("Radius to start the diaalite process , value range between MaxScale to MinScale")]
        [SerializeField][Min(0f)] private float _diaalationRadius = .5f;

        [Tooltip("Scale to range between Min to Max Scale, when targets are beyond the diaalation Radius\nAdjusted by the closest target")]
        [SerializeField][Range(0, 1)] private float _diaalationMinScale = 0.5f;

        [Tooltip("Scale to set when targets are witin the diaalation Radius")]
        [SerializeField][Range(0, 1)] private float _diaalationMaxScale = 0.9f;

        [Tooltip("The diaalite speed, affecting the Scale of the trackerTransform")]
        [SerializeField][Min(0.1f)] private float _eyeDiaationSpeed = 4f;

        private float _maxDistance = 1f;

        protected override void TrackTarget()
        {
            base.TrackTarget();

            LerpToTargetScale(_trackerTransform, GetDiaalationScaleByDistanceToClosestTarget(), _eyeDiaationSpeed);
        }

        protected override void HandleColliderEntered(Collider2D triggeredCollider)
        {
            base.HandleColliderEntered(triggeredCollider);

            UpdateMaxDistance(triggeredCollider.transform.position);
        }

        private void LerpToTargetScale(Transform target, Vector3 endScale, float speed)
        {
            target.localScale = Vector2.Lerp(target.localScale, endScale, speed * Time.deltaTime);
        }

        private Vector3 GetDiaalationScaleByDistanceToClosestTarget()
        {
            var newDiaalationScale = _diaalationMaxScale;

            if (!HasAnyTargetBelowDistance(_diaalationRadius))
            {
                var closestTargetPos = GetClosestTargetDistance();

                var percentage = 1 - ((closestTargetPos - _diaalationRadius) / (_maxDistance - _diaalationRadius));

                newDiaalationScale = ((_diaalationMaxScale - _diaalationMinScale) * percentage + _diaalationMinScale);
            } 

            return newDiaalationScale * Vector3.one;
        }

        private void UpdateMaxDistance(Vector3 position)
        {
            var newDistance = (transform.position - position).magnitude;
            if (newDistance > _maxDistance)
            {
                _maxDistance = newDistance;
            }
        }

        protected override bool HasNoTargetsAndTrackerIsNotCentered()
        {
            return base.HasNoTargetsAndTrackerIsNotCentered() && IsApproximatelyMaxScaled(_trackerTransform.localScale);
        }

        private bool IsApproximatelyMaxScaled(Vector3 scale)
            => (_diaalationMaxScale - scale.x) < 0.01f;

    }
}