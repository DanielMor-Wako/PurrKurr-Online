using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using UnityEngine;
using System.Linq;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;

// todo: move to a better namespace like player input
namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller {
    public class InputInterpreterLogic {

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

            state.SetCrouchOrStandingByUpDownInput(navigationDir);
            var isGrabbing = state.IsGrabbing();

            // full charge towards walls when any nav pad is held
            //if (!isGrabbing && (state.IsCeiling() || state.IsFrontWall()) && navigationDir != Definitions.NavigationType.None) {
            if (!isGrabbing && state.HasAnySurfaceAround() && navigationDir != Definitions.NavigationType.None) {
                if (navigationDir == Definitions.NavigationType.Up && (state.IsCeiling() || state.IsFrontWall())) {
                    moveSpeed = -stats.SprintSpeed * state.GetFacingRightAsInt();
                    return true;
                } else if (navigationDir != Definitions.NavigationType.Up && navigationDir != Definitions.NavigationType.Down) {
                    moveSpeed = actionInput.SwipeDistanceTraveledInPercentage * -stats.SprintSpeed * (_inputLogic.IsNavigationDirValidAsRight(navigationDir) ? 1 : -1);
                    return true;
                }
            }

            bool isCrouchingState = state.CurrentState != Definitions.CharacterState.Running && state.IsCrouching();
            bool isStandingState = (state.CurrentState != Definitions.CharacterState.Running && state.IsStandingUp() || isGrabbing);

            // Air-borne movement
            var rigidbodyVelocity = state.Velocity;
            var isNavRightDir = navigationDir is Definitions.NavigationType.Right or Definitions.NavigationType.DownRight or Definitions.NavigationType.UpRight;
            var isNavLeftDir = navigationDir is Definitions.NavigationType.Left or Definitions.NavigationType.DownLeft or Definitions.NavigationType.UpLeft;


            if (rigidbodyVelocity.y < -1f && state.CurrentState == Definitions.CharacterState.Falling ) {
            //if (rigidbodyVelocity.y < -1f && !state.IsJumping() && _gameplayLogic.IsStateConsideredAsAerial(state.CurrentState)) {

                if (isNavRightDir && (rigidbodyVelocity.x < stats.AirborneMaxSpeed)) {

                    forceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x + stats.AirborneSpeed, rigidbodyVelocity.y);

                    return true;

                } else if (isNavLeftDir && (rigidbodyVelocity.x > -stats.AirborneMaxSpeed)) {

                    forceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x - stats.AirborneSpeed, rigidbodyVelocity.y);
                    moveSpeed = 0;
                    return true;
                }

            } else if (!state.IsCeiling() && (state.IsFrontWall() || state.IsGrounded())) { 
            
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

            var rigidbodyVelocity = state.Velocity;
            
            switch (actionInput.ActionType) {
                
                case Definitions.ActionType.Jump:

                    if (!ended && state.CanPerformJump(_gameplayLogic.IsStateConsideredAsGrounded(state.CurrentState))) {
                        isActionPerformed = true;
                        forceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x, stats.JumpForce);

                    } else if (ended && state.Velocity.y > 0 && state.CurrentState == Definitions.CharacterState.Jumping) {
                        if (!(forceDirToSetOnFixedUpdate != Vector2.zero)) {
                            forceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x, 0);
                        } else {//if (forceDirToSetOnFixedUpdate != Vector2.zero) {
                            forceDirToSetOnFixedUpdate = Vector2.zero;
                        }
                    }
                    return true;

                case Definitions.ActionType.Attack:

                    var nearbyEnemies = character.NearbyInteractables();
                    if (nearbyEnemies == null || nearbyEnemies.Length == 0) {
                        return false;
                    }

                    closestColliders = nearbyEnemies.OrderBy(obj => Vector2.Distance(obj.transform.position, character.LegsPosition)).ToArray();

                    if (closestColliders == null) {
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
                        Debug.DrawLine(legsPosition, newPositionToSetOnFixedUpdate, Color.green, 3);
                        dir = closestFoePosition - newPositionToSetOnFixedUpdate;
                        forceDirToSetOnFixedUpdate = dir.normalized * stats.PushbackForce;
                        return true;
                    }
                    
                    break;

                case Definitions.ActionType.Block:
                    break;

                case Definitions.ActionType.Grab:
                    
                    if (!started) {
                        return false;
                    }
                    
                    var nearbyGrabables = character.NearbyInteractables();
                    if (nearbyGrabables == null || nearbyGrabables.Length == 0) {
                        return false;
                    }

                    closestColliders = nearbyGrabables.OrderBy(obj => Vector3.Distance(obj.transform.position, character.LegsPosition)).ToArray();

                    closestColliders = new Collider2D[] { closestColliders.FirstOrDefault() };

                    if (closestColliders == null) {
                        return false;
                    }
                    
                    if (started) {

                        isActionPerformed = true;
                        var legsPosition = character.LegsPosition;
                        var closestFoePosition = closestColliders.FirstOrDefault().ClosestPoint(legsPosition);
                        var dir = Vector2.up;//((Vector3)closestFoePosition - character.LegsPosition);

                        newPositionToSetOnFixedUpdate = closestFoePosition;// + Vector2.up * (character.LegsRadius);
                        Debug.DrawLine(character.LegsPosition, newPositionToSetOnFixedUpdate, Color.green, 3);
                        //dir = closestFoePosition - newPositionToSetOnFixedUpdate;
                        forceDirToSetOnFixedUpdate = dir.normalized * stats.PushbackForce;
                        
                        return true;
                    }
                    
                    break;

                case Definitions.ActionType.Projectile:
                    break;

                case Definitions.ActionType.Rope:
                    break;

                case Definitions.ActionType.Special:
                    break;

                //default:
                    //throw new ArgumentOutOfRangeException();
            }
            return false;
        }

    }

}