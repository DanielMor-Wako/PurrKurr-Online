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

        public float XMaxOffsetWhenLookingForward = 5f;
        public float XMaxVelocityToLookForwardByScreenSize = 4f;
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
        public float MinScreenVelocityForXCenterSmoothing = 5f;
        public float XBackToCenterSmoothing = 2f;
        public float minZoom = 30f;
        public float maxZoom = 9f;
        public float singleObjectZoom = 7f;
        public float zoomLimiter = 65f;
        [Min(1)] public float zoomLerpSpeed = 3;

        private Vector3 velocity;
        private Vector3 centerPoint;
        private Vector3 newPosition;
        private bool _isFreeFalling;
        private bool _isTopEdge, _isBottomEdge, _isLeftEdge, _isRightEdge;
        private List<Transform> _previousTargets = new();
        private float _zoom;
        private float _newZoom;
        const int camFixedZPosition = -10;
        private float _screenHeight, _screenWidth;


        private void Start()  {
        
            cam ??= GetComponent<Camera>();
            _screenHeight = Screen.height;
            _screenWidth = Screen.width;
            XMaxVelocityToLookForwardByScreenSize = _screenHeight * 1.5f; // multi by 1.5f so the player can can reach half the screen on each size
        }

        private void Update() {
        
            if (mainTarget == null || targets.Count == 0)
                return;
        
            var isFreeFallingHitPoint = mainTarget.IsFreeFalling(distanceToCheckWhenFreeFalling);
            _isFreeFalling = isFreeFallingHitPoint != Vector3.zero && mainTarget.Velocity.y < -MinVelocityToSetFallingYOffset;
            if (_isFreeFalling) {
                cameraFocus.transform.position = isFreeFallingHitPoint;
            }

            UpdateScreenEdge();

            UpdateScreenOffset();
            //SwitchSingleTargetToTheClosetGround(_isFreeFalling);
            Move();
            Zoom();
        }

        private void UpdateScreenEdge() {

            if (_isFreeFalling) {
                _isTopEdge = _isBottomEdge =_isLeftEdge = _isRightEdge = false;
            }

            Vector3 screenPosition = cam.WorldToScreenPoint(mainTarget.transform.position);

            _isTopEdge = screenPosition.y >= 0.8f * _screenHeight;
            _isBottomEdge = screenPosition.y <= 0.2f * _screenHeight;
            _isLeftEdge = screenPosition.x <= 0.3f * _screenWidth;
            _isRightEdge = screenPosition.x >= 0.7f * _screenWidth;
        }

        private void FixedUpdate() {
        
            if (mainTarget == null || targets.Count == 0)
                return;

            SwitchSingleTargetToTheClosetGround(_isFreeFalling);
            //Zoom();

            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
        }

        private bool SingleTargetIsMainTarget() =>
            targets.Count == 1 && targets[0] != null && targets[0] == mainTarget.transform;

        private bool SingleTargetIsCameraFocus() =>
            targets.Count == 1 && targets[0] != null && targets[0] == cameraFocus.transform;

        private void UpdateScreenOffset() {

            //RecalculateYOffsetByMaxZoom();
        
            UpdateScreenOffsetVertical();
            UpdateScreenOffsetHorizontal();
            
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

        private void UpdateScreenOffsetHorizontal() {

            if (targets.Count > 1 && AreTargetsCloseOnVertical() || SingleTargetIsCameraFocus()) {
                offset.x = 0;

            } else {
                RecalculateXOffsetBySpeed();
            }
            
        }

        void RecalculateXOffsetBySpeed() {

            var facingRight = mainTarget.State.IsFacingRight();
            var heroFacingToHorizon = facingRight && _isLeftEdge || !facingRight && _isRightEdge;//&& velocity.magnitude < 10

            // the larger x speed is applied, the larger x offset is
            if (targets.Count < 2 && Mathf.Abs(mainTarget.Velocity.x) > 0.1f) //&& !target.State.State == Definitions.CharacterState.StandingUp)
            {
                var percentage = 0f;
                if (heroFacingToHorizon) {

                    percentage = _isLeftEdge ? -1f : _isRightEdge ? 1f : 0f;
                } else {

                    percentage = ((float)mainTarget.Velocity.x) / (10); // 10 as the min velocity for any player to be considered as run
                    percentage = Mathf.Clamp(percentage, -1, 1);
                    //Debug.Log(percentage);

                }

                offset.x = 10 * percentage; // 10 is the max distance to move the offset forward
            }
             
        }
    
        private void UpdateScreenOffsetVertical() {

            var singleTarget = SingleTargetIsMainTarget();
            if (_isFreeFalling && singleTarget) {
                offset.y = FallingYOffset;

            } else if (_isTopEdge && singleTarget) {
                offset.y = YMinZoomToAddOffset * 2.5f;

            } else if (_isBottomEdge && singleTarget) {
                offset.y = -YMinZoomToAddOffset;

            } else if (AreTargetsCloseOnVertical()) {
                offset.y = YMinZoomToAddOffset;
        
            } else {
                offset.y = 0;
            }
        }

        private bool AreTargetsCloseOnVertical() {
        
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

            var smoothing = Mathf.Abs(velocity.magnitude) < MinScreenVelocityForXCenterSmoothing && Mathf.Abs(mainTarget.Velocity.magnitude) < 20f ? XBackToCenterSmoothing : smoothTime;

            transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothing);
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
