using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Views;
using Code.Wakoz.Utils.GraphicUtils.TransformUtils;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore {

    [DefaultExecutionOrder(15)]
    public class Projectile2D : Controller, IInteractableBody {

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private TransformMover _transformMover;
        [SerializeField] private Collider2D _legsCollider;

        [SerializeField] private MultiStateView _states;

        [Header("Settings")]
        [SerializeField] private float _animDuration = 2f;
        [SerializeField] private float _pushForce = 30f;
        [Tooltip("Extra time to remain active in the scene after movement animation has finished")]
        [SerializeField][Min(0)] private float _expirationTime = 3f;

        private float MovingAnimationDuration = 2.5f;
        private Vector2 NewPositionToSetOnFixedUpdte;
        private Vector2 ForceDirToSetOnFixedUpdate;

        private float _expiredEndTime;

        public void ApplyForce(Vector2 forceDir) {
            SetForceDir(forceDir);
        }

        public int DealDamage(int damage) {
            // todo: deal damage to projectile?
            return 1;
        }

        public Collider2D GetCollider() => _legsCollider ?? null;
        public Transform GetTransform() => transform;
        public Definitions.CharacterState GetCurrentState() => Definitions.CharacterState.Falling;
        public Vector3 GetCenterPosition() => transform.position;

        public IInteractableBody GetGrabbedTarget() => null;

        public float GetHpPercent() => 1f;

        public Vector2 GetVelocity() {
            return _transformMover.IsAnimating() || Time.time < _expiredEndTime ? Vector2.one * _pushForce : Vector2.zero;
        }

        public bool IsGrabbed() => false;

        public bool IsGrabbing() => false;

        public void SetAsGrabbed(IInteractableBody attackerGameObject, Vector2 grabPosition) {

            if (_transformMover.IsAnimating()) {
                Debug.Log("can not grab a moving projectile");
                return;
            }
        }

        public void SetAsGrabbing(IInteractableBody grabbedBody) { }

        public void SetTargetPosition(Vector2 newPosition, float percentToPerform = 1) {
            NewPositionToSetOnFixedUpdte = newPosition;
            MovingAnimationDuration = percentToPerform * _animDuration;
        }

        private void SetExpiracyTime() => _expiredEndTime = Time.time + _expirationTime;

        private Task OnMoveAnimationEnd() {

            UpdateState(false);
            UpdateCollider(false);
            SetExpiracyTime();

            return Task.CompletedTask;
        }

        private void UpdateState(bool isActive) {
            
            if (_states == null) {
                return;
            }

            _states.ChangeState(isActive ? 0 : 1);
        }

        private void UpdateCollider(bool isActive) {

            if (_legsCollider == null) {
                return;
            }

            _legsCollider.enabled = (isActive);
        }

        protected override void Clean() {

        }

        protected override Task Initialize() {

            return Task.CompletedTask;
        }

        public void SetForceDir(Vector2 newForceDir) {

            ForceDirToSetOnFixedUpdate = newForceDir;
        }

        private void FixedUpdate() {

            if (NewPositionToSetOnFixedUpdte != Vector2.zero) {
                if (_transformMover != null) {
                    UpdateState(true);
                    UpdateCollider(true);
                    _transformMover.MoveToPosition(this.transform, NewPositionToSetOnFixedUpdte, MovingAnimationDuration, () => OnMoveAnimationEnd());
                } else {
                    transform.position = NewPositionToSetOnFixedUpdte;
                }
                NewPositionToSetOnFixedUpdte = Vector2.zero;

            } else if (ForceDirToSetOnFixedUpdate != Vector2.zero) {

                if (_transformMover != null) {
                    _transformMover.EndMove();
                }

                if (_rigidbody != null) {
                    _rigidbody.velocity = ForceDirToSetOnFixedUpdate;
                }
                ForceDirToSetOnFixedUpdate = Vector2.zero;
            }

        }

    }
}
