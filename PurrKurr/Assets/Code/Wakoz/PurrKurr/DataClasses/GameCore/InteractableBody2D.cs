using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors;
using Code.Wakoz.Utils.GraphicUtils.TransformUtils;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore {

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

        protected override void Clean() {}
        protected override Task Initialize() {

            _anchor = gameObject.AddComponent<AnchorHandler>();
            SetAnchor();

            return Task.CompletedTask;
        }

        private void OnEnable() {
            SetAnchor();
        }

        private void SetAnchor() {
            _cling.connectedBody = SingleController.GetController<AnchorsController>().GetAnchorRigidbody(_anchor);
        }

        public Collider2D GetCollider() => _legsCollider ?? null;
        public Transform GetTransform() => transform;
        public Definitions.ObjectState GetCurrentState() => Definitions.ObjectState.Grounded;
        public Vector3 GetCenterPosition() => transform != null ? transform.position : Vector3.zero;

        public Vector2 GetVelocity() => _rigidbody.velocity;
        public float GetHpPercent() => 1;
        public int DealDamage(int damage) {
            // todo: deal damage?
            return 1;
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

            _transformMover.EndMove();
            transform.position = position;

            _anchor.ModifyAnchor(position, anchorParent?.GetTransform());
            _cling.connectedBody.simulated = anchorParent != null;

            _grabbedBody = anchorParent;
        }

        public bool IsGrabbing() => _grabbedBody != null;

        public bool IsGrabbed() => _cling.connectedBody.simulated && _grabbedBody != null;

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