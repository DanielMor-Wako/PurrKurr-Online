using Code.Wakoz.PurrKurr.DataClasses.Enums;
using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {
    public class Character2DRig : MonoBehaviour {

        [SerializeField][Range(0, 360)] private float QuaterminionOffsetAngle = 180;
        [SerializeField] private Transform _rigTrasform;
        [Tooltip("Controlls the animator rig, Auto = to be affected by facing right or left. or set as fixed direction right or left")]
        [SerializeField] private Definitions.Character2DFacingRightType _facingType;

        private float _facingRightOnSurfaceAngle;
        private Vector3 FacingRight = Vector3.one;
        private Vector3 FacingLeft = new Vector3(-1, 1, 1);

        public bool _wasInitialized { get; private set; }

        public void Init() {

            if (_wasInitialized) {
                return;
            }
            
            SetFacingRight(_facingType != Definitions.Character2DFacingRightType.FixedLeft);
            _wasInitialized = true;
        }

        public void SetFacingRight(bool isFacingRight) {

            var facingRightScale = FacingRight;
            var facingRightAngle = QuaterminionOffsetAngle;

            if (_facingType == Definitions.Character2DFacingRightType.Auto) {
                facingRightScale = isFacingRight ? FacingRight : FacingLeft;
                facingRightAngle = isFacingRight ? QuaterminionOffsetAngle : -QuaterminionOffsetAngle;

            } else if (_facingType == Definitions.Character2DFacingRightType.FixedLeft) {

                facingRightScale = FacingLeft;
                facingRightAngle = -QuaterminionOffsetAngle;
            }

            _rigTrasform.localScale = facingRightScale;
            _facingRightOnSurfaceAngle = facingRightAngle;
        }

        public void UpdateRigRotation(Quaternion terrainQuaternion, float defaultOffset = -1) {

            var offsetQuaternion = Quaternion.Euler(0f, 0f, defaultOffset < 0 ? _facingRightOnSurfaceAngle : defaultOffset);
            var newRotation = Quaternion.Lerp(_rigTrasform.rotation,
                terrainQuaternion * offsetQuaternion,
                Time.deltaTime * 10f);

            _rigTrasform.rotation = newRotation;

        }

        public void UpdateBodyScale(float bodySize) {

            transform.localScale = new Vector3(bodySize, bodySize, 1);
        }
    }
}