using Code.Wakoz.PurrKurr.DataClasses.Enums;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Projectiles {
    public sealed class RopeLinkController : Controller, IInteractableBody {

        public event Action<RopeLinkController> OnLinkStateChanged;

        [SerializeField] private Rigidbody2D _rigidBody;
        [SerializeField] private HingeJoint2D _joint;

        protected override void Clean() { }

        protected override Task Initialize() {

            _joint ??= GetComponent<HingeJoint2D>();
            _rigidBody ??= GetComponent<Rigidbody2D>();

            return Task.CompletedTask;
        }

        public Rigidbody2D GetRigidBody() => _rigidBody;

        public void ConnectJointTo(Rigidbody2D bodyToConnectTo) {

            _joint.connectedBody = bodyToConnectTo;
            _joint.enabled = bodyToConnectTo != null;
        }

        public void SetSimulated(bool isActive) {

            _rigidBody.velocity = Vector3.zero;
            _rigidBody.simulated = isActive;
        }

        public bool IsChainConnected() => _joint.connectedBody != null;

        public Rigidbody2D GetChainedBody() => _joint.connectedBody;

        public Collider2D GetCollider() => null;

        public Transform GetTransform() => transform;

        public Definitions.CharacterState GetCurrentState() =>
            IsChainConnected() ? Definitions.CharacterState.Alive : Definitions.CharacterState.Dead;

        public Vector3 GetCenterPosition() => _rigidBody != null ? _rigidBody.transform.position : Vector3.zero;

        public Vector2 GetVelocity() => _rigidBody != null ? _rigidBody.transform.position : Vector3.zero;

        public float GetHpPercent() =>
            IsChainConnected() ? 1 : 0;

        public int DealDamage(int damage) {
            ConnectJointTo(null);
            OnLinkStateChanged?.Invoke(this);
            return 0;
        }

        public void ApplyForce(Vector2 forceDir) {
            _rigidBody.velocity = forceDir;
        }

        public void SetTargetPosition(Vector2 position, float percentToPerform = 1) {
            
        }

        private IInteractableBody _grabbedBody;

        public void SetAsGrabbing(IInteractableBody grabbedBody) { }

        public void SetAsGrabbed(IInteractableBody anchorParent, Vector2 position) {

            _grabbedBody = anchorParent;
        }

        public bool IsGrabbing() => false;

        public bool IsGrabbed() => _grabbedBody != null;

        public IInteractableBody GetGrabbedTarget() => _grabbedBody;

    }
}