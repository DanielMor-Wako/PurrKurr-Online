using System;
using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Characters;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.CameraComponents {

    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour {
    
        [SerializeField] private Camera cam;
        [SerializeField] private Character2DController mainTarget;
        [SerializeField] private Transform CameraFocusPrefab;
        [SerializeField] private Transform cameraFocus;
        public List<Transform> targets;
        private bool isFocusPointActive = false;

        public float XMaxOffsetWhenLookingForward = 5f;
        public float XMinVelocityToLookForward = 10f;
        public float YMaxOffsetWhenCloseCenter = 2.2f;
        public float YMinZoomToAddOffset = 4f;
        [Min(0)] public float MinVelocityToSetFallingYOffset = 10;
        public float FallingYOffset = -5;
        [SerializeField] [Min(0)] private float distanceToCheckWhenFreeFalling = 2;
        [SerializeField] private Vector3 offset;
        [Min(1)] public float maxYDistanceToResetYOffset = 5f;

        [SerializeField] private float ySightDistanceMultiplier = 2f;
        [SerializeField] private float xSightDistanceMultiplier = 4f;
        [SerializeField] [Range(0.05f, 2f)] private float adjustFocusSpeed = 0.1f;

        public float smoothTime = 0.3f;
        public float minZoom = 30f;
        public float maxZoom = 9f;
        public float singleObjectZoom = 7f;
        public float zoomLimiter = 65f;
        [Min(1)] public float zoomLerpSpeed = 3;

        private Vector3 velocity;
        private Vector3 centerPoint;
        private Vector3 newPosition;
        private bool _isFreeFalling;
        private List<Transform> _previousTargets = new();
        private float _zoom;
        private float _newZoom;
        const int camFixedZPosition = -10;
    
        private void Start()  {
        
            cam ??= GetComponent<Camera>();
        }

        private void Update() {
        
            if (mainTarget == null || targets.Count == 0)
                return;
        
            var isFreeFallingHitPoint = mainTarget.IsFreeFalling(distanceToCheckWhenFreeFalling);
            _isFreeFalling = isFreeFallingHitPoint != Vector3.zero && mainTarget.Velocity.y < -MinVelocityToSetFallingYOffset;
            if (_isFreeFalling) {
                cameraFocus.transform.position = isFreeFallingHitPoint;
            }

            UpdateScreenOffset();
            //SwitchSingleTargetToTheClosetGround(_isFreeFalling);
            Move();
            Zoom();
        }

        private void FixedUpdate() {
        
            if (mainTarget == null || targets.Count == 0)
                return;

            SwitchSingleTargetToTheClosetGround(_isFreeFalling);
            //Zoom();
        }

        private bool SingleTargetIsMainTarget() =>
            targets.Count == 1 && targets[0] != null && targets[0] == mainTarget.transform;
        
        private void UpdateScreenOffset() {

            //RecalculateYOffsetByMaxZoom();
        
            UpdateScreenOffsetVertical(_isFreeFalling);
        
            RecalculateXOffsetBySpeed();
        }


        /*private void HandleScreenShake(float shakeDuration, float shakeForce)
        {
            object[] parms = new object[2] { shakeDuration, shakeForce };
            ActivateShake(parms);
        }*/

        /*
        private void HandleFocusPointChanged(Vector2 pos)
        {
            FollowCameraFocusAsTarget(pos, 0.35f);
        }*/

        void RecalculateXOffsetBySpeed() {

            if (targets.Count > 1) {
                offset.x = 0f;
                return;
            }


            // the larger x speed is applied, the larger x offset is
            if (Mathf.Abs(mainTarget.Velocity.x) > XMinVelocityToLookForward) //&& !target.State.State == Definitions.CharacterState.StandingUp)
            {
                float percentage = (mainTarget.Velocity.x) / (mainTarget.Stats.SprintSpeed);
                offset.x = XMaxOffsetWhenLookingForward * percentage;

            }
        }
    
        private void UpdateScreenOffsetVertical(bool isFreeFalling) {

            if (isFreeFalling && SingleTargetIsMainTarget()) {
                offset.y = FallingYOffset;

            } else if (areTargetsCloseOnVertical()) {
                offset.y = YMinZoomToAddOffset;
        
            } else {
                offset.y = 0;
            }
        }

        private bool areTargetsCloseOnVertical() {
        
            if (_newZoom <= maxYDistanceToResetYOffset) {
                return true;
            }

            foreach (var target in targets) { 

                if (target == mainTarget.transform) {
                    continue;
                }

                var yDistance = target.transform.position.y - mainTarget.transform.position.y;
                if (Mathf.Abs(yDistance) > maxYDistanceToResetYOffset) {
                    return false;
                }
            }

            return true;
        }

        private void SwitchSingleTargetToTheClosetGround(bool isFreeFalling) {
        
            if (isFreeFalling && SingleTargetIsMainTarget()) {
                _previousTargets.Clear();
                _previousTargets.AddRange(targets);
                targets.Clear();
                AddToTargetList(cameraFocus);
            
            } else if (!isFreeFalling && (targets[0] == null || targets[0] != mainTarget.transform)) {

                targets.Clear();
                if (_previousTargets.Count > 1) {
                    targets.AddRange(_previousTargets);
                
                } else {
                    AddToTargetList(mainTarget.transform);
                }
            
            }
        }

        void Move() {
        
            centerPoint = GetCenterPoint();

            newPosition = centerPoint + offset;
            newPosition.z = camFixedZPosition;

            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
        }

        void Zoom() {

            _zoom = targets.Count == 1 ? singleObjectZoom : maxZoom;
            _newZoom = Mathf.Lerp(_zoom, minZoom, GetGreatestDistance() / zoomLimiter);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, _newZoom, Time.deltaTime * zoomLerpSpeed);

        }

        float GetGreatestDistance() {

            var bounds = new Bounds(targets[0].position, Vector3.zero);
            for (int i = 0; i < targets.Count; i++) {
                bounds.Encapsulate(targets[i].position);
            }

            float greatestBound = bounds.size.x;
            if (bounds.size.y > greatestBound) {
                greatestBound = bounds.size.y;
            }

            return greatestBound;
        }

        Vector3 GetCenterPoint () {
        
            if (targets.Count == 1) {
                return targets[0].position;
            }

            var bounds = new Bounds(targets[0].position, Vector3.zero);
            for (int i = 0; i < targets.Count; i++) {
                bounds.Encapsulate(targets[i].position);
            }

            return bounds.center;
        }

        public void AddToTargetList(Transform newItem) {
        
            if (newItem == null) {
                return;
            }

            if (!targets.Contains(newItem)) {
                targets.Add(newItem);
            }
        }
        public void RemoveFromTargetList(Transform newItem) {
        
            if (newItem == null) {
                return;
            }

            int itemListID = 0;

            // find the transform similar to an existing one and remove it
            for (int i = targets.Count-1; i > 0; i--) {
                if (targets[i] != null && targets[i].name == newItem.name) {
                    itemListID = i;
                }
            }

            if (itemListID > 0) {
                targets.Remove(targets[itemListID]);
            }
            
        }

        public void SetMainHero(Character2DController hero, bool clearOtherTargets) {
        
            mainTarget = hero;

            if (clearOtherTargets) {
                targets.Clear();
            }
        
            AddToTargetList(hero.transform);
        }

        public void ShakeScreen(int force, float duration) {
            // todo: set screen shake
        }
    }
}
