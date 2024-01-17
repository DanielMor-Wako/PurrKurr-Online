using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors {
    public class DynamicAnchor : MonoBehaviour {

        // save restore points for the transform
        [SerializeField] private Vector3 localScale = Vector3.one;
        [SerializeField] private Quaternion localRotation = Quaternion.identity;

        protected AnchorHandler anchorCharacter;

        [SerializeField] private Rigidbody2D _rigidBiody;

        private void Awake() {

            localRotation = transform.localRotation;
            localScale = transform.localScale;

            _rigidBiody ??= GetComponent<Rigidbody2D>();
        }

        public void SetAnchorActive(bool isActive) => _rigidBiody.simulated = isActive;

        public void HandleAttachAnchor(HingeJoint2D joint) => joint.connectedBody = _rigidBiody;
        
        protected void HandleAnchorChanged(Vector3 newPos, GameObject newParent) {

            SetWallHangPositionRestriction(newPos, newParent);
        }

        private void SetWallHangPositionRestriction(Vector3 Pos, GameObject NewParent) {
            
            //connecting player on edge to dynamic anchor
            Pos.z = 0f;

            if (NewParent.transform != null && NewParent.transform != transform) {
                transform.SetParent(NewParent.transform);
            }

            ResetTransform(Pos);
        }

        private void ResetTransform(Vector3 newPos) {

            transform.position = newPos;
            transform.rotation = localRotation;
            transform.localScale = localScale;
        }
    }
}
