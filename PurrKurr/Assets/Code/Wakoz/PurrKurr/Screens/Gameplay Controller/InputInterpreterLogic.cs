using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using UnityEngine;
using System.Linq;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;

// todo: move to a better namespace like player input
namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller {
    public sealed class InputInterpreterLogic {

        private InputLogic _inputLogic;
        private GameplayLogic _gameplayLogic;

        public InputInterpreterLogic(InputLogic inputLogic, GameplayLogic gameplayLogic) {

            _inputLogic = inputLogic;
            _gameplayLogic = gameplayLogic;
        }

        public bool TryPerformInputNavigation(ActionInput actionInput, bool started, bool ended, Character2DController character, 
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
                state.SetCrouchOrStandingByUpDownInput(Definitions.NavigationType.None);
                return true;
            }
            
            var isGrabbing = state.IsGrabbing();
            // nth: rope specific state for movement?
            /*if (isGrabbing && state.GetGrabbedTarget() != null) {
                var grabbedTarget = character.GetGrabbedTarget();
                if (grabbedTarget is RopeLinkController) {
                    ((RopeLinkController)grabbedTarget).TryPerformInteraction(actionInput, navigationDir);
                }
            }*/

            state.SetCrouchOrStandingByUpDownInput(navigationDir);
            var isInvalidState = state.CurrentState is Definitions.ObjectState.Blocking or Definitions.ObjectState.AimingRope or Definitions.ObjectState.AimingProjectile or Definitions.ObjectState.AimingJump;
            var isCrouchingState = !isInvalidState && state.CurrentState != Definitions.ObjectState.Running && state.IsCrouchingAndNotFallingNearWall();
            var isStandingState = !isInvalidState && state.CurrentState != Definitions.ObjectState.Running && (state.IsStandingUp() || isGrabbing);

            // full charge towards walls when any nav pad is held
            //if (!isGrabbing && (state.IsCeiling() || state.IsFrontWall()) && navigationDir != Definitions.NavigationType.None) {
            if (!isInvalidState && !isGrabbing && !isCrouchingState && !isStandingState && state.HasAnySurfaceAround() && navigationDir != Definitions.NavigationType.None && state.CanPerformContinousRunning()) {

                if ( (state.IsCeiling() || state.IsFrontWall()) && _inputLogic.IsNavigationDirValidAsUp(navigationDir)
                    || _gameplayLogic.IsStateConsideredAsRunning(state.CurrentState, state.Velocity.magnitude) && state.IsBackWall() 
                    && ( state.IsFacingRight() && _inputLogic.IsNavigationDirValidAsLeft(navigationDir) || !state.IsFacingRight() && _inputLogic.IsNavigationDirValidAsRight(navigationDir))
                    ) {
                    moveSpeed = -stats.SprintSpeed * state.GetFacingRightAsInt();
                    
                    return true;
                } else if (navigationDir is not (Definitions.NavigationType.Up or Definitions.NavigationType.Down or Definitions.NavigationType.DownRight or Definitions.NavigationType.DownLeft)) {
                    moveSpeed = actionInput.SwipeDistanceTraveledInPercentage * -stats.SprintSpeed * (_inputLogic.IsNavigationDirValidAsRight(navigationDir) ? 1 : -1);
                    
                    return true;
                }
            }

            // Air-borne movement
            var rigidbodyVelocity = state.Velocity;
            var isNavRightDir = _inputLogic.IsNavigationDirValidAsRight(navigationDir);
            var isNavLeftDir = _inputLogic.IsNavigationDirValidAsLeft(navigationDir);

            //if (rigidbodyVelocity.y < -1f && !state.IsJumping() && _gameplayLogic.IsStateConsideredAsAerial(state.CurrentState)) {
            var isFalling = state.CurrentState == Definitions.ObjectState.Falling;
            //var isTraversableRunning = state.CurrentState == Definitions.ObjectState.TraversalRunning;

            if (rigidbodyVelocity.y < -1f && isFalling) {
                var yVelocity = rigidbodyVelocity.y;

                if (isNavRightDir || isNavLeftDir) {

                    // Check if the absolute value of the current x velocity is beneath the threshold
                    if (Mathf.Abs(rigidbodyVelocity.x) < stats.AirborneMaxSpeed
                        || Mathf.Sign(rigidbodyVelocity.x) > 0 && isNavLeftDir
                        || Mathf.Sign(rigidbodyVelocity.x) < 0 && isNavRightDir) {

                        var newXVelocity = rigidbodyVelocity.x + stats.AirborneSpeed * (isNavRightDir ? 1 : -1);
                        var clampedXVelocity = Mathf.Clamp(newXVelocity, -stats.AirborneMaxSpeed, stats.AirborneMaxSpeed);
                        forceDirToSetOnFixedUpdate = new Vector2(clampedXVelocity, yVelocity);

                    }/* else {
                        // Retain the original x velocity
                        forceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x, yVelocity);
                    }*/
                    moveSpeed = 0;

                    return true;

                }

            } else if (!isInvalidState && !state.IsCeiling() &&
                (state.IsTouchingAnySurface() && state.CanMoveOnSurface() || state.IsBackWall() || state.IsFrontWall() && state.Velocity.magnitude < 0.2f)) {

                // case where user has landed on wall, check for ground beneath and mark as landed
                // todo: move the special case when -> if (state.IsFrontWall() && state.Velocity.magnitude < 0.2f && !state.CanMoveOnSurface()) { }

                switch (navigationDir) {
                    case var _ when isStandingState && _inputLogic.IsNavigationDirValidAsLeft(navigationDir):
                        moveSpeed = stats.WalkSpeed;
                        break;

                    case var _ when isStandingState && _inputLogic.IsNavigationDirValidAsRight(navigationDir):
                        moveSpeed = -stats.WalkSpeed;
                        break;

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

        public bool TryPerformInputAction(ActionInput actionInput, bool started, bool ended, Character2DController character, 
            out bool isActionPerformed, out Vector2 forceDirToSetOnFixedUpdate, out Vector2 newPositionToSetOnFixedUpdate, out Collider2D[] closestColliders ) {

            isActionPerformed = false;
            forceDirToSetOnFixedUpdate = Vector2.zero;
            newPositionToSetOnFixedUpdate = Vector2.zero;
            closestColliders = null;
            
            if (character == null || actionInput.ActionGroupType != Definitions.ActionTypeGroup.Action) {
                return false;
            }

            var state = character.State;
            var stats = character.Stats;

            var heroAsInteractableBody = (IInteractableBody)character;
            if (started && actionInput.ActionType is Definitions.ActionType.Attack or Definitions.ActionType.Grab
                && heroAsInteractableBody.IsGrabbing()) {

                closestColliders = new Collider2D[] { character.GetGrabbedTarget().GetCollider() };
                forceDirToSetOnFixedUpdate = new Vector2(state.GetFacingRightAsInt(), 1) * stats.PushbackForce;
                isActionPerformed = true;
                return true;
            }
            
            switch (actionInput.ActionType) {
                
                case Definitions.ActionType.Jump:

                    if (!ended && state.CanPerformJump(_gameplayLogic.IsStateConsideredAsGrounded(state.CurrentState)) ||
                        started && character.State.CurrentState is Definitions.ObjectState.TraversalRunning or Definitions.ObjectState.RopeClinging or Definitions.ObjectState.RopeClimbing or Definitions.ObjectState.WallClinging or Definitions.ObjectState.WallClimbing) {
                        
                        isActionPerformed = true;
                        forceDirToSetOnFixedUpdate = new Vector2(0, stats.JumpForce);

                    } else if (ended && state.Velocity.y > 0 && state.CurrentState == Definitions.ObjectState.Jumping) {

                        if (!(forceDirToSetOnFixedUpdate != Vector2.zero)) {
                            forceDirToSetOnFixedUpdate = new Vector2(state.Velocity.x, 0);
                        } else {//if (forceDirToSetOnFixedUpdate != Vector2.zero) {
                            forceDirToSetOnFixedUpdate = Vector2.zero;
                        }
                    
                    } else if ((started && character.State.CurrentState == Definitions.ObjectState.Crouching && state.IsCrouchingAndNotFallingNearWall() ||
                        ended && character.State.CurrentState == Definitions.ObjectState.AimingJump)) {

                        isActionPerformed = true;
                    }
                    return true;

                case Definitions.ActionType.Attack:

                    closestColliders = character.NearbyInteractables();
                    if (closestColliders == null || closestColliders.Length == 0) {
                        return false;
                    }
                    
                    if (started) {

                        isActionPerformed = true;
                        var closestColl = closestColliders.FirstOrDefault();
                        var legsPosition = character.LegsPosition;
                        var closestFoePosition = closestColl.ClosestPoint(legsPosition);
                        var dir = ((Vector3)closestFoePosition - legsPosition);
                        var directionTowardsFoe = (closestColl.transform.position.x > legsPosition.x) ? 1 : -1;
                        if (dir == Vector3.zero) {
                            dir = new Vector3(directionTowardsFoe, 0, 0);
                        }
                        newPositionToSetOnFixedUpdate = closestFoePosition + (Vector2)dir.normalized * -(character.LegsRadius);
                        Debug.DrawLine(legsPosition, newPositionToSetOnFixedUpdate, Color.green, 3); // todo: move drawline to gameplaycontroller
                        dir = closestFoePosition - newPositionToSetOnFixedUpdate;
                        forceDirToSetOnFixedUpdate = dir.normalized * stats.PushbackForce;
                        return true;
                    }
                    
                    break;

                case Definitions.ActionType.Block:

                    if (started) {
                        isActionPerformed = true;
                        return true;
                    }

                    if (ended) {
                        isActionPerformed = true;
                        // todo: get dodge direction from inputAction and set as jump with active dodge time
                        character.TryGetDodgeDirection(actionInput.NormalizedDirection * 5, ref newPositionToSetOnFixedUpdate);

                        return true;
                    }

                    return false;

                case Definitions.ActionType.Grab:
                    
                    if (!started) {
                        return false;
                    }
                    
                    var nearbyGrabables = character.NearbyInteractables();
                    if (nearbyGrabables == null || nearbyGrabables.Length == 0) {
                        return false;
                    }

                    closestColliders = new Collider2D[] { nearbyGrabables.FirstOrDefault() };

                    if (closestColliders == null) {
                        return false;
                    }
                    
                    if (started) {

                        isActionPerformed = true;
                        var legsPosition = character.LegsPosition;
                        var closestFoePosition = closestColliders.FirstOrDefault().ClosestPoint(legsPosition);
                        var dir = Vector2.up;//((Vector3)closestFoePosition - character.LegsPosition);

                        newPositionToSetOnFixedUpdate = closestFoePosition;// + Vector2.up * (character.LegsRadius);
                        Debug.DrawLine(character.LegsPosition, newPositionToSetOnFixedUpdate, Color.green, 3); // todo: move drawline to gameplaycontroller
                        //dir = closestFoePosition - newPositionToSetOnFixedUpdate;
                        forceDirToSetOnFixedUpdate = dir.normalized * stats.PushbackForce;
                        
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

                //default:
                    //throw new ArgumentOutOfRangeException();
            }
            return false;
        }

    }

}