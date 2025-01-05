using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Ropes {
    public sealed class RopeLinkController : Controller, IInteractableBody {

        public event Action<RopeLinkController> OnLinkStateChanged;
        public event Action<RopeLinkController, ActionInput, Definitions.NavigationType> OnLinkInteracted;

        [SerializeField] private Rigidbody2D _rigidBody;
        [SerializeField] private HingeJoint2D _joint;
        [SerializeField] private Collider2D _bodyCollider;

        private IInteractableBody _grabbedBody;
        
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

        public void ApplySwingForce(Vector2 swingForceDir) {
            _rigidBody.AddForce(swingForceDir);
        }

        public Collider2D GetCollider() => _bodyCollider;

        public Transform GetTransform() => transform;

        public Transform GetCharacterRigTransform() => null;

        public Definitions.ObjectState GetCurrentState() =>
            IsChainConnected() ? Definitions.ObjectState.Alive : Definitions.ObjectState.Dead;

        public Vector3 GetCenterPosition() => _rigidBody != null ? _rigidBody.transform.position : Vector3.zero;

        public Vector2 GetVelocity() => _rigidBody != null ? _rigidBody.transform.position : Vector3.zero;

        public float GetHpPercent() =>
            IsChainConnected() ? 1 : 0;

        public int DealDamage(int damage) {

            if (damage == 0) {
                return 1;
            }

            ConnectJointTo(null);
            OnLinkStateChanged?.Invoke(this);

            var totalHp = 0;
            return totalHp;
        }

        public void ApplyForce(Vector2 forceDir) {
            
            if (IsGrabbed()) {
                return;
            }
            
            _rigidBody.velocity = forceDir;
        }

        public void SetTargetPosition(Vector2 position, float percentToPerform = 1) {
            
        }

        public void SetAsGrabbing(IInteractableBody grabbedBody) { }

        public void SetAsGrabbed(IInteractableBody anchorParent, Vector2 position) {

            _grabbedBody = anchorParent;
        }

        public bool IsGrabbing() => false;

        public bool IsGrabbed() => _grabbedBody != null;

        public IInteractableBody GetGrabbedTarget() => _grabbedBody;

        public void TryPerformInteraction(ActionInput actionInput, Definitions.NavigationType navigationDir) {
            
            OnLinkInteracted?.Invoke(this, actionInput, navigationDir);
        }

        protected override void Clean() { }

        protected override Task Initialize() {

            _joint ??= GetComponent<HingeJoint2D>();
            _rigidBody ??= GetComponent<Rigidbody2D>();

            return Task.CompletedTask;
        }

    }
}