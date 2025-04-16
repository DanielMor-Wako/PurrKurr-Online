using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Ropes;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using Code.Wakoz.Utils.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller
{
    public sealed class CharacterActionExecuter : IDisposable
    {
        private readonly InputLogic _inputLogic;
        private readonly GameplayLogic _gameplayLogic;
        private readonly InputProcessor _actionGenerator;

        private GameplayController _gameEvents;

        private LayerMask _whatIsPlatform;
        private LayerMask _whatIsClingable;
        // consider these properties as character specific?
        private float _minVelocityForFlip = 5f;
        private float _xDistanceFromClingPoint = 0.8f;

        public CharacterActionExecuter(GameplayController gameEvents, InputLogic inputLogic, GameplayLogic gameplayLogic, InputProcessor actionGenerator) {

            _gameEvents = gameEvents;

            _inputLogic = inputLogic ?? throw new ArgumentNullException(nameof(inputLogic));
            _gameplayLogic = gameplayLogic ?? throw new ArgumentNullException(nameof(gameplayLogic));
            _actionGenerator = actionGenerator ?? throw new ArgumentNullException(nameof(actionGenerator));

            _actionGenerator.OnNavigationAction += ProcessMovement;
            _actionGenerator.OnAbilityAction += ProcessAction;

            _whatIsPlatform = _gameplayLogic.GetPlatformSurfaces();
            _whatIsClingable = _gameplayLogic.GetClingableSurfaces();
        }

        public void Dispose() {

            if (_actionGenerator == null) return;

            _actionGenerator.OnNavigationAction -= ProcessMovement;
            _actionGenerator.OnAbilityAction -= ProcessAction;
        }
        // todo: move to CombatHandler
        private AttackAbility DefineAbilityVariant(Character2DController character, ActionInput action) {

            if (action == null)
                return AttackAbility.None;

            var characterState = character.State.CurrentState;
            var isAirBorneState = characterState is ObjectState.Jumping || characterState is ObjectState.Falling;

            switch (action.ActionType) {

                case ActionType.Attack:

                    return characterState switch {
                        ObjectState.Running => AttackAbility.RollAttack,
                        _ when isAirBorneState => AttackAbility.AerialAttack,
                        ObjectState.Crouching => AttackAbility.HeavyAttack,
                        ObjectState.StandingUp => AttackAbility.MediumAttack,
                        _ => AttackAbility.LightAttackAlsoDefaultAttack
                    };

                case ActionType.Grab:

                    return characterState switch {
                        _ when isAirBorneState => AttackAbility.AerialGrab,
                        ObjectState.Crouching => AttackAbility.HeavyGrab,
                        ObjectState.StandingUp => AttackAbility.MediumGrab,
                        _ => AttackAbility.LightGrabAlsoDefaultGrab
                    };

                case ActionType.Block:

                    return AttackAbility.LightBlock;

                default:
                    break;
            }

            return AttackAbility.None;
        }

        private void ProcessMovement(Character2DController character, ActionInput action, bool started, bool ended) {

            if (action == null || character == null)
                return;

            if (started) {
                OnNavigationStarted(character, action);

            } else if (ended) {
                OnNavigationEnded(character, action);

            } else {
                OnNavigationOngoing(character, action);

            }
        }

        public void ProcessAction(Character2DController character, ActionInput action, bool started, bool ended) {

            if (action == null || character == null)
                return;

            if (started) {
                OnActionStarted(character, action);
            
            } else if (ended) {
                OnActionEnded(character, action);

            } else {
                OnActionOngoing(character, action);

            }
        }

        public async Task ApplyAimingAction(Character2DController character, ActionInput actionInput) {

            var actionType = actionInput.ActionType;

            var state = character.State;

            var isRope = actionType is ActionType.Rope;
            var isProjectile = actionType is ActionType.Projectile;
            var isJumpAim = actionType is ActionType.Jump;

            var validAimingAction = isRope || isProjectile || isJumpAim;

            if (!validAimingAction || validAimingAction && state.IsAiming()) {
                return;
            }

            state.SetAiming(10f);

            Vector2 newPos = Vector2.zero;
            Vector2 endPos = Vector2.zero;
            float distancePercentReached = 1;
            Quaternion rotation = Quaternion.identity;
            Vector3[] linePoints = null;

            bool hasAimDir, hasHitData = false;

            while (state.IsAiming() && character != null) {

                hasAimDir = actionInput.NormalizedDirection != Vector2.zero;

                if (isRope) {
                    hasHitData = character.TryGetRopeDirection(actionInput.NormalizedDirection, ref newPos, ref rotation, out var cursorPos, ref distancePercentReached);
                    newPos = cursorPos;
                    endPos = cursorPos;

                } else if (isProjectile) {
                    hasHitData = character.TryGetProjectileDirection(actionInput.NormalizedDirection, ref endPos, ref rotation, ref distancePercentReached);
                    newPos = character.LegsPosition;

                } else if (isJumpAim) {
                    linePoints = new Vector3[] { Vector3.zero, Vector2.one };
                    var forceDir = actionInput.NormalizedDirection * character.Stats.JumpForce;
                    hasHitData = character.TryGetJumpTrajectory(forceDir, ref endPos, ref linePoints);
                    newPos = character.LegsPosition;
                }

                if (hasAimDir) {
                    _gameEvents.InvokeAimingAction(actionType, newPos, rotation, linePoints, hasHitData);

                } else {
                    _gameEvents.InvokeAimingActionEnd(actionType);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(5));
            }

            _gameEvents.InvokeAimingActionEnd(actionType);
        }


        private void ApplyClingingAction(Character2DController character, Collider2D coll) {

            /*if (coll is not EdgeCollider2D) {
                return;
            }*/

            var navType = character.State.NavigationDir;
            var facingRightAsInt = character.State.GetFacingRightAsInt();
            var horizontalNavigationDirAsInt = _inputLogic.GetNavigationDirAsFacingRightInt(navType);
            //var isPlayerMovingTowardsWall = horizontalNavigationDirAsInt == 1 && _hero.State.IsFrontWall();

            if (!character.State.IsClinging() && character.State.IsFrontWall()
                && facingRightAsInt != horizontalNavigationDirAsInt) {
                return;
            }

            // 0.55f is the default hingeJoint2D y offset of the Anchor, the climbSpeed determines the offset when up or down
            var climbSpeed = 0.1f;
            var yMovementByNavigation = navType is NavigationType.Up ? climbSpeed : navType is NavigationType.Down ? -climbSpeed : 0;

            var reachedWallBottom = yMovementByNavigation < 0 && character.State.IsCeiling();
            if (reachedWallBottom) {
                character.SetAsClinging(null, character.transform.position);
                return;
            }

            var anchorDistanceFromLegsPosition = (character.transform.position.y - character.LegsPosition.y);
            if (anchorDistanceFromLegsPosition > 0 && navType is NavigationType.Up) {
                Debug.Log($"anchor too far from legs position, climb up ignored {anchorDistanceFromLegsPosition} > 0");
                return;
            }

            var edges = coll;// as EdgeCollider2D;

            var legsPos = character.transform.position;
            legsPos.y += yMovementByNavigation;
            var edgesClosestPoint = edges.ClosestPoint(legsPos);
            var wallPoint = edgesClosestPoint;
            wallPoint.y = legsPos.y;
            var isWallPositionedRightToThePlayer = wallPoint.x > legsPos.x;

            var isNavTowardsWall = _inputLogic.IsNavigationDirValidAsRight(character.State.NavigationDir) && isWallPositionedRightToThePlayer;
            if (!isNavTowardsWall) {
                isNavTowardsWall = _inputLogic.IsNavigationDirValidAsLeft(character.State.NavigationDir) && !isWallPositionedRightToThePlayer;
                if (!isNavTowardsWall) {
                    isNavTowardsWall = character.State.IsFacingRight() == isWallPositionedRightToThePlayer;
                }
            }

            var notFacingTowardsWall = !isNavTowardsWall;
            if (notFacingTowardsWall) {
                return;
            }

            var xHorizontalOffsetFromWall = new Vector2(isWallPositionedRightToThePlayer ? -1 : 1, 0); //Quaternion.Inverse(character.State.ReturnForwardDirByTerrainQuaternion())
            //Vector2 offsetAngleFromWall = xOffsetFromWall * character.LegsRadius * 0.75f; // legsRadius * 0.9f, position the character right next the edge of the collider
            wallPoint += xHorizontalOffsetFromWall * character.LegsRadius * _xDistanceFromClingPoint;
            character.SetAsClinging(edges, wallPoint);

            Debug.DrawLine(edgesClosestPoint, wallPoint, Color.magenta, 1f);
        }


        // for ref
        private void OnNavigationStarted(Character2DController character, ActionInput actionInput) {

            if (ValidateNavigation(character, actionInput, true, false, 
                    out var moveSpeed, out Vector2 forceDirNavigation, out var navigationDir)) {

                character.DoMove(moveSpeed);
                character.SetForceDir(forceDirNavigation);
                character.SetNavigationDir(navigationDir);
                if (character.IsGrabbed()) {
                    character.FlipCharacterTowardsPoint(_inputLogic.IsNavigationDirValidAsRight(navigationDir));
                }
            }

        }

        private void OnNavigationOngoing(Character2DController character, ActionInput actionInput) {

            if (ValidateNavigation(character, actionInput, false, false,
                    out var moveSpeed, out var forceDirNavigation, out var navigationDir)) {

                character.DoMove(moveSpeed);
                character.SetForceDir(forceDirNavigation);
                character.SetNavigationDir(navigationDir);
                if (!character.State.IsMoveAnimation() && navigationDir != NavigationType.None) {

                    bool? facingRight = _inputLogic.IsNavigationDirValidAsRight(navigationDir) ? true :
                        _inputLogic.IsNavigationDirValidAsLeft(navigationDir) ? false : null;

                    bool isNotJumpingState = character.State.CurrentState != ObjectState.Jumping;
                    bool isRunState = character.State.CurrentState == ObjectState.Running;

                    if (facingRight != null && isNotJumpingState && (!isRunState && Mathf.Abs(character.Velocity.x) > _minVelocityForFlip)) {
                        // check if player is not facing a wall while running so momentum sustains
                        character.FlipCharacterTowardsPoint(facingRight == true);
                    }

                    // interact with rope link
                    if (character.IsGrabbed() && _gameEvents.CharactersRope.Count > 0 && !character.State.IsAiming()) {
                        var rope = _gameEvents.CharactersRope.FirstOrDefault();
                        var grabbedInteractable = character.GetGrabbedTarget();
                        var ropeLink = grabbedInteractable as RopeLinkController;
                        if (ropeLink != null) {
                            if (navigationDir is NavigationType.Up or NavigationType.Down) {

                                rope.HandleMoveToNextLink(ropeLink, character, navigationDir is NavigationType.Up);

                            } else if (navigationDir is not (NavigationType.None)) {
                                var isNavRightByFacingRight = facingRight != null ? facingRight == true ? true : false : false;

                                rope.HandleSwingMommentum(ropeLink, isNavRightByFacingRight);
                            }
                        }
                    } else if (character.State.CurrentState is not (ObjectState.RopeClinging or ObjectState.RopeClimbing)) {

                        var collSurfaceLayer = character.GetSurfaceCollLayer();
                        if (collSurfaceLayer > -1 && HelperFunctions.IsObjectInLayerMask(collSurfaceLayer, ref _whatIsClingable)) {

                            var closestWall = character._solidColliders.FirstOrDefault();
                            if (closestWall != null) {
                                ApplyClingingAction(character, closestWall);
                            }

                        } else if (character.State.IsClinging() && !character.State.IsTouchingAnySurface() && _inputLogic.IsNavigationDirValidAsDown(navigationDir)) {

                            var xOffsetFromWall = new Vector2(0, -0.2f);
                            var clingPointCloserToTopEdge = (Vector2)character.transform.position + xOffsetFromWall;
                            character.SetAsClinging(null, clingPointCloserToTopEdge);
                        }
                    }

                }
            }
        }

        private void OnNavigationEnded(Character2DController character, ActionInput actionInput) {

            if (ValidateNavigation(character, actionInput, false, true, 
                    out var moveSpeed, out Vector2 forceDirNavigation, out var navigationDir)) {

                character.DoMove(moveSpeed);
                character.SetForceDir(forceDirNavigation);
                character.SetNavigationDir(NavigationType.None);
            }

        }

        private void OnActionStarted(Character2DController character, ActionInput actionInput) {

            if (ValidateAction(character, actionInput, true, false, 
                    out var isActionPerformed, out var interactedColliders)) {

                if (isActionPerformed) {

                    var isCombatAction = actionInput.ActionType is ActionType.Attack or ActionType.Grab or ActionType.Block;
                    if (isCombatAction && !character.State.IsGrabbed()) {
                        _gameEvents.CombatLogic(character, actionInput.ActionType);

                    } else if (interactedColliders != null && character.State.IsGrabbed() &&
                        actionInput.ActionType is ActionType.Grab or ActionType.Attack) {
                        if (actionInput.ActionType is ActionType.Grab) {
                            _gameEvents.DisconnectFromRope(character);
                        } else {// if (actionInput.ActionType is ActionType.Attack) {
                            _gameEvents.CutRopeLink(character);
                        }

                    } else if (actionInput.ActionType is ActionType.Special) {
                        _gameEvents.ApplySpecialAction(character, true);

                    } else if (actionInput.ActionType is ActionType.Rope or ActionType.Projectile) {
                        _ = ApplyAimingAction(character, actionInput);

                    } else if (actionInput.ActionType is ActionType.Jump && character.State.CurrentState == ObjectState.Crouching) {

                        if (HelperFunctions.IsObjectInLayerMask(character.GetSurfaceCollLayer(), ref _whatIsPlatform)) {
                            var moveToPosition = Vector2.zero;
                            character.TryGetDodgeDirection(-Vector2.up * 2, ref moveToPosition);
                            character.SetTargetPosition(moveToPosition);
                        } else {
                            _ = ApplyAimingAction(character, actionInput);
                        }

                    } else if (actionInput.ActionType == ActionType.Jump) {
                        _gameEvents.DisconnectFromAnyGrabbing(character);
                        var jumpForceDir = new Vector2(0, character.Stats.JumpForce);
                        _gameEvents.AlterJumpDirByStateNavigationDirection(ref jumpForceDir, character.State);
                        character.SetJumping(Time.time + .1f);
                        character.SetForceDir(jumpForceDir);
                        character.DoMove(0); // might conflict with the TryPerformInputNavigation when the moveSpeed is already set by Navigation
                        _gameEvents.ApplyEffectOnCharacter(character, Effect2DType.DustCloud, character.State.ReturnForwardDirByTerrainQuaternion());

                    }

                    character.State.SetActiveCombatAbility(actionInput.ActionType);
                }

            }

        }

        private void OnActionOngoing(Character2DController character, ActionInput actionInput) {

        }

        private void OnActionEnded(Character2DController character, ActionInput actionInput) {

            if (ValidateAction(character, actionInput, false, true, 
                    out var isActionPerformed, out var interactedCollider)) {

                if (character.State.CurrentState == ObjectState.Jumping && character.Velocity.y > 0) {

                    var jumpforceDirKiller = new Vector2(character.State.Velocity.x, 0);
                    character.SetForceDir(jumpforceDirKiller);
                    //character.SetForceDir(forceDir, true);
                    return;
                }

                if (isActionPerformed) {

                    var isBlockingEnded = actionInput.ActionType is ActionType.Block ;// && character.State.CurrentState is ObjectState.Blocking;
                    var isProjectilingEnded = actionInput.ActionType is ActionType.Projectile && character.State.CurrentState is ObjectState.AimingProjectile;
                    var isRopingEnded = actionInput.ActionType is ActionType.Rope && character.State.CurrentState is ObjectState.AimingRope;
                    var isJumpAimEnded = actionInput.ActionType is ActionType.Jump && character.State.CurrentState is ObjectState.AimingJump;
                    var isSpecial = actionInput.ActionType is ActionType.Special && character.State.CurrentState is ObjectState.InterruptibleAnimation;

                    if (isBlockingEnded || isProjectilingEnded || isRopingEnded || isJumpAimEnded || isSpecial) {

                        character.State.SetActiveCombatAbility(ActionType.Empty);

                        if (isSpecial) {
                            _gameEvents.ApplySpecialAction(character, false);

                        } else if (actionInput.NormalizedDirection != Vector2.zero) {

                            if (isBlockingEnded) {
                                var dodgeEndPosition = Vector2.zero;
                                character.TryGetDodgeDirection(actionInput.NormalizedDirection * 5, ref dodgeEndPosition);
                                character.SetTargetPosition(dodgeEndPosition);
                                _gameEvents.ApplyEffectOnCharacter(character, Effect2DType.DodgeActive);
                                _gameEvents.ApplyEffectOnCharacter(character, Effect2DType.DustCloud, character.State.ReturnForwardDirByTerrainQuaternion());

                            } else {

                                Vector2 newPos = Vector2.zero;
                                Quaternion rotation = Quaternion.identity;
                                float distancePercentReached = 1f;

                                if (isProjectilingEnded) {
                                    _gameEvents.ShootProjectile(character, actionInput, ref newPos, ref rotation, ref distancePercentReached);
                                    
                                } else if (isRopingEnded) {
                                    _gameEvents.ShootRope(character, actionInput, newPos, rotation, distancePercentReached);

                                } else if (isJumpAimEnded) {
                                    // apply jump aim in trajectory dir
                                    character.SetJumping(Time.time + .5f);
                                    var aimJumpDir = actionInput.NormalizedDirection * character.Stats.JumpForce;
                                    character.SetForceDir(aimJumpDir, true);
                                    _gameEvents.ApplyEffectOnCharacter(character, Effect2DType.DustCloud, character.State.ReturnForwardDirByTerrainQuaternion());
                                }
                            }
                        }

                        if (isProjectilingEnded || isRopingEnded || isJumpAimEnded) {
                            character.State.StopAiming();
                        }
                    }
                }

            }
        }


        public bool ValidateNavigation(Character2DController character, ActionInput actionInput, bool started, bool ended, 
                out float moveSpeed, out Vector2 forceDirToSetOnFixedUpdate, out Definitions.NavigationType navigationDir) {

            moveSpeed = 0;
            forceDirToSetOnFixedUpdate = Vector2.zero;
            navigationDir = Definitions.NavigationType.None;

            if (character == null || actionInput.ActionGroupType != Definitions.ActionTypeGroup.Navigation) {
                return false;
            }

            var state = character.State;
            var stats = character.Stats;

            navigationDir = _inputLogic.GetInputDirection(actionInput.NormalizedDirection);

            if (ended) {
                state.UpdateCrouchOrStandingByNavDir(Definitions.NavigationType.None);
                return true;
            }

            var isGrabbing = state.IsGrabbing();

            state.UpdateCrouchOrStandingByNavDir(navigationDir);
            var isInvalidMovementState = state.CurrentState is Definitions.ObjectState.Blocking or Definitions.ObjectState.AimingRope or Definitions.ObjectState.AimingProjectile or Definitions.ObjectState.AimingJump;
            var isCrouchingState = !isInvalidMovementState && state.CurrentState != Definitions.ObjectState.Running && state.IsCrouchingAndNotFallingNearWall();
            var isStandingState = !isInvalidMovementState && state.CurrentState != Definitions.ObjectState.Running && (state.IsStandingUp() || isGrabbing);

            // full charge towards walls when any nav pad is held
            if (!isInvalidMovementState && !isGrabbing && !isCrouchingState && !isStandingState && state.HasAnySurfaceAround() && navigationDir != Definitions.NavigationType.None && state.CanPerformContinousRunning()) {

                if ((state.IsCeiling() || state.IsFrontWall()) && _inputLogic.IsNavigationDirValidAsUp(navigationDir)
                    || _gameplayLogic.IsStateConsideredAsRunning(state.CurrentState, state.Velocity.magnitude) && state.IsBackWall()
                    && (state.IsFacingRight() && _inputLogic.IsNavigationDirValidAsLeft(navigationDir) || !state.IsFacingRight() && _inputLogic.IsNavigationDirValidAsRight(navigationDir))
                    ) {

                    moveSpeed = -stats.SprintSpeed * state.GetFacingRightAsInt();

                    return true;
                } else if (navigationDir is not (Definitions.NavigationType.Up or Definitions.NavigationType.Down or Definitions.NavigationType.DownRight or Definitions.NavigationType.DownLeft)) {

                    var maxSpeed = (actionInput.SwipeDistanceTraveledInPercentage == 1) ? stats.SprintSpeed : stats.RunSpeed;
                    moveSpeed = actionInput.SwipeDistanceTraveledInPercentage * -maxSpeed * (_inputLogic.IsNavigationDirValidAsRight(navigationDir) ? 1 : -1);

                    return true;
                }
            }

            // Air-borne movement
            var rigidbodyVelocity = state.Velocity;
            var isNavRightDir = _inputLogic.IsNavigationDirValidAsRight(navigationDir);
            var isNavLeftDir = _inputLogic.IsNavigationDirValidAsLeft(navigationDir);

            var isAirBorne = state.CurrentState is Definitions.ObjectState.Falling or Definitions.ObjectState.Jumping && !state.IsJumping();

            if (isAirBorne) {
                var yVelocity = rigidbodyVelocity.y;

                if (isNavRightDir || isNavLeftDir) {

                    // Check if the absolute value of the current x velocity is beneath the threshold
                    if (Mathf.Abs(rigidbodyVelocity.x) < stats.AirborneMaxSpeed
                        || Mathf.Sign(rigidbodyVelocity.x) > 0 && isNavLeftDir
                        || Mathf.Sign(rigidbodyVelocity.x) < 0 && isNavRightDir) {

                        var newXVelocity = rigidbodyVelocity.x + stats.AirborneSpeed * (isNavRightDir ? 1 : -1);
                        var clampedXVelocity = Mathf.Clamp(newXVelocity, -stats.AirborneMaxSpeed, stats.AirborneMaxSpeed);
                        forceDirToSetOnFixedUpdate = new Vector2(clampedXVelocity, yVelocity);
                    }
                    moveSpeed = 0;

                    return true;

                }

            } else if (!isInvalidMovementState && !state.IsCeiling() &&
                (state.IsTouchingAnySurface() && state.CanMoveOnSurface() || state.IsBackWall() || state.IsFrontWall() && state.Velocity.magnitude < 0.2f)) {

                switch (navigationDir) {

                    case var _ when isCrouchingState && navigationDir == Definitions.NavigationType.DownLeft:
                        moveSpeed = stats.WalkSpeed;
                        break;

                    case var _ when isCrouchingState && navigationDir == Definitions.NavigationType.DownRight:
                        moveSpeed = -stats.WalkSpeed;
                        break;

                    case Definitions.NavigationType.Right:
                        moveSpeed = -actionInput.SwipeDistanceTraveledInPercentage * stats.SprintSpeed;
                        break;

                    case Definitions.NavigationType.Left:
                        moveSpeed = actionInput.SwipeDistanceTraveledInPercentage * stats.SprintSpeed;
                        break;

                }

            }

            return true;
        }

        public bool ValidateAction(Character2DController character, ActionInput actionInput, bool started, bool ended, 
            out bool isActionPerformed, out Collider2D[] closestColliders) {

            isActionPerformed = false;
            closestColliders = null;

            if (character == null || actionInput.ActionGroupType != Definitions.ActionTypeGroup.Action) {
                return false;
            }

            var state = character.State;

            if (state.IsMoveAnimation() && started && actionInput.ActionType != ActionType.Block || character.GetHpPercent() <= 0) {
                return false;
            }

            var stats = character.Stats;

            var heroAsInteractableBody = (IInteractableBody)character;
            if (started && actionInput.ActionType is Definitions.ActionType.Attack or Definitions.ActionType.Grab
                && heroAsInteractableBody.IsGrabbing()) {

                closestColliders = new Collider2D[] { character.GetGrabbedTarget().GetCollider() };
                isActionPerformed = true;
                return true;
            }

            switch (actionInput.ActionType) {

                case Definitions.ActionType.Jump:

                    if (!ended && state.CanPerformJump(_gameplayLogic.IsStateConsideredAsGrounded(state.CurrentState)) ||
                        started && character.State.CurrentState is Definitions.ObjectState.TraversalRunning or Definitions.ObjectState.RopeClinging or Definitions.ObjectState.RopeClimbing or Definitions.ObjectState.WallClinging or Definitions.ObjectState.WallClimbing) {

                        isActionPerformed = true;

                    } else if ((started && character.State.CurrentState == Definitions.ObjectState.Crouching && state.IsCrouchingAndNotFallingNearWall() ||
                        ended && character.State.CurrentState == Definitions.ObjectState.AimingJump)) {

                        isActionPerformed = true;
                    }
                    return true;

                case Definitions.ActionType.Attack:

                    /*closestColliders = character.NearbyInteractables();
                    if (closestColliders == null || closestColliders.Length == 0) {
                        return false;
                    }*/

                    if (started) {

                        /*var closestColl = closestColliders.Where(o => o.GetComponent<IInteractable>()?.GetInteractableBody()?.GetHpPercent() > 0).FirstOrDefault();
                        if (closestColl == null)
                            return false;*/

                        isActionPerformed = true;
                        // todo: delete these calculation from here after they are move to lateon when action occur, both forceDir and newPosition
                        
                        //forceDirToSetOnFixedUpdate = dir.normalized * stats.PushbackForce;
                        return true;
                    }

                    return false;

                case Definitions.ActionType.Block:

                    if (started || ended) {
                        isActionPerformed = true;
                        return true;
                    }

                    return false;

                case Definitions.ActionType.Grab:

                    if (!started) {
                        return false;
                    }

                    /*var nearbyGrabables = character.NearbyInteractables();
                    if (nearbyGrabables == null || nearbyGrabables.Length == 0) {
                        return false;
                    }

                    closestColliders = new Collider2D[] { nearbyGrabables.FirstOrDefault() };

                    if (closestColliders == null) {
                        return false;
                    }*/

                    if (started) {

                        isActionPerformed = true;
                        // todo: delete these calculation from here after they are move to lateon when action occur, both forceDir and newPosition
                                                                                                             //dir = closestFoePosition - newPositionToSetOnFixedUpdate;
                        //forceDirToSetOnFixedUpdate = dir.normalized * stats.PushbackForce;

                        return true;
                    }

                    break;

                case Definitions.ActionType.Projectile:

                    isActionPerformed = started || ended;

                    return isActionPerformed;

                case Definitions.ActionType.Rope:

                    isActionPerformed = started || ended;

                    return isActionPerformed;

                case Definitions.ActionType.Special:

                    isActionPerformed = started || ended;

                    return isActionPerformed;

            }
            return false;
        }

    }
}