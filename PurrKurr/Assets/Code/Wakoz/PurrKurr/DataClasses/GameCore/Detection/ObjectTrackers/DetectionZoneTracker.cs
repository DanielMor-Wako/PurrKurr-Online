using System.Collections;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection
{
    public class DetectionZoneTracker : MonoBehaviour
    {
        [SerializeField] private DetectionZone _zone;

        [Tooltip("The transform to move within the radius")]
        [SerializeField] protected Transform _trackerTransform;

        [Tooltip("Time in seconds to refresh the closest target to focus on\ndefault 0 = constant refresh rate")]
        [SerializeField] private float _refreshDurationInSeconds = 0.4f;

        [Tooltip("Movement speed within the radius")]
        [SerializeField][Min(0.1f)] private float _trackingSpeed = 2f;

        [Tooltip("Max radius to move inside")]
        [SerializeField][Min(0.1f)] protected float _maxRadius = 1f;

        private Coroutine _trackingCoroutine;
        private Collider2D _closestTarget;
        private float _closestTargetDistance;

        protected float GetClosestTargetDistance()
            => _closestTargetDistance;

        protected bool HasAnyTargetBelowDistance(float distance) 
            => _closestTargetDistance < distance;

        protected void Awake()
        {
            Bind();
            StartTracking();
        }

        protected void OnDestroy()
        {
            Unbind();
            StopTracking();
        }

        private void Bind()
        {
            if (_zone == null)
                return;

            _zone.OnColliderEntered += HandleColliderEntered;
            _zone.OnColliderExited += HandleColliderExited;
        }

        private void Unbind()
        {
            if (_zone == null)
                return;

            _zone.OnColliderEntered -= HandleColliderEntered;
            _zone.OnColliderExited -= HandleColliderExited;
        }

        protected virtual void HandleColliderEntered(Collider2D triggeredCollider)
        {
            if (_zone.GetColliders().Length == 1)
            {
                StartTracking();
            }
        }

        protected virtual void HandleColliderExited(Collider2D triggeredCollider)
        {
            if (_zone.GetColliders().Length == 0)
            {
                StopTracking();
            }

        }
        private void StartTracking()
        {
            if (_trackingCoroutine != null)
            {
                StopCoroutine(_trackingCoroutine);
            }
            _trackingCoroutine = StartCoroutine(TrackClosestCollider());
        }

        private void StopTracking()
        {
            if (_trackingCoroutine != null)
            {
                StopCoroutine(_trackingCoroutine);
                _trackingCoroutine = null;
            }
        }

        private IEnumerator TrackClosestCollider()
        {
            float elapsedTime = _refreshDurationInSeconds;
            _closestTarget = null;

            while (true)
            {
                elapsedTime += Time.deltaTime;

                if (elapsedTime >= _refreshDurationInSeconds)
                {
                    elapsedTime = 0f;

                    var targetsOrderedByClosestDistance = _zone.GetColliders().
                    OrderBy(obj => (obj.transform.position - transform.position).sqrMagnitude).ToArray();

                    _closestTarget = targetsOrderedByClosestDistance.FirstOrDefault();
                }

                TrackTarget();

                yield return null;
            }
        }

        protected virtual void TrackTarget()
        {
            var endPosition = _closestTarget != null ? _closestTarget.transform.position : transform.position;
            UpdateClosestTargetDistance(endPosition);
            
            if (!HasAnyTargetBelowDistance(_maxRadius))
            {
                endPosition = transform.position + (endPosition - transform.position).normalized * _maxRadius;
            }

            LerpToTargetPosition(_trackerTransform, endPosition, _trackingSpeed);
        }

        private void UpdateClosestTargetDistance(Vector3 targetPos)
        {
            _closestTargetDistance = Vector2.Distance(transform.position, targetPos);
            return;

            // todo: check distance.sqrMagnitude as alternative for calculating radius
            // var distance = endPosition - transform.position;
            // Debug.Log($"squaredDistanceToTarget {_closestTargetDistance} <> {distance.sqrMagnitude} sqrMagnitude");
        }

        private void LerpToTargetPosition(Transform target, Vector3 endPosition, float speed)
        {
            target.position = Vector2.Lerp(target.position, endPosition, speed * Time.deltaTime);
        }
    }
}