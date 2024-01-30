using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.Screens.CameraComponents;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDetection;
using Code.Wakoz.Utils.Extensions;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller {

    [DefaultExecutionOrder(14)]
    public class GameplayController : SingleController {
        
        public event Action<ActionInput> OnTouchPadDown;
        public event Action<ActionInput> OnTouchPadClick;
        public event Action<ActionInput> OnTouchPadUp;
        public event Action<Character2DState> OnStateChanged;
        public event Action<List<DisplayedableStatData>> OnStatsChanged;

        [SerializeField] private CameraFollow _cam;
        [SerializeField] private List<Character2DController> _heroes;
        [SerializeField] private Character2DController _mainHero;
        private Character2DController _hero;

        private bool _firstTimeInitialized = false;
        private bool _heroInitialized = false;
        private bool _gameRunning = false;

        private LogicController _logic;
        private InputController _input;
        private InputInterpreterLogic _inputInterpreterLogic; 
        private UIController _ui;

        private LayerMask _whatIsSolid;
        private LayerMask _whatIsSurface;
        private LayerMask _whatIsCharacter;
        private LayerMask _whatIsDamageableCharacter;

        protected override Task Initialize() {

            _cam ??= Camera.main.GetComponent<CameraFollow>();
            
            _input = SingleController.GetController<InputController>();
            _logic = SingleController.GetController<LogicController>();
            _inputInterpreterLogic = new InputInterpreterLogic(_logic.InputLogic, _logic.GameplayLogic);

            _ui ??= SingleController.GetController<UIController>();
            if (_ui.TryBindToCharacterController(this)) {
                RegisterInputEvents();
            } else {
                _ui = null;
                Debug.Log("Something went wrong, there is no active ref to PadsDisplayController");
            }
            
            _whatIsSurface = _logic.GameplayLogic.GetSurfaces();
            _whatIsSolid = _logic.GameplayLogic.GetSolidSurfaces();
            _whatIsCharacter = _logic.GameplayLogic.GetDamageables();
            _whatIsDamageableCharacter = _logic.GameplayLogic.GetDamageables();

            TryInitHero(_mainHero);

            RefreshSpritesOrder();

            _gameRunning = true;

            return Task.CompletedTask;
        }

        private void RefreshSpritesOrder() {
            
            foreach(var character in _heroes) {
                if (character == null) {
                    continue;
                }
                var isAlive = character.Stats.GetHealthPercentage() > 0;
                character.SetSpriteOrder(character == _hero ? 2 : isAlive ? 1 : 0);
            }
        }

        [ContextMenu("Init Referenced Hero")]
        public void SetHeroAsReferenced() {
            TryInitHero(_mainHero);
            _ui.InitUiForCharacter(_mainHero);
            RefreshSpritesOrder();
        }

        public void SetNewHero(Character2DController hero) {
            TryInitHero(hero);
        }
        
        private bool TryInitHero(Character2DController hero) {

            if (hero == null) {
                return false;
            }

            if (_hero != null && _heroInitialized) {
                _hero.OnStateChanged -= OnStateChanged;
                _hero.OnUpdatedStats -= UpdateOrInitUiDisplayForCharacter;
            }
            
            _hero = hero;

            _cam.SetMainHero(_hero, true);

            _hero.OnStateChanged += OnStateChanged;
            _hero.OnUpdatedStats += UpdateOrInitUiDisplayForCharacter;

            _firstTimeInitialized = true;

            _heroInitialized = true;

            return true;
        }

        [ContextMenu("Update Ui Display by Character Stats")]
        private void UpdateOrInitUiDisplayForCharacter(List<DisplayedableStatData> list = null) {

            if (_ui != null) {

                if (list != null) {
                    OnStatsChanged?.Invoke(list);
                } else {
                    _ui.InitUiForCharacter(_hero);
                }
                
                if (!_firstTimeInitialized) { return; }
                _firstTimeInitialized = false;
                RefreshSpritesOrder();
            }
        }

        private void SetHeroLevel(int heroLevel) {

            if (heroLevel == 0) {
                _hero.SetMinLevel();
            
            } else {
              _hero.SetMaxLevel();  
            }
        }

        public void SetHeroMinLevel() {
            _hero.SetMinLevel();
        }
        public void SetHeroMaxLevel() => _hero.SetMaxLevel();
        public void RandomizeHero() {
            if (_heroes == null) { return; }
            _mainHero = HelperFunctions.Random(_heroes);
            SetHeroAsReferenced();
        }
        public void SetTimeScaleMin() => Time.timeScale = 0.3f;
        public void SetTimeScaleHalf() => Time.timeScale = 0.5f;
        public void SetTimeScaleTwoQuaters() => Time.timeScale = 0.8f;
        public void SetTimeScaleMax() => Time.timeScale = 1f;

        private void UpdateStatsOnLevelUp(int level) {
            if (_hero == null) {
                return;
            }
            _hero.Stats.UpdateStats(level);
        }

        protected override void Clean() {

            DeregisterInputEvents();

            _gameRunning = false;

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
            
            if (_hero.State.CanPerformAction() && _hero.Stats.GetHealthPercentage() > 0) {

                if (_inputInterpreterLogic.TryPerformInputNavigation(actionInput, true, false, _hero,
                        out var moveSpeed, out Vector2 forceDirNavigation, out var navigationDir)) {

                    // save for later to be called on tick?
                    _hero.DoMove(moveSpeed);
                    _hero.SetForceDir(forceDirNavigation); // used for airborne, probably more accurate after the diagnosis
                    _hero.SetNavigationDir(navigationDir);
                    if (_hero.IsGrabbed()) {
                        FaceCharacterTowardsPointByNavigationDir(navigationDir);
                    }
                }
            
                if (_inputInterpreterLogic.TryPerformInputAction(actionInput, true, false, _hero,
                        out var isActionPerformed, out var forceDir, out var moveToPosition, out var interactedColliders)) {

                    if (isActionPerformed && interactedColliders != null && !_hero.State.IsGrabbed()) {
                        CombatLogic(_hero, actionInput.ActionType, moveToPosition, interactedColliders, forceDir);
                    }

                    var isJump = isActionPerformed && actionInput.ActionType == Definitions.ActionType.Jump;
                    if (isJump && forceDir != Vector2.zero) {
                        _hero.SetJumping(Time.time + .2f);
                        AlterJumpDirBasedOnNavigationDirection(ref forceDir, _hero.State.NavigationDir, _hero.Stats.JumpForce);
                        _hero.SetForceDir(forceDir);
                        _hero.DoMove(0); // might conflict with the TryPerformInputNavigation when the moveSpeed is already set by Navigation
                    }

                    if (isActionPerformed) {
                        _hero.State.SetActiveCombatAbility(actionInput.ActionType);
                    }
                    
                }
            } else {
                _hero.DoMove(0);
            }
            
            OnTouchPadDown?.Invoke(actionInput);
        }

        private void FaceCharacterTowardsPointByNavigationDir(Definitions.NavigationType navigationDir) {

            if (_logic.InputLogic.IsNavigationDirValidAsRight(navigationDir)) {
                _hero.FaceCharacterTowardsPoint(true);
            } else if (_logic.InputLogic.IsNavigationDirValidAsLeft(navigationDir)) {
                _hero.FaceCharacterTowardsPoint(false);
            }
        }

        private void OnActionOngoing(ActionInput actionInput) {
            
            if (_hero.State.CanPerformAction() && _hero.Stats.GetHealthPercentage() > 0) {

                if (_inputInterpreterLogic.TryPerformInputNavigation(actionInput, false, false, _hero, 
                        out var moveSpeed, out var forceDirNavigation, out var navigationDir)) {
                    
                    _hero.DoMove(moveSpeed);
                    _hero.SetForceDir(forceDirNavigation);
                    _hero.SetNavigationDir(navigationDir);
                    if (!_hero.State.IsAnimating() && navigationDir != Definitions.NavigationType.None) {
                        
                        bool? facingRight = _logic.InputLogic.IsNavigationDirValidAsRight(navigationDir) ? true :
                            _logic.InputLogic.IsNavigationDirValidAsLeft(navigationDir) ? false : null;

                        if (facingRight != null) {
                            _hero.FaceCharacterTowardsPoint(facingRight == true);

                        }
                        
                    }
                }
            
                /*if (_inputInterpreterLogic.DiagnosePlayerInputAction(actionInput, false, false,
                        out var isActionPerformed, out Vector2 forceDirAction, out Vector2 moveToPosition, out Collider2D interactedCollider)) {
                
                }*/
            }

            OnTouchPadClick?.Invoke(actionInput);
        }

        private void OnActionEnded(ActionInput actionInput) {

            //if (_hero.State.CanPerformAction()) {

                if (_inputInterpreterLogic.TryPerformInputNavigation(actionInput, false, true, _hero, 
                        out var moveSpeed, out Vector2 forceDirNavigation, out var navigationDir)) {
                
                    _hero.DoMove(moveSpeed);
                    _hero.SetForceDir(forceDirNavigation);
                    _hero.SetNavigationDir(Definitions.NavigationType.None);
                }
            
                if (_inputInterpreterLogic.TryPerformInputAction(actionInput, false, true, _hero,
                        out var isActionPerformed, out var forceDir, out var moveToPosition, out var interactedCollider)) {
                
                    if (forceDir != Vector2.zero) {
                        // might conflict with the DiagnosePlayerInputNavigation when the forceDir is already set by Navigation
                        _hero.SetForceDir(forceDir, true);
                    }
                    _hero.SetNewPosition(moveToPosition);

                }
            //}
            OnTouchPadUp?.Invoke(actionInput);
        }

        private void AlterJumpDirBasedOnNavigationDirection(ref Vector2 forceDir, Definitions.NavigationType navigationDir, float jumpForce) {

            var inputLogic = _logic.InputLogic;

            Debug.DrawRay(_hero.LegsPosition, forceDir, Color.gray, 3);
            if (navigationDir == Definitions.NavigationType.Up) {
                forceDir.x = _hero.State.GetFacingRightAsInt();
                
            } else if (inputLogic.IsNavigationDirValidAsRight(navigationDir)) {
                forceDir.x = jumpForce * 0.5f;

            } else if (inputLogic.IsNavigationDirValidAsLeft(navigationDir)) {
                forceDir.x = -jumpForce * 0.5f;

            } else {
                forceDir.x = _hero.State.GetFacingRightAsInt() * jumpForce * 0.25f;

            }
            Debug.DrawRay(_hero.LegsPosition, forceDir, Color.magenta, 3);
        }

        private void AlterThrowDirBasedOnNavigationDirection(ref Vector2 throwDir, Definitions.NavigationType navigationDir, float pushbackForce) {

            Debug.DrawRay(_hero.LegsPosition, throwDir, Color.gray, 3);
            if (!_logic.InputLogic.IsNavigationDirValidAsUp(navigationDir)) {
                throwDir = AlterDirectionToMaxForce(throwDir);
            }
            Debug.DrawRay(_hero.LegsPosition, throwDir, Color.magenta, 3);
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
            var attackAbility = _logic.AbilitiesLogic.GetAttackAbility(attacker.GetNavigationDir(), actionType, attacker.State.CurrentState);

            if (attacker.Stats.TryGetAttack(ref attackAbility, out var attackProperties)) {

                ValidateAttackCondition(ref attacker, ref attackAbility, ref attackProperties);
                GetAvailableTargets(ref attacker, ref attackProperties, moveToPosition, ref interactedColliders, out var interactableBodies);
                HitTargets(attacker, ref moveToPosition, ref interactableBodies, forceDirAction, ref newFacingDirection, ref attackAbility, ref attackProperties);
            
            } else {
                Debug.LogWarning($"Attack Ability {attackAbility} is missing from attack moves");
            }

            var isNewFacingDirectionSetAndCharacterIsFacingAnything = newFacingDirection != 0;
            if (isNewFacingDirectionSetAndCharacterIsFacingAnything) {

                attacker.FaceCharacterTowardsPoint(newFacingDirection == 1);
                attacker.SetNewPosition(moveToPosition);
            }

        }

        private void ValidateAttackCondition(ref Character2DController attacker, ref Definitions.AttackAbility attackAbility, ref AttackBaseStats attackProperties) { 
            
            var attackAvailable = ValidateAttackConditions(attackProperties, attacker.State);

            if (!attackAvailable) {

                if (ValidateAlternativeAttackConditions(ref attackAbility, ref attackProperties, attacker.State)) {
                    attackAvailable = attacker.Stats.TryGetAttack(ref attackAbility, out var alternativeAttackProperties);
                    if (attackAvailable) {
                        attackProperties = alternativeAttackProperties;
                    }

                } else {
                    Debug.Log($"no alternative attack for {attackAbility}");
                }
            }

        }

        private void GetAvailableTargets(ref Character2DController attacker, ref AttackBaseStats attackProperties, Vector2 moveToPosition, ref Collider2D[] interactedColliders, out IInteractableBody[] interactableBodies) {

            interactableBodies = new IInteractableBody[] { };
            SetSingleOrMultipleTargets(attacker.GetGrabbedTarget(), ref interactedColliders, ref interactableBodies );
            
            var canAttackMultiTarget = attackProperties.Properties.Count > 0 && attackProperties.Properties.Contains(Definitions.AttackProperty.MultiTargetOnSurfaceHit);
            if (canAttackMultiTarget && interactedColliders.Length > 1) {
                attacker.FilterNearbyCharactersAroundHitPointByDistance(ref interactableBodies, moveToPosition, attacker.Stats.MultiTargetsDistance);
            }

        }

        private void SetSingleOrMultipleTargets(IInteractableBody grabbedTarget, ref Collider2D[] interactedColliders, ref IInteractableBody[] targets) {
            
            if (grabbedTarget != null) {
                targets = new IInteractableBody[] { grabbedTarget };
                return;
            }

            var targetsList = new List<IInteractableBody>();

            foreach (var col in interactedColliders) {

                var interactable = col.GetComponent<IInteractable>();

                var interactableBody = interactable?.GetInteractable();

                if (interactableBody == null) {
                    continue;
                }

                targetsList.Add(interactableBody);
            }

            targets = targetsList.ToArray();
            return;
        }

        private void HitTargets(Character2DController attacker, ref Vector2 moveToPosition, ref IInteractableBody[] interactedColliders, Vector2 forceDirAction, ref int newFacingDirection, ref Definitions.AttackAbility attackAbility, ref AttackBaseStats attackProperties) {

            var canMultiHit = attackProperties.Properties.Count > 0 && attackProperties.Properties.Contains(Definitions.AttackProperty.MultiTargetOnSurfaceHit);

            foreach (var col in interactedColliders) {

                AttackStats attackStats = new AttackStats(attacker.Stats.Damage,
                    interactedColliders.Length < 2 ? forceDirAction : GetSingleHitForceDir(attacker.Stats.PushbackForce, col.GetCenterPosition(), moveToPosition));
               
                var validAttack = TryPerformCombat(attacker, ref attackAbility, ref attackProperties, moveToPosition, col, attackStats);
                
                if (validAttack != 0 && newFacingDirection == 0) {
                    
                    newFacingDirection = validAttack;
                    moveToPosition = !col.IsGrabbed() ? col.GetCenterPosition() : moveToPosition;

                    if (!canMultiHit) {
                        break;
                    }
                } 
            }
        }

        private static Vector2 GetSingleHitForceDir(float pushbackForce, Vector2 startPosition, Vector2 endPosition) {
            return (startPosition - endPosition).normalized * pushbackForce;
        }

        private int TryPerformCombat(Character2DController attacker, ref Definitions.AttackAbility attackAbility, ref AttackBaseStats attackProperties, Vector2 moveToPosition, IInteractableBody interactedCollider, AttackStats attackStats) {

            var facingRightOrLeftTowardsPoint = 0;

            if (interactedCollider == null) {
                return facingRightOrLeftTowardsPoint;
            }

            var foe = interactedCollider;
            var foeState = foe.GetCurrentState();
            
            // todo: ValidateOpponentConditions of health and change the moveToPosition accordingly in case a foe is already dead
            var isAttackAction = _logic.AbilitiesLogic.IsAbilityAnAttack(attackAbility);
            var foeValidState = !isAttackAction || isAttackAction && ValidateOpponentConditions(attackProperties, foe);
            if (!foeValidState) {
                Debug.LogWarning($"Invalid foe state {foe.GetTransform().gameObject.name}");
                return facingRightOrLeftTowardsPoint;
            }
            var attackAvailable = foeValidState;
            if (attackAvailable) {

                facingRightOrLeftTowardsPoint = foe.GetCenterPosition().x > attacker.LegsPosition.x ? 1 : -1;
                
                var properties = attackProperties.Properties;
                var isFoeBlocking = foeState == Definitions.CharacterState.Blocking;

                if (isAttackAction) {
                    
                    if (properties.Contains(Definitions.AttackProperty.PushBackOnHit) ||
                        properties.Contains(Definitions.AttackProperty.PushBackOnBlock) && isFoeBlocking) {

                        ApplyForceOnFoeWithDelay(foe, attackStats, (attacker.Stats.AttackDurationInMilliseconds) );

                    } else if (properties.Contains(Definitions.AttackProperty.PushUpOnHit) ||
                                properties.Contains(Definitions.AttackProperty.PushUpOnBlock) && isFoeBlocking) {
                        
                        if ((attackStats.ForceDir.normalized.y) < 0.75f) { // 0.75f is the threshold to account is angle that is not 45 angle
                            Debug.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.yellow, 4);
                            attackStats.ForceDir.y = attacker.Stats.PushbackForce;
                        }

                        attackStats.ForceDir.x *= 0.05f;

                        ApplyForceOnFoeWithDelay(foe, attackStats, (attacker.Stats.AttackDurationInMilliseconds));

                    } else if (properties.Contains(Definitions.AttackProperty.PushDiagonalOnHit) ||
                                properties.Contains(Definitions.AttackProperty.PushDiagonalOnBlock) && isFoeBlocking) {
                    
                        //Debug.Log($"forceDirAction.y {forceDirAction.ForceDir.normalized.y} during PushDiagonal");

                        if ((attackStats.ForceDir.normalized.y) < 0.1f) {
                            Debug.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.yellow, 4);
                            attackStats.ForceDir.y = attacker.Stats.PushbackForce;
                        }
                        attackStats.ForceDir.x *= 0.5f;

                        ApplyForceOnFoeWithDelay(foe, attackStats, (attacker.Stats.AttackDurationInMilliseconds));

                    }
                        
                } else if (_logic.AbilitiesLogic.IsAbilityAGrab(attackAbility)) {

                    //var abovePosition = new Vector3(moveToPosition.x, moveToPosition.y + attacker.LegsRadius, 0);
                    var isGroundSmash = properties.Contains(Definitions.AttackProperty.GrabToGroundSmash) ||
                        foe.GetCurrentState() is Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling;
                    var grabPointOffset = (isGroundSmash) ? Vector2.down : Vector2.up;
                    var endPosition = moveToPosition + grabPointOffset * (attacker.LegsRadius);
                    var attackerAsInteractable = (IInteractableBody)attacker;

                    var isThrowingGrabbedFoe = attackerAsInteractable.IsGrabbing() && foe.IsGrabbed();
                    if (isThrowingGrabbedFoe) {

                        AlterThrowDirBasedOnNavigationDirection(ref attackStats.ForceDir, attacker.State.NavigationDir, attacker.Stats.PushbackForce);
                        ApplyGrabOnFoeWithDelay(foe, null, foe.GetTransform().position);
                        ApplyForceOnFoeWithDelay(foe, attackStats, attacker.Stats.AttackDurationInMilliseconds);
                        attackerAsInteractable.SetAsGrabbing(null);
                        ApplyProjectileStateWhenThrown(foe, attackStats.Damage, attackerAsInteractable);

                    } else {
                        
                        foe.SetTargetPosition(endPosition);

                        _cam.RemoveFromTargetList(foe.GetTransform());
                        ApplyGrabOnFoeWithDelay(foe, attacker, endPosition, attacker.Stats.AttackDurationInMilliseconds);
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

        private async void ApplyProjectileStateWhenThrown(IInteractableBody grabbed, int damage, IInteractableBody thrower) {

            Debug.Log("trying as damager -> damage: " + damage);

            var _bodyDamager = (grabbed as Character2DController);

            if (damage == 0 || _bodyDamager == null) {
                return;
            }

            _bodyDamager.SetProjectileState(true);

            while (thrower != null && (grabbed.GetVelocity().magnitude < 1 || grabbed.IsGrabbed()) ) {
                await Task.Delay(TimeSpan.FromMilliseconds(20));
            }
            if (thrower == null) {
                return;
            }

            var grabbedVelocity = grabbed.GetVelocity();
            Debug.Log("Started as damager -> Mag: " + grabbedVelocity.magnitude);

            var damagees = new List<Collider2D>() { thrower.GetCollider() };

            var legsPosition = grabbed.GetCenterPosition();
            var grabbedColl = grabbed.GetCollider();
            var legsRadius = ((CircleCollider2D)grabbedColl).radius;

            while (_gameRunning && grabbedVelocity.magnitude > 1) {

                //Debug.Log("as damager -> Mag: " + grabbedVelocity.magnitude);
                legsPosition = grabbed.GetCenterPosition();

                var solidOutRadiusObjectsColliders =
                Physics2D.OverlapCircleAll(legsPosition, legsRadius + 0.25f, _whatIsDamageableCharacter).
                    Where(collz => collz.gameObject != grabbedColl.gameObject && collz.gameObject != thrower.GetTransform().gameObject && !damagees.Contains(collz)).ToArray();

                foreach (var interactable in solidOutRadiusObjectsColliders)
                {

                    var damagee = interactable.GetComponent<IInteractable>();
                    if (damagee == null)
                    {
                        continue;
                    }

                    var interactableBody = damagee.GetInteractable();
                    if (interactableBody != null && !damagees.Contains(interactable)) {

                        damagees.Add(interactable);

                        var closestFoePosition = interactable.ClosestPoint(legsPosition);
                        var dir = ((Vector3)closestFoePosition - legsPosition).normalized;
                        var directionTowardsFoe = (closestFoePosition.x > legsPosition.x) ? 1 : -1;
                        dir.x = directionTowardsFoe;
                        var newPositionToSetOnFixedUpdate = closestFoePosition + (Vector2)dir.normalized * -(legsRadius);
                        interactableBody.ApplyForce(dir * grabbedVelocity * 0.7f); // 0.7f is an impact damage decrease
                        // todo: damage decrease based on velocity?
                        Debug.Log($"damage on {interactableBody.GetTransform().gameObject.name} by {thrower.GetTransform().gameObject.name}");
                        interactableBody.DealDamage(damage);
                        Debug.DrawLine(closestFoePosition, newPositionToSetOnFixedUpdate, Color.white, 3);
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(10));

                grabbedVelocity = grabbed.GetVelocity();
            }

            _bodyDamager.SetProjectileState(false);
        }

        private bool ValidateAttackConditions(AttackBaseStats attack, Character2DState characterState) {

            var attackerState = characterState.CurrentState;
            if (attack.Ability == Definitions.AttackAbility.RollAttack) { Debug.Log($"roll attack data: {characterState.Velocity.magnitude}"); }

            foreach (var condition in attack.CharacterStateConditions) {

                switch (condition) {
                    case Definitions.CharacterState.Running when !_logic.GameplayLogic.IsStateConsideredAsRunning(attackerState, characterState.Velocity.magnitude):
                    case Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling when !(_logic.GameplayLogic.IsStateConsideredAsAerial(attackerState)):
                    case Definitions.CharacterState.Grounded when !_logic.GameplayLogic.IsStateConsideredAsGrounded(attackerState):
                    case Definitions.CharacterState.Crouching when attackerState != Definitions.CharacterState.Crouching:
                    case Definitions.CharacterState.StandingUp when attackerState != Definitions.CharacterState.StandingUp:
                    case Definitions.CharacterState.Blocking when attackerState != Definitions.CharacterState.Blocking:
                    case Definitions.CharacterState.Grabbing when attackerState != Definitions.CharacterState.Grabbing:
                        return false;
                }

            }
            
            return true;
        }

        private bool ValidateAlternativeAttackConditions(ref Definitions.AttackAbility attackAbility, ref AttackBaseStats attack, Character2DState characterState) {

            var state = characterState.CurrentState;
            Debug.Log($"checking matching alternative Attack for {attackAbility}");
            foreach (var condition in attack.CharacterStateConditions) {
                //Debug.Log($"checking alt condition {condition} is considered ? {characterState.Velocity.magnitude}");
                switch (condition) {

                    case Definitions.CharacterState.Running when !_logic.GameplayLogic.IsStateConsideredAsRunning(characterState.CurrentState, characterState.Velocity.magnitude):
                        attackAbility = Definitions.AttackAbility.LightAttackAlsoDefaultAttack;
                        return true;

                    case var _ when condition is Definitions.CharacterState.StandingUp or Definitions.CharacterState.Crouching && _logic.GameplayLogic.IsStateConsideredAsAerial(characterState.CurrentState):
                        if (attackAbility is Definitions.AttackAbility.MediumAttack or Definitions.AttackAbility.HeavyAttack) {
                            attackAbility = Definitions.AttackAbility.AerialAttack;
                            return true;

                        } else if (attackAbility is Definitions.AttackAbility.MediumGrab or Definitions.AttackAbility.HeavyGrab) {
                            attackAbility = Definitions.AttackAbility.AerialGrab;
                            return true;
                        }
                        break;
                }

            }

            return false;
        }

        private bool ValidateOpponentConditions(AttackBaseStats attack, IInteractableBody interactable) {

            var opponentState = interactable.GetCurrentState();
            
            foreach (var condition in attack.OpponentStateConditions) {
                
                switch (condition) {
                    // todo: change this to check when a player is grounded but is considered flying type, so the state of the opponent is grounded when in midair and not falling
                    /*case Definitions.CharacterState.Falling or Definitions.CharacterState.Jumping or Definitions.CharacterState.Grounded
                        when opponentState is (Definitions.CharacterState.Falling or Definitions.CharacterState.Jumping or Definitions.CharacterState.Grounded) :
                        return true;*/
                    
                    case Definitions.CharacterState.Alive when interactable.GetHpPercent() == 0:
                    case Definitions.CharacterState.Running when opponentState != Definitions.CharacterState.Running:
                    case Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling when !(_logic.GameplayLogic.IsStateConsideredAsAerial(opponentState)):
                    case Definitions.CharacterState.Grounded when !_logic.GameplayLogic.IsStateConsideredAsGrounded(opponentState) && !interactable.IsGrabbed():
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

            var ifDamageableIsGrabbedThenIgnoreAddToTargetsList = damageableBody.IsGrabbed();
            if (!ifDamageableIsGrabbedThenIgnoreAddToTargetsList) {
                _cam.AddToTargetList(damageableBody.GetTransform());
            }
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

            damageableBody.DealDamage(attackStats.Damage);
            damageableBody.ApplyForce(attackStats.ForceDir);
            
            Debug.DrawRay(damageableBody.GetCenterPosition(), attackStats.ForceDir, Color.red, 4);

            var distanceFromAttacker = damageableBody.GetCenterPosition() - _hero.LegsPosition;
            while (Mathf.Abs(distanceFromAttacker.x) < 15 && Mathf.Abs(distanceFromAttacker.y) < 8
                 && (_hero.Velocity.magnitude <= 20 || _hero.Velocity.magnitude > 20 && distanceFromAttacker.magnitude > 200 && Mathf.Sign(_hero.Velocity.y) == Mathf.Sign(distanceFromAttacker.y)) ) {
                await Task.Delay((500));
                distanceFromAttacker = damageableBody.GetCenterPosition() - _hero.LegsPosition;
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
            ApplyProjectileStateWhenThrown(grabbed, attackStats.Damage, grabber);
        }
        
    }

}
