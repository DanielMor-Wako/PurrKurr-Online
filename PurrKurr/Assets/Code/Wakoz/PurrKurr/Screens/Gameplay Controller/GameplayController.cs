using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller {

    [DefaultExecutionOrder(13)]
    public class GameplayController : SingleController {
        
        public event Action<ActionInput> OnTouchPadDown;
        public event Action<ActionInput> OnTouchPadClick;
        public event Action<ActionInput> OnTouchPadUp;
        public event Action<Character2DState> OnStateChanged;
        public event Action<List<DisplayedableStatData>> OnStatsChanged;

        [SerializeField] private CameraFollow _cam;
        [SerializeField] private Character2DController _hero;
        //private Character2DState _characterState;
        private bool _heroInitialized;

        private LayerMask _whatIsSolid;
        private LayerMask _whatIsSurface;
        private LayerMask _whatIsCharacter;

        private LogicController _logic;
        private InputController _input;
        private InputInterpreterLogic _inputInterpreterLogic; 
        private PadsDisplayController _uiDisplay;

        private void UpdateStats(int level) {
            if (_hero == null) {
                return;
            }
            _hero.Stats.UpdateStats(level);
        }

        protected override Task Initialize() {

            _cam ??= Camera.main.GetComponent<CameraFollow>();
            
            _input = SingleController.GetController<InputController>();
            _uiDisplay ??= SingleController.GetController<PadsDisplayController>();
            _logic = SingleController.GetController<LogicController>();

            if (_uiDisplay.TryBindToCharacterController(this)) {
                RegisterInputEvents();
            } else {
                _uiDisplay = null;
                Debug.Log("Something went wrong, there is no active ref to PadsDisplayController");
            }
            
            _whatIsSurface = _logic.GameplayLogic.GetSurfaces();
            _whatIsSolid = _logic.GameplayLogic.GetSolidSurfaces();
            _whatIsCharacter = _logic.GameplayLogic.GetDamageables();

            TryInitHero();
            
            return Task.CompletedTask;
        }

        [ContextMenu("Init Referenced Hero")]
        public void SetHeroAsReferenced() {
            TryInitHero(_hero);
        }
        
        public void SetNewHero(Character2DController hero) {
            TryInitHero(hero);
        }
        
        private bool TryInitHero(Character2DController hero = null) {

            hero ??= _hero;
            
            if (hero == null) {
                return false;
            }

            if (_hero != null && _heroInitialized) {
                _hero.OnStateChanged -= OnStateChanged;
                _hero.OnUpdatedStats -= UpdateOrInitUiDisplayForCharacter;
                _heroInitialized = false;
            }

            _hero = hero;
            _inputInterpreterLogic = new InputInterpreterLogic(_hero, _logic.InputLogic);
            SetHeroLevel(0);
            _cam.SetMainHero(_hero, true);

            _hero.OnStateChanged += OnStateChanged;
            _hero.OnUpdatedStats += UpdateOrInitUiDisplayForCharacter;

            _heroInitialized = true;

            return true;
        }

        [ContextMenu("Update Ui Display by Character Stats")]
        private void UpdateOrInitUiDisplayForCharacter(List<DisplayedableStatData> list = null) {

            if (_uiDisplay != null) {

                if (list != null) {
                    OnStatsChanged?.Invoke(list);
                } else {
                    _uiDisplay.Init(_hero);
                }
                
            }
        }

        private void SetHeroLevel(int heroLevel) {
            if (heroLevel == 0) {
                _hero.SetMinLevel();
            } else {
              _hero.SetMaxLevel();  
            }
        }
        
        public void SetHeroMinLevel() => _hero.SetMinLevel();
        public void SetHeroMaxLevel() => _hero.SetMaxLevel();
        
        protected override void Clean() {
            DeregisterInputEvents();
            if (_hero == null) {
                return;
            }
            _hero.OnStateChanged -= OnStateChanged;
            _hero.OnUpdatedStats -= UpdateOrInitUiDisplayForCharacter;
        }
        
        private void RegisterInputEvents() {

            if (_input == null) {
                Debug.LogError("Touch display has no available events for input");
                return;
            }
            
            _input.OnTouchPadDown += OnActionStarted;
            _input.OnTouchPadClick += OnActionOngoing;
            _input.OnTouchPadUp += OnActionEnded;
        }
        
        private void DeregisterInputEvents() {

            _input.OnTouchPadDown -= OnActionStarted;
            _input.OnTouchPadClick -= OnActionOngoing;
            _input.OnTouchPadUp -= OnActionEnded;

            _input = null;
        }

        private void OnActionStarted(ActionInput actionInput) {
            
            if (_hero.State.CanPerformAction()) {

                if (_inputInterpreterLogic.TryPerformInputNavigation(actionInput, true, false,
                        out var moveSpeed, out Vector2 forceDirNavigation, out var navigationDir)) {
                
                    // save for later to be called on tick?
                    _hero.DoMove(moveSpeed);
                    _hero.SetForceDir(forceDirNavigation); // used for airborne, probably more accurate after the diagnosis
                    _hero.SetNavigationDir(navigationDir);
                }
            
                if (_inputInterpreterLogic.TryPerformInputAction(actionInput, true, false,
                        out var isActionPerformed, out var forceDir, out var moveToPosition, out var interactedColliders)) {

                    if (isActionPerformed && interactedColliders != null) {
                        CombatLogic(_hero, actionInput.ActionType, moveToPosition, interactedColliders, forceDir);
                    }

                    var isJump = isActionPerformed && actionInput.ActionType == Definitions.ActionType.Jump;
                    if (isJump && forceDir != Vector2.zero) {
                        _hero.SetJumping(Time.time + .2f);
                        //_hero.SetForceDir(forceDir, true);
                        AlterJumpDirBasedOnNavigationDirection(ref forceDir, ref navigationDir, _hero.Stats.JumpForce);
                        _hero.SetForceDir(forceDir);
                        _hero.DoMove(0); // might conflict with the TryPerformInputNavigation when the moveSpeed is already set by Navigation
                    }

                    if (isActionPerformed) {
                        _hero.State.SetActiveCombatAbility(actionInput.ActionType);
                    }
                    
                }
            }
            
            OnTouchPadDown?.Invoke(actionInput);
        }

        private void AlterJumpDirBasedOnNavigationDirection(ref Vector2 forceDir, ref Definitions.NavigationType navigationDir, float jumpForce) {

            var inputLogic = _logic.InputLogic;

            Debug.DrawRay(_hero.LegsPosition, forceDir, Color.gray, 3);
            if (inputLogic.IsNavigationDirValidAsRight(navigationDir)) {
                forceDir.x = jumpForce;

            } else if (inputLogic.IsNavigationDirValidAsLeft(navigationDir)) {
                forceDir.x = -jumpForce;
            
            } else {
                forceDir.x = _hero.State.GetFacingRightAsInt();

            }
            Debug.DrawRay(_hero.LegsPosition, forceDir, Color.magenta, 3);
        }

        private void OnActionOngoing(ActionInput actionInput) {
            
            if (_inputInterpreterLogic.TryPerformInputNavigation(actionInput, false, false,
                    out var moveSpeed, out var forceDirNavigation, out var navigationDir)) {
                
                _hero.DoMove(moveSpeed);
                _hero.SetForceDir(forceDirNavigation);
                _hero.SetNavigationDir(navigationDir);
            }
            
            /*if (_inputInterpreterLogic.DiagnosePlayerInputAction(actionInput, false, false,
                    out var isActionPerformed, out Vector2 forceDirAction, out Vector2 moveToPosition, out Collider2D interactedCollider)) {
                
            }*/

            OnTouchPadClick?.Invoke(actionInput);
        }

        private void OnActionEnded(ActionInput actionInput) {
            
            if (_inputInterpreterLogic.TryPerformInputNavigation(actionInput, false, true,
                    out var moveSpeed, out Vector2 forceDirNavigation, out var navigationDir)) {
                
                _hero.DoMove(moveSpeed);
                _hero.SetForceDir(forceDirNavigation);
                _hero.SetNavigationDir(Definitions.NavigationType.None);
            }
            
            if (_inputInterpreterLogic.TryPerformInputAction(actionInput, false, true,
                    out var isActionPerformed, out var forceDir, out var moveToPosition, out var interactedCollider)) {
                
                if (forceDir != Vector2.zero) {
                    // might conflict with the DiagnosePlayerInputNavigation when the forceDir is already set by Navigation
                    _hero.SetForceDir(forceDir, true);
                }
                _hero.SetNewPosition(moveToPosition);

            }

            OnTouchPadUp?.Invoke(actionInput);
        }
/*
        private void Update() {

            
        }

        private void FixedUpdate() {

            //DoTick();

        }*/

        public void SetCameraScreenShake() => _cam.ShakeScreen(1, 0.5f);

        private void CombatLogic(Character2DController attacker, Definitions.ActionType actionType, Vector2 moveToPosition, Collider2D[] interactedColliders, Vector2 forceDirAction) {

            var newFacingDirection = 0;
            var attackAbility = _logic.AbilitiesLogic.GetAttackAbility(_hero.GetNavigationDir(), actionType, _hero.State.CurrentState);
            
            if (_hero.Stats.TryGetAttack(attackAbility, out var attackProperties)) {

                SetSingleOrMultipleTargets(attacker.GetGrabbedTarget(), ref attackProperties, ref interactedColliders);
                _hero.FilterNearbyCharactersAroundHitPointByDistance(ref interactedColliders, moveToPosition, attacker.Stats.MultiTargetsDistance);
                HitAvailableTargets(attacker, moveToPosition, interactedColliders, forceDirAction, ref newFacingDirection, ref attackAbility, ref attackProperties);
            
            } else {
                Debug.LogWarning($"Attack Ability {attackAbility} is missing from attack moves");
            }

            var isNewFacingDirectionSetAndCharacterIsFacingAnything = newFacingDirection != 0;
            if (isNewFacingDirectionSetAndCharacterIsFacingAnything) {

                attacker.FaceCharacterTowardsPoint(newFacingDirection == 1);
                attacker.SetNewPosition(moveToPosition);
            }

        }

        private void SetSingleOrMultipleTargets(IInteractableBody grabbedTarget, ref AttackBaseStats attackProperties, ref Collider2D[] targets) {
            
            if (attackProperties.Properties.Count == 0 || !attackProperties.Properties.Contains(Definitions.AttackProperty.MultiTargetOnSurfaceHit)) {
                
                if (grabbedTarget != null) {
                    targets = new Collider2D[] { grabbedTarget.GetCollider() };
                } else {
                    targets = new Collider2D[] { targets.FirstOrDefault() };
                }
                return;
            }
        }

        private void HitAvailableTargets(Character2DController attacker, Vector2 moveToPosition, Collider2D[] interactedColliders, Vector2 forceDirAction, ref int newFacingDirection, ref Definitions.AttackAbility attackAbility, ref AttackBaseStats attackProperties) {

            AttackStats attackStats = new AttackStats(attacker.Stats.Damage, forceDirAction);

            foreach (var col in interactedColliders) {
                var validAttack = TryPerformCombat(attacker, ref attackAbility, ref attackProperties, moveToPosition, col, attackStats);
                newFacingDirection = newFacingDirection == 0 ? validAttack : newFacingDirection;
            }
        }

        private int TryPerformCombat(Character2DController attacker, ref Definitions.AttackAbility attackAbility, ref AttackBaseStats attackProperties, Vector2 moveToPosition, Collider2D interactedCollider, AttackStats attackStats) {

            var damageable = interactedCollider.GetComponent<IInteractable>();

            var foe = damageable?.GetInteractable();

            var facingRightOrLeftTowardsPoint = 0;

            if (foe == null) {
                return facingRightOrLeftTowardsPoint;
            }

            var foeState = foe.GetCurrentState();

            if (ValidateAttackConditions(attackProperties, attacker.State, foe)) {

                facingRightOrLeftTowardsPoint = foe.GetCenterPosition().x > attacker.LegsPosition.x ? 1 : -1;
                
                var properties = attackProperties.Properties;
                var isFoeBlocking = foeState == Definitions.CharacterState.Blocking;

                if (_logic.AbilitiesLogic.IsAbilityAnAttack(attackAbility)) {
                    
                    if (properties.Contains(Definitions.AttackProperty.PushBackOnHit) ||
                        properties.Contains(Definitions.AttackProperty.PushBackOnBlock) && isFoeBlocking) {

                        ApplyForceOnFoeWithDelay(foe, attackStats, (_hero.Stats.AttackDurationInMilliseconds) );

                    } else if (properties.Contains(Definitions.AttackProperty.PushUpOnHit) ||
                                properties.Contains(Definitions.AttackProperty.PushUpOnBlock) && isFoeBlocking) {
                        
                        if ((attackStats.ForceDir.normalized.y) < 0.75f) { // 0.75f is the threshold to account is angle that is not 45 angle
                            Debug.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.yellow, 4);
                            attackStats.ForceDir.y = attacker.Stats.PushbackForce;
                        }

                        attackStats.ForceDir.x *= 0.05f;

                        ApplyForceOnFoeWithDelay(foe, attackStats, (_hero.Stats.AttackDurationInMilliseconds));

                    } else if (properties.Contains(Definitions.AttackProperty.PushDiagonalOnHit) ||
                                properties.Contains(Definitions.AttackProperty.PushDiagonalOnBlock) && isFoeBlocking) {
                    
                        //Debug.Log($"forceDirAction.y {forceDirAction.ForceDir.normalized.y} during PushDiagonal");

                        if ((attackStats.ForceDir.normalized.y) < 0.1f) {
                            Debug.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.yellow, 4);
                            attackStats.ForceDir.y = attacker.Stats.PushbackForce;
                        }
                        attackStats.ForceDir.x *= 0.5f;

                        ApplyForceOnFoeWithDelay(foe, attackStats, (_hero.Stats.AttackDurationInMilliseconds));

                    }
                        
                } else if (_logic.AbilitiesLogic.IsAbilityAGrab(attackAbility)) {

                    //var abovePosition = new Vector3(moveToPosition.x, moveToPosition.y + attacker.LegsRadius, 0);
                    var isGroundSmash =properties.Contains(Definitions.AttackProperty.GrabToGroundSmash) ||
                        foe.GetCurrentState() is Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling;
                    var grabPointOffset = (isGroundSmash) ? Vector2.down : Vector2.up;
                    var endPosition = moveToPosition + grabPointOffset * (attacker.LegsRadius);
                    var attackerAsInteractable = (IInteractableBody)attacker;

                    var isThrowingGrabbedFoe = attackerAsInteractable.IsGrabbing() && foe.IsGrabbed();
                    if (isThrowingGrabbedFoe) {

                        ApplyGrabOnFoeWithDelay(foe, null, foe.GetTransform().position);
                        ApplyForceOnFoeWithDelay(foe, attackStats, _hero.Stats.AttackDurationInMilliseconds);
                        attackerAsInteractable.SetAsGrabbing(null);
                        ((Character2DController)foe)?.ActAsProjectileWhileThrown(attackStats.Damage, attackerAsInteractable);

                    } else {
                        
                        foe.SetTargetPosition(endPosition);

                        _cam.RemoveFromTargetList(foe.GetTransform());
                        ApplyGrabOnFoeWithDelay(foe, attacker, endPosition, _hero.Stats.AttackDurationInMilliseconds);
                        attackerAsInteractable.SetAsGrabbing(foe);
                        Debug.DrawRay(moveToPosition, Vector2.up * 10, Color.grey, 4);
                        Debug.DrawRay(endPosition, Vector2.up * 10, Color.yellow, 4);

                        if (isGroundSmash) {

                            ApplyGrabThrowWhenGrounded(foe, attacker, attackStats);

                        } else if (properties.Contains(Definitions.AttackProperty.GrabAndPickUp)) {

                        } else {

                            var directionTowardsFoe = (endPosition.x >= attacker.LegsPosition.x) ? 1 : -1;

                            if (properties.Contains(Definitions.AttackProperty.StunOnGrabber)) {

                                attackStats.ForceDir.x = -directionTowardsFoe * 0.3f * Mathf.Abs(attackStats.ForceDir.y);
                                ApplyGrabThrowWhenGrounded(foe, attacker, attackStats);
                            
                            } else {

                                attackStats.ForceDir.x = directionTowardsFoe * Mathf.Abs(attackStats.ForceDir.y);
                                attackStats.ForceDir.y *= 0.4f;
                                ApplyGrabThrowWhenGrounded(foe, attacker, attackStats);
                            }
                        }

                    }

                    if (properties.Contains(Definitions.AttackProperty.PenetrateDodge)) {

                    }

                    if (properties.Contains(Definitions.AttackProperty.StunResist)) {

                    }

                }

            }

            return facingRightOrLeftTowardsPoint;
        }

        private bool ValidateAttackConditions(AttackBaseStats attack, Character2DState characterState, IInteractableBody interactable) {

            var state = characterState.CurrentState;
            var opponentState = interactable.GetCurrentState();


            foreach (var condition in attack.CharacterStateConditions) {

                switch (condition) {
                    case Definitions.CharacterState.Running when state != Definitions.CharacterState.Running:
                    case Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling when !(characterState.IsStateConsideredAsAerial()):
                    case Definitions.CharacterState.Grounded when !characterState.IsStateConsideredAsGrounded():
                    case Definitions.CharacterState.Crouching when state != Definitions.CharacterState.Crouching:
                    case Definitions.CharacterState.StandingUp when state != Definitions.CharacterState.StandingUp:
                    case Definitions.CharacterState.Blocking when state != Definitions.CharacterState.Blocking:
                    case Definitions.CharacterState.Grabbing when state != Definitions.CharacterState.Grabbing:
                        return false;
                }

            }
            
            foreach (var condition in attack.OpponentStateConditions) {
//todo: replace the using of another character to analyze the opponent state, the is state grounded or jumping/falling should be determined by some game logic
                switch (condition) {
                    // todo: change this to check when a player is grounded but is considered flying type, so the state of the opponent is grounded when in midair and not falling
                    case Definitions.CharacterState.Falling or Definitions.CharacterState.Jumping or Definitions.CharacterState.Grounded
                        when state is (Definitions.CharacterState.Falling or Definitions.CharacterState.Jumping or Definitions.CharacterState.Grounded) :
                        return true;

                    case Definitions.CharacterState.Running when opponentState != Definitions.CharacterState.Running:
                    case Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling when !(characterState.IsStateConsideredAsAerial(opponentState)):
                    case Definitions.CharacterState.Grounded when !characterState.IsStateConsideredAsGrounded(opponentState) && !interactable.IsGrabbed():
                    case Definitions.CharacterState.Crouching when opponentState != Definitions.CharacterState.Crouching:
                    case Definitions.CharacterState.StandingUp when opponentState != Definitions.CharacterState.StandingUp:
                    case Definitions.CharacterState.Blocking when opponentState != Definitions.CharacterState.Blocking:
                    case Definitions.CharacterState.Grabbing when opponentState != Definitions.CharacterState.Grabbing:
                        return false;
                }

            }

            return true;
        }

        private async void ApplyForceOnFoeWithDelay(IInteractableBody damageableBody, AttackStats attackStats, int delayInMilliseconds = 0) {
            
            _cam.AddToTargetList(damageableBody.GetTransform());
            //foeRigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
            
            if (delayInMilliseconds > 0) {
                await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));
            }
            
                
            if (damageableBody == null) {
                return;
            }
            //foeRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
            /*if (forceDirAction.normalized.y < -0.2f && Mathf.Abs(damageableBody.GetVelocity().y) < 1) {
                // when the force to apply is downwards and the enemy is grounded? 
                forceDirAction.y *= -0.5f;
                forceDirAction.x *= 0.5f;
            }*/

            var modifiedHealth = damageableBody.DealDamage(attackStats.Damage);
            var modifiedForce = modifiedHealth > 0 ? attackStats.ForceDir : AlterDirectionToMaxForce(attackStats.ForceDir);
            damageableBody.ApplyForce(modifiedForce);
            
            Debug.DrawRay(damageableBody.GetCenterPosition(), attackStats.ForceDir, Color.red, 4);

            var distanceFromAttacker = Vector2.Distance(damageableBody.GetCenterPosition(), _hero.LegsPosition);
            while ( distanceFromAttacker < 15 ) {
                await Task.Delay((1000));
                distanceFromAttacker = Vector2.Distance(damageableBody.GetCenterPosition(), _hero.LegsPosition);
            }

            if (damageableBody != null) {
                _cam.RemoveFromTargetList(damageableBody.GetTransform());
            }
            
        }

        private Vector2 AlterDirectionToMaxForce(Vector2 forceDirAction) {

            var xDirection = forceDirAction.x * 1.4f;
            var yDirection = forceDirAction.y * 0.55f;
            return new Vector2(xDirection, yDirection);
        }

        private async void ApplyGrabOnFoeWithDelay(IInteractableBody grabbed, IInteractableBody grabber, Vector2 position, int delayInMilliseconds = 0) {

            if (delayInMilliseconds > 0) {
                await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds));
            }

            if (grabbed == null) {
                return;
            }

            grabbed.SetAsGrabbed(grabber, position);

        }

        private async void ApplyGrabThrowWhenGrounded(IInteractableBody grabbed, IInteractableBody grabber, AttackStats attackStats) {

            await Task.Delay(150);

            while (grabber.GetCurrentState() is not (Definitions.CharacterState.Grounded or Definitions.CharacterState.Running or Definitions.CharacterState.Crouching or Definitions.CharacterState.StandingUp)) {

                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
                
            if (grabbed == null || !grabber.IsGrabbing()) {
                return;
            }

            grabber.SetAsGrabbing(null);
            ApplyGrabOnFoeWithDelay(grabbed, null, grabbed.GetCenterPosition());
            ApplyForceOnFoeWithDelay(grabbed, attackStats);
            ((Character2DController)grabbed)?.ActAsProjectileWhileThrown(attackStats.Damage, grabber);

        }
        
    }

}
