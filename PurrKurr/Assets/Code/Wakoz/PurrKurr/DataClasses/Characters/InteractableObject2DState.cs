using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Ropes;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {
    [System.Serializable]
    public sealed class InteractableObject2DState {

        public Definitions.ObjectState CurrentState => _currentState;
        private Definitions.ObjectState _currentState;

        private bool _facingRight = true;
        private bool _wasGrounded = false;
        private bool _isGrounded = false;
        private bool _wasCeiling = false;
        private bool _isCeiling = false;
        private bool _wasRightWall = false;
        private bool _isRightWall = false;
        private bool _wasLeftWall = false;
        private bool _isLeftWall = false;
        private bool _wasTraversable = false;
        private bool _isTraversable = false;
        private bool _isInTraversableCrouchArea = false;

        public Vector2 ClosestSurfaceDir => _closestSurfaceDir;
        private Vector2 _closestSurfaceDir = Vector2.zero;
        public Vector2 ClosestPoint => _closestSurfacePoint;
        private Vector2 _closestSurfacePoint = Vector2.zero;
        public Vector2 AlternativeSurfaceDir => _alternativeSurfaceDir;
        private Vector2 _alternativeSurfaceDir = Vector2.zero;
        private int _collLayer = -1;

        public bool IsInCrouchOnlyArea => _isInTraversableCrouchArea;

        private Definitions.ActionType _combatAbility;
        private float _moveAnimation;
        private float _interruptibleAnimation;
        private float _uninterruptibleAnimation;
        private float _cayoteEndTime;
        private float _jumpingEndTime;
        private bool _hasLanded;
        private bool _isCrouching;
        private bool _isStanding;
        private bool _chargingSuper;
        private Vector2 _position;
        private Vector2 _velocity;
        private bool _hasGroundBeneathByRayCast;
        private Definitions.NavigationType _navigationDirection;
        private IInteractableBody _grabberAnchor;
        private IInteractableBody _grabbedAnchor;
        private IInteractableBody _latestInteraction;
        private const float _cayoteTimeDuration = 0.2f;

        public void SetAsLanded() {
            _hasLanded = true;
            _hasGroundBeneathByRayCast = true;
        }

        public void SetState(Definitions.ObjectState newState) {
            _currentState = newState;
        }

        public void SetChargingSuper(bool isActive) => _chargingSuper = isActive;

        public bool isChargingSuper() => _chargingSuper;

        public void DiagnoseState(Vector3 hitPoint, Vector2 collDir, int collLayer, Vector2 position, Vector2 velocity, bool hasGroundBeneathByRayCast, bool isInTraversableSurface, bool isInTraversableCrouchArea) {
            
            _wasGrounded = _isGrounded; _isGrounded = false;
            _wasCeiling = _isCeiling; _isCeiling = false;
            _wasRightWall = _isRightWall; _isRightWall = false;
            _wasLeftWall = _isLeftWall; _isLeftWall = false;
            _wasTraversable = _isTraversable; _isTraversable = false;
            _position = position;
            _velocity = velocity;
            _hasGroundBeneathByRayCast = hasGroundBeneathByRayCast;

            _closestSurfaceDir = collDir;
            _closestSurfacePoint = hitPoint; // used to register the location of hitpoint
            _collLayer = collLayer;

            /*if (collLayer == -1) {
                // maybe can skip because there is nothing to check collision with?
            }*/

            if (collDir.y <= -0.5f) {
                _isGrounded = true;

            } else if (collDir.y >= 0.5f) {
                _isCeiling = true;
            }

            // validating wall hitpoint - that is not the too low beneath the player to be a wall 
            if (collDir.y >= -0.5f) {
                if (collDir.x <= -0.7) {
                    _isLeftWall = true;
                } else if (collDir.x >= 0.7) {
                    _isRightWall = true;
                }
            }

            _isTraversable = isInTraversableSurface;
            _isInTraversableCrouchArea = isInTraversableCrouchArea;

            UpdateState();
            
            UpdateCayoteTime();
        }

        private void UpdateCayoteTime() {
            if (_wasGrounded && !_isGrounded && !_isLeftWall && !_isRightWall && !IsJumping() && _velocity.y < 4 && _velocity.magnitude > 1) {
                _cayoteEndTime = Time.time + _cayoteTimeDuration;
            }
        }

        private void UpdateState() {

            if (_combatAbility != Definitions.ActionType.Empty) {

                if (IsMoveAnimation()) {

                    if (_combatAbility == Definitions.ActionType.Attack) {
                        SetState(Definitions.ObjectState.Attacking);
                        return;
                    } else if (_combatAbility == Definitions.ActionType.Grab) {
                        SetState(Definitions.ObjectState.Grabbing);
                        return;
                    }

                } else if (_combatAbility == Definitions.ActionType.Block) {
                    SetState(Definitions.ObjectState.Blocking);
                    return;

                } else if (_combatAbility == Definitions.ActionType.Projectile) {
                    SetState(Definitions.ObjectState.AimingProjectile);
                    return;

                } else if (_combatAbility == Definitions.ActionType.Rope) {
                    SetState(Definitions.ObjectState.AimingRope);
                    return;

                } else if (_combatAbility == Definitions.ActionType.Special) {
                    SetState(Definitions.ObjectState.InterruptibleAnimation);
                    return;

                } else if (_combatAbility == Definitions.ActionType.Jump && IsAiming()) {
                    SetState(Definitions.ObjectState.AimingJump);
                    return;

                } else if (!IsMoveAnimation() && IsInterraptibleAnimation()) {
                    SetState(Definitions.ObjectState.InterruptibleAnimation);
                    return;

                } else {
                    SetActiveCombatAbility(Definitions.ActionType.Empty);
                }

            }

            // todo: add the UninterruptibleAnimation
            if (_currentState == Definitions.ObjectState.InterruptibleAnimation && _combatAbility == Definitions.ActionType.Empty && !IsInterraptibleAnimation()) {
                SetState(Definitions.ObjectState.Grounded);
                SetAsLanded();
                return;
            }

            if (!_hasLanded && _currentState is Definitions.ObjectState.Falling or Definitions.ObjectState.Jumping && (!_wasGrounded && _isGrounded || ( IsFrontWall() || IsBackWall() ) && _hasGroundBeneathByRayCast)) {
                SetState(Definitions.ObjectState.Landed);
                SetAsLanded();
                return;
            }

            if ( (_hasGroundBeneathByRayCast || _hasLanded) && (_isGrounded || IsTouchingAnySurface())) {

                if (!_hasLanded && _hasGroundBeneathByRayCast) {
                    SetAsLanded();
                }

                if (_isCrouching) {
                    SetState(Definitions.ObjectState.Crouching);
                    return;
                } else if (_isStanding) {
                    SetState(Definitions.ObjectState.StandingUp);
                    return;
                } else if (Velocity.magnitude > 2 && (_navigationDirection is Definitions.NavigationType.Right or Definitions.NavigationType.Left)) {
                    SetState(Definitions.ObjectState.Running);
                    return;
                } else if (_wasGrounded && _isGrounded /*&& _currentState == Definitions.CharacterState.Landed*/) {
                    SetState(Definitions.ObjectState.Grounded);
                    return;
                } else if (!_wasGrounded && !_isGrounded && IsFrontWall() && _hasLanded && _velocity.magnitude < 0.1f) {
                    SetState(Definitions.ObjectState.Grounded);
                    return;
                }

            }

            if (IsJumping()) {
                SetState(Definitions.ObjectState.Jumping);
                _hasLanded = false;
                return;

            } else if (IsGrabbing() && GetGrabbedTarget() is RopeLinkController) {
                SetState(IsNotMoving() ? Definitions.ObjectState.RopeClinging : Definitions.ObjectState.RopeClimbing);
                //_hasLanded = false;
                return;

            } else if (IsClinging()) {
                SetState(IsNotMoving() ? Definitions.ObjectState.WallClinging : Definitions.ObjectState.WallClimbing);
                //_hasLanded = false;
                return;

            } else if (_isTraversable) {
                SetState(Definitions.ObjectState.TraversalRunning);
                return;

            } else if (_closestSurfaceDir == Vector2.zero && _closestSurfacePoint == Vector2.zero && !IsTouchingAnySurface() && _alternativeSurfaceDir == Vector2.zero) {
                SetState(IsUpwardMovement() ? Definitions.ObjectState.Jumping : Definitions.ObjectState.Falling);
                _hasLanded = false;
                return;
            }
        }

        public void SetMoveAnimation(float time) {
            _moveAnimation = time;
        }

        public void SetInterruptibleAnimation(float time) {
            _interruptibleAnimation = time;
        }

        public void SetUninterruptibleAnimation(float time) {
            _uninterruptibleAnimation = time;
        }

        public void SetJumping(float time) {
            _jumpingEndTime = time;
        }

        public void SetCrouchOrStandingByUpDownInput(Definitions.NavigationType verticalInput) {

            var isNotMoving = IsNotMoving();//&& CurrentState != Definitions.CharacterState.Running;
            var isOnAnySurface = _hasGroundBeneathByRayCast && IsTouchingAnySurface();
            var isCrouchingKey = verticalInput is Definitions.NavigationType.Down or Definitions.NavigationType.DownLeft or Definitions.NavigationType.DownRight;
            var isStandingUpKey = verticalInput is Definitions.NavigationType.Up or Definitions.NavigationType.UpLeft or Definitions.NavigationType.UpRight
                 && !_wasCeiling && !_isCeiling;

            var isCrouching = isNotMoving && isOnAnySurface && isCrouchingKey;
            var isStanding = isNotMoving && isOnAnySurface && isStandingUpKey || IsGrabbing();

            _isCrouching = isCrouching;
            _isStanding = isStanding;
        }
        
        public bool IsTouchingAnySurface() => _wasGrounded || _isGrounded || _wasRightWall || _isRightWall || _wasLeftWall || _isLeftWall || _wasCeiling || _isCeiling;

        public bool IsCeiling() => _wasCeiling || _isCeiling;

        public bool IsCoyoteTime() => Time.time < _cayoteEndTime;

        public bool IsUpwardMovement() => _velocity.y > 2.5;

        public bool IsNotMoving() => Velocity.magnitude < 10;

        public bool IsMoveAnimation() => Time.time < _moveAnimation;

        public bool IsInterraptibleAnimation() => Time.time < _interruptibleAnimation;

        public bool CanPerformAction() =>
            !IsMoveAnimation() && 
            _currentState is Definitions.ObjectState.Crouching or Definitions.ObjectState.StandingUp or
                Definitions.ObjectState.Jumping or Definitions.ObjectState.Falling or
                Definitions.ObjectState.AerialJumping or Definitions.ObjectState.Grabbing or
                Definitions.ObjectState.Grounded or Definitions.ObjectState.Landed or
                Definitions.ObjectState.Running or Definitions.ObjectState.AirGliding or 
                Definitions.ObjectState.WallClimbing or Definitions.ObjectState.RopeClinging or
                Definitions.ObjectState.TraversalRunning or Definitions.ObjectState.WallClinging or 
                Definitions.ObjectState.Alive or Definitions.ObjectState.Blocking or
                Definitions.ObjectState.AimingRope or Definitions.ObjectState.AimingProjectile or 
                Definitions.ObjectState.InterruptibleAnimation or Definitions.ObjectState.AimingJump;
        
        public bool IsJumping() => Time.time < _jumpingEndTime;

        public bool IsGrounded() => _isGrounded;

        public bool IsCrouching() => _isCrouching;

        public bool IsCrouchingAndNotFallingNearWall() => _isCrouching && (_hasGroundBeneathByRayCast && (_isGrounded || _isLeftWall || _isRightWall) );

        public bool IsStandingUp() => _isStanding;

        public int GetFacingRightAsInt() => _facingRight ? 1 : -1;

        public bool IsFacingRight() => _facingRight;

        public void SetFacingRight(bool isFacingRight) => _facingRight = isFacingRight;

        public bool IsFrontWall() => _facingRight && _isRightWall || !_facingRight && _isLeftWall;
        public bool IsBackWall() => _facingRight && _isLeftWall || !_facingRight && _isRightWall;

        private Collider2D _clingingOnObject;
        public Collider2D GetClingableObject() => _clingingOnObject;
        public bool IsClinging() => _clingingOnObject != null;
        public void SetClinging(Collider2D objectToClingOn) => _clingingOnObject = objectToClingOn;
        public void StopClinging() => SetClinging(null);

        private float _aimingEndTime;
        public bool IsAiming() => Time.time < _aimingEndTime;
        public void SetAiming(float durationInMilliseconds) => _aimingEndTime = Time.time + durationInMilliseconds;
        public void StopAiming() => SetAiming(0);

        public Quaternion ReturnForwardDirByTerrainQuaternion()  {
            
            if (IsGrabbed()) {
                Quaternion surfaceQuaternion = Quaternion.LookRotation(Vector3.forward, Vector2.down);
                return surfaceQuaternion;
            }
            
            if (_closestSurfaceDir != Vector2.zero && _closestSurfacePoint != Vector2.zero && IsTouchingAnySurface()) {
                Quaternion surfaceQuaternion = Quaternion.LookRotation(Vector3.forward, _closestSurfaceDir);
                return surfaceQuaternion;
            }

            if (HasAnySurfaceAround()) {
                Quaternion surfaceQuaternion = Quaternion.LookRotation(Vector3.forward, _alternativeSurfaceDir);
                return surfaceQuaternion;
            }

            /*if (IsClinging()) {
                var clingableCollider = _clingingOnObject.GetComponent<Collider2D>();
                Quaternion surfaceQuaternion = Quaternion.LookRotation(Vector3.forward, clingableCollider.ClosestPoint(_position));
                return surfaceQuaternion;
            }*/

            Quaternion velocityQuaternion = Quaternion.LookRotation(Vector3.forward, _velocity);
            return velocityQuaternion;
            
        }

        public bool HasAnySurfaceAround() => _closestSurfaceDir != Vector2.zero && _hasLanded;

        public Vector2 Velocity => _velocity;

        public void SetNavigationDir(Definitions.NavigationType navigationDirection) {
            _navigationDirection = navigationDirection;
        }

        public Definitions.NavigationType NavigationDir => _navigationDirection;

        public void SetActiveCombatAbility(Definitions.ActionType combatAbility) => _combatAbility = combatAbility;

        public bool CanPerformJump(bool isStateConsideredAsGrounded) {

            var isVerticalVelocityExceeding = Velocity.y < 10;

            var isGroundedOrCayoteTime = (IsTouchingAnySurface() && isStateConsideredAsGrounded || IsCoyoteTime() || _hasLanded);
            return isVerticalVelocityExceeding && isGroundedOrCayoteTime && !IsJumping() && CurrentState != Definitions.ObjectState.Crouching;
        }

        public void SetAsGrabbed(IInteractableBody grabber) => _grabberAnchor = grabber;

        public bool IsGrabbed() => _grabberAnchor != null;

        public void SetAsGrabbing(IInteractableBody grabbedBody) => _grabbedAnchor = grabbedBody;

        public bool IsGrabbing() => _grabbedAnchor != null;

        public IInteractableBody GetGrabbedTarget() => _grabbedAnchor;

        public void MarkLatestInteraction(IInteractableBody latestInteraction) => _latestInteraction = latestInteraction;
        public IInteractableBody GetLatestInteraction() => _latestInteraction;

        public bool CanMoveOnSurface() => _hasLanded;

        public bool CanPerformContinousRunning() {

            if (!_hasLanded) {
                return false;
            }
            
            if (IsCeiling() && Mathf.Abs(_velocity.x) < 10 ) {
                return false;
            }

            if (IsFrontWall() && _velocity.magnitude < 20) {
                return false;
            }

            return true;
        }

        
    }
}