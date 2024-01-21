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

        private Character2DController _hero;
        private InputLogic _inputLogic;

        public InputInterpreterLogic(Character2DController hero, InputLogic inputLogic) {
            _hero = hero;
            _inputLogic = inputLogic;
        }

        public bool TryPerformInputNavigation(ActionInput actionInput, bool started, bool ended,
            out float moveSpeed, out Vector2 forceDirToSetOnFixedUpdate, out Definitions.NavigationType navigationDir) {
            
            moveSpeed = 0;
            forceDirToSetOnFixedUpdate = Vector2.zero;
            navigationDir = Definitions.NavigationType.None;
            
            if (_hero == null || actionInput.ActionGroupType != Definitions.ActionTypeGroup.Navigation) {
                return false;
            }

            navigationDir = _inputLogic.GetInputDirection(actionInput.NormalizedDirection);
            
            if (ended) {
                _hero.State.SetCrouchOrStandingByUpDownInput(Definitions.NavigationType.None);
                return true;
            }
            
            _hero.State.SetCrouchOrStandingByUpDownInput(navigationDir);
            
            if ( (_hero.State.IsCeiling() || _hero.State.IsFrontWall()) && navigationDir != Definitions.NavigationType.None) {
                // full charge towards walls when any nav pad is held
                moveSpeed = -_hero.Stats.SprintSpeed * (_inputLogic.IsNavigationDirValidAsRight(navigationDir) ? 1 : -1);
                return true;
            }

            switch (navigationDir) {
                case Definitions.NavigationType.Right:
                    moveSpeed = -GetHorizontalSpeedBySwipeDistance(actionInput.SwipeDistanceTraveledInPercentage);
                    break;

                case Definitions.NavigationType.DownRight:
                    moveSpeed = -_hero.Stats.WalkSpeed;
                    break;
                
                case Definitions.NavigationType.UpRight:
                    moveSpeed = -_hero.Stats.WalkSpeed;
                    break;

                case Definitions.NavigationType.Left:
                    moveSpeed = GetHorizontalSpeedBySwipeDistance(actionInput.SwipeDistanceTraveledInPercentage);
                    break;

                case Definitions.NavigationType.DownLeft:
                    moveSpeed = _hero.Stats.WalkSpeed;
                    break;
                
                case Definitions.NavigationType.UpLeft:
                    moveSpeed = _hero.Stats.WalkSpeed;
                    break;
            }

            // Air-borne movement
            var rigidbodyVelocity = _hero.State.Velocity;
            if (rigidbodyVelocity.y < -1f && _hero.State.CurrentState == Definitions.CharacterState.Falling) {
                switch (navigationDir) {
                    
                    case Definitions.NavigationType.Right or Definitions.NavigationType.DownRight or Definitions.NavigationType.UpRight
                        when (rigidbodyVelocity.x < _hero.Stats.AirborneMaxSpeed):

                        forceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x + _hero.Stats.AirborneSpeed, rigidbodyVelocity.y);
                        moveSpeed = 0;
                        return true;
                    
                    case Definitions.NavigationType.Left or Definitions.NavigationType.DownLeft or Definitions.NavigationType.UpLeft
                        when rigidbodyVelocity.x > -_hero.Stats.AirborneMaxSpeed:
                        
                        forceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x - _hero.Stats.AirborneSpeed, rigidbodyVelocity.y);
                        return true;
                }
            }

            return true;
        }

        private float GetHorizontalSpeedBySwipeDistance(float swipeDistanceDone) {

            return swipeDistanceDone * _hero.Stats.SprintSpeed;
        }

        public bool TryPerformInputAction(ActionInput actionInput, bool started, bool ended,
            out bool isActionPerformed, out Vector2 forceDirToSetOnFixedUpdate, out Vector2 newPositionToSetOnFixedUpdate, out Collider2D[] closestColliders ) {

            isActionPerformed = false;
            forceDirToSetOnFixedUpdate = Vector2.zero;
            newPositionToSetOnFixedUpdate = Vector2.zero;
            closestColliders = null;
            
            if (_hero == null || actionInput.ActionGroupType != Definitions.ActionTypeGroup.Action) {
                return false;
            }

            var heroAsInteractableBody = (IInteractableBody)_hero;
            if (started && actionInput.ActionType is Definitions.ActionType.Attack or Definitions.ActionType.Grab
                && heroAsInteractableBody.IsGrabbing()) {

                closestColliders = new Collider2D[] { _hero.GetGrabbedTarget().GetCollider() };
                forceDirToSetOnFixedUpdate = new Vector2(_hero.State.GetFacingRightAsInt(), 1) * _hero.Stats.PushbackForce;
                isActionPerformed = true;
                return true;
            }

            var rigidbodyVelocity = _hero.State.Velocity;
            
            switch (actionInput.ActionType) {
                
                case Definitions.ActionType.Jump:
                    if (!ended && _hero.State.IsJumpingAllowed()) {
                        isActionPerformed = true;
                        forceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x, _hero.Stats.JumpForce);
                    } else if (ended && _hero.State.Velocity.y > 0 && _hero.State.CurrentState == Definitions.CharacterState.Jumping) {
                        if (!(forceDirToSetOnFixedUpdate != Vector2.zero)) {
                            forceDirToSetOnFixedUpdate = new Vector2(rigidbodyVelocity.x, 0);
                        } else {//if (forceDirToSetOnFixedUpdate != Vector2.zero) {
                            forceDirToSetOnFixedUpdate = Vector2.zero;
                        }
                    }
                    return true;

                case Definitions.ActionType.Attack:

                    var nearbyEnemies = _hero.NearbyCharacters();
                    if (nearbyEnemies == null || nearbyEnemies.Length == 0) {
                        return false;
                    }

                    closestColliders = nearbyEnemies.OrderBy(obj 
                        => Vector2.Distance(obj.transform.position, _hero.LegsPosition)).ToArray();

                    if (closestColliders == null) {
                        return false;
                    }
                    
                    if (started) {

                        isActionPerformed = true;
                        var closestColl = closestColliders.FirstOrDefault();
                        var legsPosition = _hero.LegsPosition;
                        var closestFoePosition = closestColl.ClosestPoint(legsPosition);
                        var dir = ((Vector3)closestFoePosition - legsPosition);
                        var directionTowardsFoe = (closestColl.transform.position.x > legsPosition.x) ? 1 : -1;
                        if (dir == Vector3.zero) {
                            dir = new Vector3(directionTowardsFoe, 0, 0);
                        }
                        newPositionToSetOnFixedUpdate = closestFoePosition + (Vector2)dir.normalized * -(_hero.LegsRadius);
                        Debug.DrawLine(legsPosition, newPositionToSetOnFixedUpdate, Color.green, 3);
                        dir = closestFoePosition - newPositionToSetOnFixedUpdate;
                        forceDirToSetOnFixedUpdate = dir.normalized * _hero.Stats.PushbackForce;
                        return true;
                    }
                    
                    break;

                case Definitions.ActionType.Block:
                    break;

                case Definitions.ActionType.Grab:
                    
                    if (!started) {
                        return false;
                    }
                    
                    var nearbyGrabables = _hero.NearbyCharacters();
                    if (nearbyGrabables == null || nearbyGrabables.Length == 0) {
                        return false;
                    }

                    closestColliders = nearbyGrabables.OrderBy(obj => Vector3.Distance(obj.transform.position, _hero.LegsPosition)).ToArray();

                    closestColliders = new Collider2D[] { closestColliders.FirstOrDefault() };

                    if (closestColliders == null) {
                        return false;
                    }
                    
                    if (started) {

                        isActionPerformed = true;
                        var legsPosition = _hero.LegsPosition;
                        var closestFoePosition = closestColliders.FirstOrDefault().ClosestPoint(legsPosition);
                        var dir = Vector2.up;//((Vector3)closestFoePosition - _hero.LegsPosition);

                        newPositionToSetOnFixedUpdate = closestFoePosition;// + Vector2.up * (_hero.LegsRadius);
                        Debug.DrawLine(_hero.LegsPosition, newPositionToSetOnFixedUpdate, Color.green, 3);
                        //dir = closestFoePosition - newPositionToSetOnFixedUpdate;
                        forceDirToSetOnFixedUpdate = dir.normalized * _hero.Stats.PushbackForce;
                        
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