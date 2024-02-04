using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {
    [System.Serializable]
    public class Character2DState {

        public Definitions.CharacterState CurrentState => _currentState;
        private Definitions.CharacterState _currentState;
        
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

        public Vector2 farSurfaceDir => _farSurfaceDir;
        private Vector2 _farSurfaceDir = Vector2.zero;
        public Vector2 ClosestSurfaceDir => _closestSurfaceDir;
        private Vector2 _closestSurfaceDir = Vector2.zero;
        public Vector2 ClosestPoint => _closestSurfacePoint;
        private Vector2 _closestSurfacePoint = Vector2.zero;

        private Definitions.ActionType _combatAbility;
        private float _moveAnimation;
        private float _interruptibleAnimation;
        private float _uninterruptibleAnimation;
        private float _cayoteEndTime;
        private float _jumpingEndTime;
        private bool _hasLanded;
        private bool _isCrouching;
        private bool _isStanding;
        private Vector2 _velocity;
        private bool _hasGroundBeneathByRayCast;
        private Definitions.NavigationType _navigationDirection;
        private IInteractableBody _grabberAnchor;
        private IInteractableBody _grabbedAnchor;
        private IInteractableBody _latestInteraction;
        private const float _cayoteTimeDuration = 0.4f;

        public void SetAsLanded() {
            _hasLanded = true;
            _hasGroundBeneathByRayCast = true;
        }

        public void SetState(Definitions.CharacterState newState) {
            _currentState = newState;
        }

        public void DiagnoseState(Vector3 hitPoint, Vector2 collDir, Vector2 farSurfaceDir, Vector2 velocity, bool hasGroundBeneathByRayCast) {
            
            _wasGrounded = _isGrounded; _isGrounded = false;
            _wasCeiling = _isCeiling; _isCeiling = false;
            _wasRightWall = _isRightWall; _isRightWall = false;
            _wasLeftWall = _isLeftWall; _isLeftWall = false;
            _wasTraversable = _isTraversable; _isTraversable = false;
            _velocity = velocity;
            _hasGroundBeneathByRayCast = hasGroundBeneathByRayCast;

            _closestSurfaceDir = collDir;
            _closestSurfacePoint = hitPoint; // used to register the location of hitpoint
            _farSurfaceDir = farSurfaceDir;
            GameObject objTemp_WallEdgeParent = null; // used as temporary vefire wallEdgeParent is set
            GameObject obj_WallEdgeParent = null;  // checks if player should FrontWallHang, when null it does nothing.. otherwise locks the player to the front wall position

            // bitshift to get a single layer
            //LayerMask layerMask = 1 << collLayer;
            
            /*if (collLayer == -1) {
                // maybe can skip because there is nothing to check collision with?
            }*/
            
            if (collDir.y <= -0.5f) {
                _isGrounded = true;

            } else if (farSurfaceDir.y >= 0.5f) {
                _isCeiling = true;
            }

            // validating wall hitpoint - that is not the too low beneath the player to be a wall 
            if (farSurfaceDir.y >= -0.5f) {
                if (farSurfaceDir.x <= -0.7) {
                    _isLeftWall = true;
                } else if (farSurfaceDir.x >= 0.7) {
                    _isRightWall = true;
                }
            }
            
            /*if (_closestSurfaceDir == Vector2.zero && !_isCrouching) {
                _closestSurfaceDir = collDir;
                _closestSurfacePoint = hitPoint;
            } else if (Mathf.Abs(_closestSurfacePoint.x) <= -0.7f || _isCrouching) {
                // only overide rotation if character is already grounded and there is new dir from wall or ceil
                _closestSurfaceDir = collDir;
                _closestSurfacePoint = hitPoint;
                //if (collDir != Vector2.zero) { Debug.Log("set closest surface to " + m_ClosestSurfaceDir); }
            }*/
            
            /*if (collLayer == LayerMask.NameToLayer("Traversable")) {
                _isTraversable = true;
            }*/

            /*
                // handle climbing new front wall
            if (_isFrontWall && (!joint.enabled || _isRopeSwing)) {
                Vector3 dir = _closestSurfaceDir;
                //float frontWallAngle = Mathf.Abs(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f);
                float maxAngleForClimbing = 55f; // starts as 90 and then decreased by this value (based on facing direction)
                //Debug.Log("front wall valid? frontWallAngle=" + Mathf.RoundToInt(frontWallAngle));
                //bool isFrontWallAngleValid = (m_FacingRight && frontWallAngle < maxAngleForClimbing && frontWallAngle >= -maxAngleForClimbing || !m_FacingRight && frontWallAngle > (90 - maxAngleForClimbing) && frontWallAngle <= 90);
                //bool isFrontWallAngleValid = frontWallAngle > maxAngleForClimbing && dir.y >= 0;
                //Debug.Log("front wall valid? "+ m_ClosestSurfaceDir);
                bool isFrontWallAngleValid = true;//dir.y >= -0.2f;
                //Debug.Log("front wall valid? " + isFrontWallAngleValid+" _ "+Mathf.RoundToInt(frontWallAngle));
                if (isFrontWallAngleValid)
                {// player is infront of a wall
                    int maxMagToAllowWallHang = 30;
                    // initial contact of new front wall
                    //if (!m_Ceiling && !m_Grounded && (charInput.horizontalMove < 0 && m_ClosestSurfaceDir.x <= -0.7f || charInput.horizontalMove > 0 && m_ClosestSurfaceDir.x >= 0.7f) && Mathf.Round(m_Rigidbody2D.velocity.magnitude) < 20)  //&& Mathf.Round(m_Rigidbody2D.velocity.y) < 0)
                    if (((charInput.horizontalMove < 0 || charInput.verticalMove > 0) && _closestSurfaceDir.x <= -0.7f || (charInput.horizontalMove > 0 || charInput.verticalMove > 0) && _closestSurfaceDir.x >= 0.7f) && (Mathf.Round(m_Rigidbody2D.velocity.magnitude) < maxMagToAllowWallHang || Mathf.Abs(m_Rigidbody2D.velocity.x) <= 1) && m_Rigidbody2D.velocity.y < 0) // && Mathf.Round(m_Rigidbody2D.velocity.y) < 0)
                    {
                        //Debug.Log("wall crush yspeed: " + m_Rigidbody2D.velocity.y);
                        //Debug.Log("new front wall: "+maxMagToAllowWallHang+">" + Mathf.Round(m_Rigidbody2D.velocity.magnitude) + " v:" + dir);
                        // start wall hang if player crashed onto front wall
                        obj_WallEdgeParent = objTemp_WallEdgeParent;
                        //stop player rotation while confronting new valid wall
                        JointMotor2D m = m_LegsMotor.motor;
                        m.motorSpeed = 0;
                        m_LegsMotor.motor = m;
                    }
                    else
                    {
                        //Debug.Log("new front wall not valid cuz: "+ maxMagToAllowWallHang + ">"+ Mathf.Round(m_Rigidbody2D.velocity.magnitude)+" v:"+ dir);
                    }
                } else
                {
                    //Debug.Log("not valid");
                }
            }

            // Handle situational changes based on vars
            // set player to wall hang if player is touching a front wall and holding key specified in obj_WallEdgeParent
            //if (obj_WallEdgeParent != null && m_FrontWall && !m_isPlatformClimb && (Time.time - time_wallHang) >= 0f && !m_isAiming && m_ActiveAttack == 0 && !joint.enabled)
            if (obj_WallEdgeParent != null && !m_isPlatformClimb && (Time.time - time_wallHang) >= 0f && !m_isAiming && m_ActiveAttack == 0)
            {
                //Debug.Log("wall hang on: " + obj_WallEdgeParent);
                DoWallHang(_closestSurfacePoint, obj_WallEdgeParent);
                if (m_isBlocking) // abort blocking when staring wallhang
                    DoBlocking(false);
            }
            // when player is falling and not touching any surface, change animation back to jumping
            if (!m_isCrouching && !_isJumping && !_isGrounded && !_isTraversable && !_isFrontWall && !isBackWall && !_isCeiling && !m_isPlatformClimb)
            {
                //if (m_isCrouching)
                    
                    //DoCrouch(false);
                if (m_isStanding)
                    DoStand(false);
                if (_isRopeSwing && !joint.enabled)
                    DisconnectRope();

                _isJumping = true;
                //m_canDoubleJump = false;
                charAnimator.DoJump(true);
            }
            */
            /* not sure it is needed
            // when player is crouching but collider not touching the ground while crouch is activated
            else if (m_isCrouching && !m_Grounded && wasGrounded && Mathf.Abs(m_Rigidbody2D.velocity.x) <= 1)
            {
                // then sustain grounded as if he is still grounded
                m_Grounded = true;
            }
            */
            
            // grace period when player is not on ground anymore
            /*if (!_isGrounded && wasGrounded)
            {
                if (m_gracePeriod < Time.time)
                    m_gracePeriod = Time.time + _cayoteTime;
                
                //m_Grounded = true;
            }
            // when player landed, check for reduction
            else if (!wasGrounded && !wasInbackOfWall && !wasInfrontOfWall && (_isGrounded))
            {
                // call clear jump trajectory on landing 
                aimTrajectory.ClearJumpTrajectory();
                // land crash - normal 'hardcore' landing, added some friction to the motor on impact
                if (Mathf.Abs(torq_speed) > (torq_max * 0.2f))
                {
                    //Debug.Log("torq decrease: " + torq_speed+ " -> "+ Mathf.CeilToInt(torq_speed * 0.9f));
                    torq_speed = Mathf.CeilToInt(torq_speed * 0.9f);
                }// else
                 //Debug.Log("torq too low to decrease: "+ torq_speed); 
                 //}

                if (m_Rigidbody2D.velocity.y <= -15f) // was -15f
                {
                    //Debug.Log("landed crush on time: " + Time.time);
                    //Debug.Log("landed crush with y velo of: " + m_Rigidbody2D.velocity.y);
                    time_landCrushed = Time.time;

                    if (m_Rigidbody2D.velocity.y <= -50f) // was -15f
                    {
                        GetComponent<Player_Gui>()?.ActivateScreenShake(0.2f, 0.5f);
                        //GameManager._instance.ActivateScreenShake(0.2f, 0.5f);
                    }
                }
            }*/

            /*if (_wasTraversable && !_isTraversable)
            {
                // change animation to landing
                _isJumping = Time.time;
            }*/
            
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
                        SetState(Definitions.CharacterState.Attacking);
                        return;
                    } else if (_combatAbility == Definitions.ActionType.Block) {
                        SetState(Definitions.CharacterState.Blocking);
                        return;
                    } else if (_combatAbility == Definitions.ActionType.Grab) {
                        SetState(Definitions.CharacterState.Grabbing);
                        return;
                    }

                } else if (!IsMoveAnimation() && IsInterraptibleAnimation()) {
                    SetState(Definitions.CharacterState.InterruptibleAnimation);
                    return;

                } else {
                    SetActiveCombatAbility(Definitions.ActionType.Empty);
                }

            }

            // todo: add the UninterruptibleAnimation
            if (_currentState == Definitions.CharacterState.InterruptibleAnimation && _combatAbility == Definitions.ActionType.Empty && !IsInterraptibleAnimation()) {
                SetState(Definitions.CharacterState.Grounded);
                SetAsLanded();
                return;
            }

            if (!_hasLanded && _currentState is Definitions.CharacterState.Falling or Definitions.CharacterState.Jumping && (!_wasGrounded && _isGrounded || ( IsFrontWall() || IsBackWall() ) && _hasGroundBeneathByRayCast)) {
                SetState(Definitions.CharacterState.Landed);
                SetAsLanded();
                return;
            }

            if ( (_hasGroundBeneathByRayCast || _hasLanded) && (_isGrounded || IsTouchingAnySurface())) {

                if (!_hasLanded && _hasGroundBeneathByRayCast) {
                    SetAsLanded();
                }

                if (_isCrouching) {
                    SetState(Definitions.CharacterState.Crouching);
                    return;
                } else if (_isStanding) {
                    SetState(Definitions.CharacterState.StandingUp);
                    return;
                } else if (Velocity.magnitude > 2 && (_navigationDirection is Definitions.NavigationType.Right or Definitions.NavigationType.Left)) {
                    SetState(Definitions.CharacterState.Running);
                    return;
                } else if (_wasGrounded && _isGrounded /*&& _currentState == Definitions.CharacterState.Landed*/) {
                    SetState(Definitions.CharacterState.Grounded);
                    return;
                } else if (!_wasGrounded && !_isGrounded && IsFrontWall() && _hasLanded && _velocity.magnitude < 0.1f) {
                    SetState(Definitions.CharacterState.Grounded);
                    return;
                }

            }

            if (IsJumping()) {
                SetState(Definitions.CharacterState.Jumping);
                _hasLanded = false;
                return;

            } else if (_closestSurfaceDir == Vector2.zero && _closestSurfacePoint == Vector2.zero && !IsTouchingAnySurface() && _farSurfaceDir == Vector2.zero) {
                SetState(IsUpwardMovement() ? Definitions.CharacterState.Jumping : Definitions.CharacterState.Falling);
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
            _currentState is Definitions.CharacterState.Crouching or Definitions.CharacterState.StandingUp or
                Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling or
                Definitions.CharacterState.AerialJumping or Definitions.CharacterState.Grabbing or
                Definitions.CharacterState.Grounded or Definitions.CharacterState.Landed or
                Definitions.CharacterState.Running or Definitions.CharacterState.AirGliding or 
                Definitions.CharacterState.WallClimbing or Definitions.CharacterState.RopeClinging or
                Definitions.CharacterState.TraversalRunning or Definitions.CharacterState.WallClinging or 
                Definitions.CharacterState.Alive;
        
        public bool IsJumping() => Time.time < _jumpingEndTime;

        public bool IsGrounded() => _isGrounded;

        public bool IsCrouching() => _isCrouching;

        public bool IsCrouchingAndNotFallingNearWall() => _isCrouching && (_hasGroundBeneathByRayCast && (_isGrounded || _isLeftWall || _isRightWall) );

        public bool IsStandingUp() => _isStanding;

        // todo: move all the consideredAs.... to gameplayLogic

        public int GetFacingRightAsInt() => _facingRight ? 1 : -1;

        public bool IsFacingRight() => _facingRight;

        public void SetFacingRight(bool isFacingRight) => _facingRight = isFacingRight;

        public bool IsFrontWall() => _facingRight && _isRightWall || !_facingRight && _isLeftWall;
        public bool IsBackWall() => _facingRight && _isLeftWall || !_facingRight && _isRightWall;
        

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
                Quaternion surfaceQuaternion = Quaternion.LookRotation(Vector3.forward, _farSurfaceDir);
                return surfaceQuaternion;
            }
            
            Quaternion velocityQuaternion = Quaternion.LookRotation(Vector3.forward, _velocity);
            return velocityQuaternion;
            
        }

        public bool HasAnySurfaceAround() => _farSurfaceDir != Vector2.zero && _hasLanded;

        public Vector2 Velocity => _velocity;

        public void SetNavigationDir(Definitions.NavigationType navigationDirection) {
            _navigationDirection = navigationDirection;
        }

        public Definitions.NavigationType NavigationDir => _navigationDirection;

        public void SetActiveCombatAbility(Definitions.ActionType combatAbility) => _combatAbility = combatAbility;

        public bool CanPerformJump(bool isStateConsideredAsGrounded) {

            var isVerticalVelocityExceeding = Velocity.y < 10;

            var isGroundedOrCayoteTime = (IsTouchingAnySurface() && isStateConsideredAsGrounded || IsCoyoteTime() || _hasLanded);
            return isVerticalVelocityExceeding && isGroundedOrCayoteTime && !IsJumping() && CurrentState != Definitions.CharacterState.Crouching;
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