using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors;
using Code.Wakoz.Utils.GraphicUtils.TransformUtils;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore
{

    [DefaultExecutionOrder(15)]
    public class InteractableBody2D : Controller, IInteractableBody {

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private TransformMover _transformMover;
        [SerializeField] private Collider2D _legsCollider;

        [SerializeField] private HingeJoint2D _cling;
        private AnchorHandler _anchor;

        private Vector3 NewPositionToSetOnFixedUpdte;
        private Vector2 ForceDirToSetOnFixedUpdate;

        private IInteractableBody _grabbedBody;

        [Header("Stats")]
        [Tooltip("Maximum health points.\nValue as -1 prevents dealing any damage (aka indestructible)")]
        [SerializeField] [Min(-1)] private int _maxHealth = 1;

        private float _health;

        protected override void Clean() {}
        protected override Task Initialize() {

            if (_cling != null)
            {
                _anchor = gameObject.AddComponent<AnchorHandler>();
                SetAnchor();
            }

            _health = _maxHealth;

            return Task.CompletedTask;
        }

        private void OnEnable() {
            SetAnchor();
        }

        private void SetAnchor() {
            if (_cling == null)
            {
                return;
            }

            _cling.connectedBody = SingleController.GetController<AnchorsController>().GetAnchorRigidbody(_anchor);
        }

        public Collider2D GetCollider() => _legsCollider ?? null;
        public Transform GetTransform() => transform;
        public Transform GetCharacterRigTransform() => null;
        public Definitions.ObjectState GetCurrentState() 
            => _health > 0 ? Definitions.ObjectState.Alive : Definitions.ObjectState.Dead;
        public Vector3 GetCenterPosition() => transform != null ? transform.position : Vector3.zero;

        public Vector2 GetVelocity() => _rigidbody.velocity;
        public float GetHpPercent() 
            => _maxHealth > 0 ? 
            Mathf.Clamp(_health / _maxHealth, 0, _maxHealth) : 0;

        public void DealDamage(int damage)
        {
            if (damage == 0 || _health == -1)
            {
                return;
            }

            // Deal damage
            _health = Mathf.Clamp(_health - damage, 0, _maxHealth);

            if (_health == 0)
            {
                gameObject.SetActive(false);
            }
        }

        public void ApplyForce(Vector2 forceDir) {
            SetForceDir(forceDir);
        }

        public void SetTargetPosition(Vector2 newPosition, float percentToPerform = 1) {
            NewPositionToSetOnFixedUpdte = newPosition;
        }

        public void SetAsGrabbing(IInteractableBody grabbedBody) {

            _grabbedBody = grabbedBody;
        }

        public void SetAsGrabbed(IInteractableBody anchorParent, Vector2 position) {

            var hasGrabber = anchorParent != null;

            if (_transformMover != null)
            {
                _transformMover.EndMove();
                transform.position = position;
            }

            if (_cling != null)
            {
                _anchor.ModifyAnchor(position, anchorParent?.GetTransform());
                _cling.connectedBody.simulated = hasGrabber;
            }

            if (_legsCollider != null)
            {
                _legsCollider.isTrigger = hasGrabber;
            }

            if (hasGrabber && _rigidbody != null)
            {
                _rigidbody.angularVelocity = 0;
            }

            _grabbedBody = anchorParent;
        }

        public bool IsGrabbing() => _grabbedBody != null;

        public bool IsGrabbed() => _cling != null && _cling.connectedBody.simulated && _grabbedBody != null;

        public IInteractableBody GetGrabbedTarget() => _grabbedBody;

        public void SetForceDir(Vector2 newForceDir) {

            ForceDirToSetOnFixedUpdate = newForceDir;
        }

        private void FixedUpdate() {
            
            if (NewPositionToSetOnFixedUpdte != Vector3.zero) {
                if (_transformMover != null) {
                    _transformMover.MoveToPosition(this.transform, NewPositionToSetOnFixedUpdte, 0.15f); // 0.15f is the fastest combat turn
                } else {
                    transform.position = NewPositionToSetOnFixedUpdte;
                }
                NewPositionToSetOnFixedUpdte = Vector3.zero;
                
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