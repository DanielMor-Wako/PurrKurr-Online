using UnityEngine;
using System.Collections.Generic;
using System;
using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.Utils.CameraUtils;
using System.Linq;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    public sealed class CameraCharacterHandler : IBindableHandler, ICameraNotifier
    {
        public Transform FocusTransform { get; private set; }
        public Transform CharacterTransform { get; private set; }
        public CameraHandler CameraHandler { get; private set; }

        private Character2DController CharacterController;
        private GameplayController GameEvents;
        private Camera _cam;

        private List<Transform> _characterSet;
        private List<Transform> _focusSet;
        private List<Transform> _characterAndFocusSet;
        private List<Transform> _nearbyInteractables;
        private readonly LifeCycleData _defaultLifeCycle = new();

        [Tooltip("Regulates the value of velocity by the player, used to calculate future position of the player")]
        private readonly float _velocityMulti = 0.5f;
        private readonly float yOffsetFromCenterWhenCeiling = 25;

        public CameraCharacterHandler(CameraHandler cameraHandler, GameplayController gameEvents, Transform CameraFocusTransform, Transform mainCharacter)
        {
            FocusTransform = CameraFocusTransform;
            CharacterTransform = mainCharacter;
            GameEvents = gameEvents;
            CameraHandler = cameraHandler;
            _cam = CameraHandler.CameraComponent;

            _characterSet = new List<Transform> { CharacterTransform };
            _focusSet = new List<Transform> { FocusTransform };
            _characterAndFocusSet = new List<Transform> { CharacterTransform, FocusTransform };
            _nearbyInteractables = new List<Transform>();
        }

        public void NotifyCameraChange(BaseCamera camera)
        {
            CameraHandler.EnqueueCamera(camera);
        }

        public void Unbind()
        {
            if (GameEvents == null)
                return;

            GameEvents.OnNewHero -= HandleNewHero;
            GameEvents.OnHeroReposition -= HandleReposition;
            GameEvents.OnCameraFocusPoint -= HandleCameraFocusPoint;
            GameEvents.OnCameraFocusTargets -= HandleCameraFocusTargets;
            GameEvents.OnInteractablesEntered -= HandleNearbyTargetEntered;
            GameEvents.OnInteractablesExited -= HandleNearbyTargetExited;
            GameEvents.OnStateChanged -= HandleStateChanged;
            GameEvents.OnAimingAction -= HandleAiming;
            GameEvents.OnAimingActionEnd -= HandleAimingDefault;

            GameEvents = null;
        }

        public void Bind()
        {
            if (GameEvents == null)
                return;

            GameEvents.OnNewHero += HandleNewHero;
            GameEvents.OnHeroReposition += HandleReposition;
            GameEvents.OnCameraFocusPoint += HandleCameraFocusPoint;
            GameEvents.OnCameraFocusTargets += HandleCameraFocusTargets;
            GameEvents.OnInteractablesEntered += HandleNearbyTargetEntered;
            GameEvents.OnInteractablesExited += HandleNearbyTargetExited;
            GameEvents.OnStateChanged += HandleStateChanged;
            GameEvents.OnAimingAction += HandleAiming;
            GameEvents.OnAimingActionEnd += HandleAimingDefault;
        }

        private void UpdateHandler(CameraData data, Action processActionCallback = null)
        {
            CameraHandler.EnqueueCamera(data, processActionCallback);
        }

        private void FollowFocusPoint(Vector3 targetPos)
        {
            FocusTransform.transform.position = targetPos;
            FollowSingleTarget(FocusTransform);
        }

        private void FollowSingleTarget(Transform target)
        {
            UpdateHandler(new CameraData(new Transform[1] { target } ));
        }

        private void FollowMultipleTargets(List<Transform> targets)
        {
            UpdateHandler(new CameraData(targets, new LifeCycleData(1.5f)));
        }

        private void HandleCameraFocusPoint(Vector2 vector)
        {
            FollowFocusPoint(vector);
        }

        private void HandleCameraFocusTargets(List<Transform> list)
        {
            FollowMultipleTargets(list);
        }

        private void HandleNewHero(Character2DController characterController)
        {
            Debug.Log("GameEvent: new hero");
            CharacterTransform = characterController.transform;
            CharacterController = characterController;

            FollowSingleTarget(CharacterTransform);
        }

        private void HandleReposition(Vector3 newPosition)
        {
            CharacterTransform.position = newPosition;
            FocusTransform.position = newPosition;
            CameraHandler.SetPositionAndSize(newPosition, 0);
        }

        private void HandleAiming(Definitions.ActionType type, Vector2 position, Quaternion quaternion, bool hitData, Vector3[] lineTrajectory)
        {
            Debug.Log("GameEvent: aiming (values) " + type);
            if (CameraHandler == null)
            {
                Debug.LogError("camera handler is missing");
                return;
            }
            
            switch (type)
            {
                case Definitions.ActionType.Rope:
                    FocusTransform.transform.position = position;
                break;

                case Definitions.ActionType.Jump:
                    var totalLinesCount = lineTrajectory.Length;
                    // -> less than or equal to 6 means endPosition is too close to character, therefore using character position
                    FocusTransform.transform.position =
                    totalLinesCount > 6 ?
                    lineTrajectory[totalLinesCount - 1]
                    : CharacterTransform.position;
                break;

                case Definitions.ActionType.Projectile:
                    FocusTransform.transform.position = position;
                break;
            }
        }

        private void HandleAimingDefault(Definitions.ActionType type)
        {
            return;
            Debug.Log("GameEvent: aiming (default) " + type);
            if (CameraHandler == null)
            {
                Debug.LogError("camera handler is missing");
                return;
            }
        }


        private void HandleNearbyTargetEntered(Collider2D d)
        {
            //Debug.Log("GameEvent: Handle Nearby Target Entered " + d.transform.name);
            _nearbyInteractables.Add(d.transform);
            //HandleStateChanged(CharacterController.State);
        }

        private void HandleNearbyTargetExited(Collider2D d)
        {
            //Debug.Log("GameEvent: Handle Nearby Target Exited " + d.transform.name);
            _nearbyInteractables.Remove(d.transform);
            //HandleStateChanged(CharacterController.State);
        }

        private void HandleStateChanged(InteractableObject2DState state)
        {
            // Debug.Log("GameEvent: new state "+ state.CurrentState);
            if (CameraHandler == null)
            {
                Debug.LogError("camera handler is missing");
                return;
            }

            // todo: break to small functions. GetCameraData(state). GetActionForState(state)
            switch (state.CurrentState)
            {
                case Definitions.ObjectState.Jumping:
                    //FocusTransform.position = CharacterTransform.position;
                    float followSpeed = 2f;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterAndFocusSet,
                        _defaultLifeCycle,
                        //new CameraTransitionsData() { SmoothSpeed = 10f, MaxZoom = 6, MinZoom = 8 },
                        new CameraTransitionsData() { SmoothSpeed = 5f, MaxZoom = 6, MinZoom = 25, ZoomSpeed = 2 },
                        new CameraOffsetData() { SmoothSpeed = 1f, TargetOffset = Vector3.up * 2 }),
                            () =>
                            {
                                UpdateFocusByVelocityDirWhenNoFrontWallOrCeiling(FocusTransform, 5, 5f);
                                //MoveTarget(FocusTransform, CharacterTransform, followSpeed);
                            });
                    break;
                
                case Definitions.ObjectState.Falling:
                    //FocusTransform.position = CharacterTransform.position;
                    float distanceToCheckWhenFreeFalling = 80f;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterAndFocusSet,
                        _defaultLifeCycle,
                        new CameraTransitionsData() { SmoothSpeed = 5f, MaxZoom = 6, ZoomSpeed = 5 },
                        new CameraOffsetData() { SmoothSpeed = 2f }), 
                            () => UpdateFocusByFalling(distanceToCheckWhenFreeFalling));
                    break;
                
                case var _ when state.CurrentState is Definitions.ObjectState.Grounded:
                    var centerOnPlayerSmoothSpeed = 5f;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterAndFocusSet,
                        _defaultLifeCycle,
                        new CameraTransitionsData() { SmoothSpeed = 2f/*5f*/, MaxZoom = 6, MinZoom = 25, ZoomSpeed = 2 },
                        new CameraOffsetData() { SmoothSpeed = 5, TargetOffset = Vector3.up * 3 }),
                            () => {
                                //UpdateZoomByVelocity(1f, 6f, 25f);
                                MoveTarget(FocusTransform, CharacterTransform, centerOnPlayerSmoothSpeed);
                            });
                    break;

                case Definitions.ObjectState.Running:
                case Definitions.ObjectState.RopeClinging:
                case Definitions.ObjectState.TraversalRunning:
                    var trackingSmoothSpeed = 10f;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterAndFocusSet,
                        _defaultLifeCycle,
                        new CameraTransitionsData() { SmoothSpeed = 2f/*8f*/, MaxZoom = 6, MinZoom = 25, ZoomSpeed = 2 },
                        new CameraOffsetData() { TargetOffset = Vector3.up * 3 }),
                            () => {
                                //UpdateZoomByVelocity(1f, 8f, 25f);
                                UpdateFocusByVelocityDirWhenNoFrontWallOrCeiling(FocusTransform, trackingSmoothSpeed, 5f);
                            });
                break;

                case Definitions.ObjectState.StandingUp:
                    FocusTransform.position = CharacterTransform.position;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterSet,
                        _defaultLifeCycle,
                        new CameraTransitionsData() { SmoothSpeed = 5f },
                        new CameraOffsetData() { TargetOffset = Vector2.up * 4 }));
                    break;

                case Definitions.ObjectState.Crouching:
                    FocusTransform.position = CharacterTransform.position;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterSet,
                        _defaultLifeCycle,
                        new CameraTransitionsData() { SmoothSpeed = 5f },
                        new CameraOffsetData() { TargetOffset = Vector2.up }));
                    break;

                case Definitions.ObjectState.AimingRope:
                    FocusTransform.position = CharacterTransform.position;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterAndFocusSet,
                        _defaultLifeCycle,
                        new CameraTransitionsData() { SmoothSpeed = 5f, MaxZoom = 7, MinZoom = 7, ZoomSpeed = 5 },
                        new CameraOffsetData() { SmoothSpeed = 2f, TargetOffset = Vector3.up * 3 }));
                break;

                case Definitions.ObjectState.AimingJump:
                    FocusTransform.position = CharacterTransform.position;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterAndFocusSet,
                        _defaultLifeCycle,
                        new CameraTransitionsData() { SmoothSpeed = 3f, MaxZoom = 7, ZoomSpeed = 3, ZoomLimiter = 80 },
                        new CameraOffsetData() { SmoothSpeed = 2f, TargetOffset = Vector3.up * 3 }),
                        null); //() => UpdateFocusByAiming());
                break;

                case Definitions.ObjectState.AimingProjectile:
                    FocusTransform.position = CharacterTransform.position;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterAndFocusSet,
                        _defaultLifeCycle,
                        new CameraTransitionsData() { SmoothSpeed = 5f, MaxZoom = 7, MinZoom = 7, ZoomSpeed = 5 },
                        new CameraOffsetData() { SmoothSpeed = 2f, TargetOffset = Vector3.up * 3 }),
                        null); //() => UpdateFocusByAiming());
                break;

                case Definitions.ObjectState.InterruptibleAnimation or Definitions.ObjectState.UninterruptibleAnimation:
                    FocusTransform.position = CharacterTransform.position;
                    var smoothSpeed = 2f;
                    CameraHandler.EnqueueCamera(
                        new CameraData(_characterAndFocusSet,
                        _defaultLifeCycle,
                        new CameraTransitionsData() { SmoothSpeed = 5f, MaxZoom = 5, MinZoom = 7, ZoomSpeed = 5 },
                        new CameraOffsetData() { SmoothSpeed = 10f, TargetOffset = Vector3.up * 3 }),
                            () => MoveTargetAxis(FocusTransform, CharacterTransform, smoothSpeed, false, false));
                break;

                //case var _ when state.CurrentState is Definitions.ObjectState.Running or Definitions.ObjectState.RopeClinging or Definitions.ObjectState.TraversalRunning:
                //break;

                default:
                    break;
            }
        }

        private IEnumerable<Transform> GetWithNearbyTargets(ref List<Transform> initialSet)
        {
            if (_nearbyInteractables.Count == 0)
            {
                return initialSet;
            }

            return initialSet.Concat(_nearbyInteractables);
        }

        private void LerpToTargetPosition(Transform target, Vector3 endPosition, float speed)
        {
            target.position = Vector2.Lerp(target.position, endPosition, speed * Time.deltaTime);
        }

        private void MoveTarget(Transform transform, Transform targetTransform, float speed)
        {
            var newPos = Vector2.zero;

            if (_nearbyInteractables.Count > 0)
            {
                newPos = CameraUtils.GetAveragePoint(ref _nearbyInteractables);
            }
            else
            {
                newPos = (Vector2)targetTransform.position + (CharacterController.Velocity * _velocityMulti);
            }

            LerpToTargetPosition(transform, newPos, speed);
        }

        private void MoveTargetAxis(Transform target, Transform endTarget, float speed, bool lockXAxis = false, bool lockYAxis = false)
        {
            Vector3 endPosition = new Vector3()
            {
                x = lockXAxis ? target.position.x : endTarget.position.x,
                y = lockYAxis ? target.position.y : endTarget.position.y
            };

            LerpToTargetPosition(target, endPosition, speed);
        }

        private void UpdateFocusByFalling(float distanceToCheckWhenFreeFalling)
        {
            var minDistanceBetweenCharacterToFocus = 22f;
            var freeFallingHitPoint = CharacterController.IsFreeFalling(distanceToCheckWhenFreeFalling);
            var isLargeFallingDistance = (CharacterController.LegsPosition.y - freeFallingHitPoint.y) > minDistanceBetweenCharacterToFocus;
            var isFallingHighVelocity = CharacterController.Velocity.y <= -20;

            if (freeFallingHitPoint != Vector3.zero)
            {
                LerpToTargetPosition(FocusTransform, freeFallingHitPoint, 4);
            }
            
            var newZoomSpeed = isLargeFallingDistance && isFallingHighVelocity ? 5 : 2;
            // minZoom as default 6 is the same as the player normal minZoom size , 25 is the highest zoom so camera can scale up
            var minZoom = isFallingHighVelocity && isLargeFallingDistance ? 35 : 6;
            minZoom = 35;

            var currentCameraData = CameraHandler.CurrentCamera;
            currentCameraData.SetZoom(newZoomSpeed, minZoom, 6);
            currentCameraData.SetTargetOffset(isFallingHighVelocity && isLargeFallingDistance ? Vector3.down * 3 : Vector3.up * 2);
            
            /*if (!isLargeFallingDistance && isFallingHighVelocity && currentCameraData.Data.TargetsCount > 1)
            {
                currentCameraData.SwitchData(
                    new CameraData(_focusSet,
                    _defaultLifeCycle,
                    new CameraTransitionsData() { SmoothSpeed = 6f, MaxZoom = 7, ZoomSpeed = 5 },
                    new CameraOffsetData() { SmoothSpeed = 10f, TargetOffset = Vector3.up * 2 }));
            }*/
        }

        private void UpdateFocusByVelocityDirWhenNoFrontWallOrCeiling(Transform transform, float speedWhenOnSurface, float speedBackToCenter)
        {
            var endPosition = (Vector2)CharacterTransform.position;
            var state = CharacterController.State;
            var isCeiling = state.IsCeiling();
            var isFrontWallOrCeiling =
                state.IsFrontWall() && isCeiling
                || isCeiling;

            if (!isFrontWallOrCeiling)
            {
                endPosition += CharacterController.Velocity * _velocityMulti;
            }
            else if (isCeiling)
            {
                var offsetFromCenter = CameraUtils.GetOffsetFromCenter(endPosition, ref _cam);
                endPosition.y += yOffsetFromCenterWhenCeiling * offsetFromCenter.y;
            }
            
            LerpToTargetPosition(transform, endPosition, !isFrontWallOrCeiling ? speedWhenOnSurface : speedBackToCenter);
        }

        private void UpdateZoomByVelocity(float zoomSpeed = 2, float lowVelocityMaxZoom = 6, float highVelocityMaxZoom = 25)
        {
            var mag = (CharacterTransform.position - FocusTransform.position).magnitude / 15;
            var targetMaxZoom = lowVelocityMaxZoom + mag * (highVelocityMaxZoom - lowVelocityMaxZoom);
            CameraHandler.CurrentCamera.SetZoom(zoomSpeed, lowVelocityMaxZoom, targetMaxZoom);
        }
    }
}
