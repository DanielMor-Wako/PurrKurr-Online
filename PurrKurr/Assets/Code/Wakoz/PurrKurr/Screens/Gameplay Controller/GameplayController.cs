using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Code.Wakoz.Utils.Extensions;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Ropes;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Projectiles;
using Code.Wakoz.PurrKurr.DataClasses.GamePlayUtils;
using Code.Wakoz.PurrKurr.Screens.CameraComponents;
using Code.Wakoz.PurrKurr.Screens.Init;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDetection;
using Code.Wakoz.PurrKurr.Screens.Effects;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;
using Code.Wakoz.PurrKurr.Screens.Levels;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller {

    [DefaultExecutionOrder(14)]
    public sealed class GameplayController : SingleController {
        
        private const float attackCooldownDuration = 0.25f;
        private const float _MinVelocityForFlip = 5;
        [SerializeField] private float _xDistanceFrommClingPoint = 0.8f;

        public event Action<ActionInput> OnTouchPadDown;
        public event Action<ActionInput> OnTouchPadClick;
        public event Action<ActionInput> OnTouchPadUp;
        public event Action<InteractableObject2DState> OnStateChanged;
        public event Action<List<DisplayedableStatData>> OnStatsChanged;

        [SerializeField] private CameraFollow _cam;
        [SerializeField] private Character2DController _mainHero;
        private Character2DController _hero;

        private bool _firstTimeInitialized = false;
        private bool _heroInitialized = false;
        private bool _gameRunning = false;

        private LogicController _logic;
        private InputController _input;
        private InputInterpreterLogic _inputInterpreterLogic; 
        private UIController _ui;
        private GameplayLogic _gameplayLogic;
        private GamePlayUtils _gameplayUtils;
        private EffectsController _effects;
        private LevelsController _levelsController;
        private InteractablesController _interactables;
        private DebugController _debug;

        private LayerMask _whatIsSolid;
        private LayerMask _whatIsPlatform;
        private LayerMask _whatIsClingable;
        private LayerMask _whatIsSurface;
        private LayerMask _whatIsCharacter;
        private LayerMask _whatIsDamageableCharacter;

        protected override void Clean() {

            DeregisterInputEvents();

            _gameRunning = false;

            if (_hero == null) {
                return;
            }
            _hero.OnStateChanged -= OnStateChanged;
            _hero.OnUpdatedStats -= UpdateOrInitUiDisplayForCharacter;
        }

        protected override Task Initialize() {

            _cam ??= Camera.main.GetComponent<CameraFollow>();
            
            _input = GetController<InputController>();
            _logic = GetController<LogicController>();
            _inputInterpreterLogic = new InputInterpreterLogic(_logic.InputLogic, _logic.GameplayLogic);
            _gameplayLogic = _logic.GameplayLogic;
            _levelsController = GetController<LevelsController>();
            _interactables = GetController<InteractablesController>();
            _debug = GetController<DebugController>();

            _ui ??= GetController<UIController>();
            if (_ui.TryBindToCharacterController(this)) {
                RegisterInputEvents();
            } else {
                _ui = null;
                _debug.LogWarning("Something went wrong, there is no active ref to PadsDisplayController");
            }
            
            _whatIsSurface = _gameplayLogic.GetSurfaces();
            _whatIsSolid = _gameplayLogic.GetSolidSurfaces();
            _whatIsPlatform = _gameplayLogic.GetPlatformSurfaces();
            _whatIsClingable = _gameplayLogic.GetClingableSurfaces();
            _whatIsCharacter = _gameplayLogic.GetDamageables();
            _whatIsDamageableCharacter = _gameplayLogic.GetDamageables();

            InitLevel();

            TryInitHero(_mainHero);

            _levelsController.RefreshSpritesOrder(_hero);

            _gameRunning = true;

            return Task.CompletedTask;
        }

        private void InitLevel() {

            _levelsController.InitInteractablePools(_interactables);

            LoadLevel(0);
        }

        public void OnCharacterNearDoor(DoorController door, Collider2D triggeredCollider) {

            var x = triggeredCollider.GetComponent<IInteractable>();
            var character = x.GetInteractable();
            
            if (character != (IInteractableBody)_hero) {
                return;
            }

            if (_logic.InputLogic.IsNavigationDirValidAsUp(_hero.State.NavigationDir)) {
                LoadLevel(door.GetRoomIndex());
            }

        }

        public void LoadLevel(int levelToLoad) {

            _levelsController.LoadLevel(levelToLoad);
        }

        private void ReviveAllHeroes() {

            var _heroes = _interactables._heroes;

            if (_heroes == null) {
                return;
            }

            foreach (var character in _heroes) {
                
                if (character == null || character == _hero) {
                    continue;
                }
                
                character.Revive();
                _levelsController.RefreshSpriteOrder(character, character == _hero);
            }

            _hero?.SetLevel(_hero.Stats.GetCurrentLevel());
        }

        [ContextMenu("Init Referenced Hero")]
        public void SetHeroAsReferenced() {
            TryInitHero(_mainHero);
            _ui.InitUiForCharacter(_mainHero);
            _levelsController.RefreshSpritesOrder(_hero);
        }

        public void SetNewHero(Character2DController hero) => TryInitHero(hero);

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
                _levelsController.RefreshSpritesOrder(_hero);
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
        public void SetHeroLevel() => _hero.SetLevel(Mathf.FloorToInt(_hero.Stats.MaxLevel * 0.5f));
        public void SetHeroMaxLevel() => _hero.SetMaxLevel();
        public void ReviveHeroes() => ReviveAllHeroes();
        public void RandomizeHero() {

            var _heroes = _interactables._heroes;

            if (_heroes == null) {
                return;
            }

            var newHero = HelperFunctions.Random(_heroes);
            if (newHero == null) {
                return;
            }

            _mainHero = newHero;
            SetHeroAsReferenced();
        }
        
        private void UpdateStatsOnLevelUp(int level) {
            if (_hero == null) {
                return;
            }
            _hero.Stats.UpdateStats(level);
        }

        private void RegisterInputEvents() {

            if (_input == null) {
                _debug.LogError("Touch display has no available events for input");
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
            
            if (CanPerformActionAlive()) {
                
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
                    
                    if (isActionPerformed) {

                        if (interactedColliders != null && !_hero.State.IsGrabbed()) {
                            CombatLogic(_hero, actionInput.ActionType, moveToPosition, interactedColliders, forceDir);

                        } else if (interactedColliders != null && _hero.State.IsGrabbed() &&
                            actionInput.ActionType is ActionType.Grab or ActionType.Attack) {
                            if (actionInput.ActionType is ActionType.Grab) {
                                DisconnectFromRope(_hero);
                            } else {// if (actionInput.ActionType is ActionType.Attack) {
                                CutRopeLink(_hero);
                            }

                        } else if (actionInput.ActionType is ActionType.Special) {
                            ApplySpecialAction(_hero, true);

                        } else if (actionInput.ActionType is ActionType.Rope or ActionType.Projectile) {
                            ApplyAimingAction(_hero, actionInput);

                        } else if (actionInput.ActionType is ActionType.Jump && _hero.State.CurrentState == ObjectState.Crouching) {
                            
                            if (HelperFunctions.IsObjectInLayerMask(_hero.GetSurfaceCollLayer(), ref _whatIsPlatform)) {
                                _hero.TryGetDodgeDirection(-Vector2.up * 2, ref moveToPosition);
                                _hero.SetTargetPosition(moveToPosition);
                            } else {
                                ApplyAimingAction(_hero, actionInput);
                            }

                        } else if (actionInput.ActionType == ActionType.Jump && forceDir != Vector2.zero) {
                            // todo: flip horizontal when player infront of wall?
                            var flipHorizontalDirectionWhileClinging = false ;// _hero.State.IsClinging() && _hero.State.IsFrontWall();
                            DisconnectFromAnyGrabbing(_hero);
                            _hero.SetJumping(Time.time + .2f);
                            AlterJumpDirByStateNavigationDirection(ref forceDir, _hero.State, flipHorizontalDirectionWhileClinging);
                            _hero.SetForceDir(forceDir);
                            _hero.DoMove(0); // might conflict with the TryPerformInputNavigation when the moveSpeed is already set by Navigation
                            ApplyEffectForDurationAndSetRotation(_hero, Effect2DType.DustCloud, _hero.State.ReturnForwardDirByTerrainQuaternion());

                        } else if (actionInput.ActionType == ActionType.Block) {
                            ApplyEffectForDuration(_hero, Effect2DType.BlockActive);
                        }

                        _hero.State.SetActiveCombatAbility(actionInput.ActionType);
                    }

                }

            } else {

                _hero.DoMove(0);
            }
            
            OnTouchPadDown?.Invoke(actionInput);
        }

        private void OnActionOngoing(ActionInput actionInput) {
            
            if (CanPerformActionAlive()) {

                if (_inputInterpreterLogic.TryPerformInputNavigation(actionInput, false, false, _hero, 
                        out var moveSpeed, out var forceDirNavigation, out var navigationDir)) {
                    
                    _hero.DoMove(moveSpeed);
                    _hero.SetForceDir(forceDirNavigation);
                    _hero.SetNavigationDir(navigationDir);
                    if (!_hero.State.IsMoveAnimation() && navigationDir != NavigationType.None) {
                        
                        bool? facingRight = _logic.InputLogic.IsNavigationDirValidAsRight(navigationDir) ? true :
                            _logic.InputLogic.IsNavigationDirValidAsLeft(navigationDir) ? false : null;

                        bool isNotJumpingState = _hero.State.CurrentState != ObjectState.Jumping;
                        bool isRunState = _hero.State.CurrentState == ObjectState.Running;

                        if (facingRight != null && isNotJumpingState && (!isRunState && Mathf.Abs(_hero.Velocity.x) > _MinVelocityForFlip )) {
                            // check if player is not facing a wall while running so momentum sustains
                            _hero.FlipCharacterTowardsPoint(facingRight == true);
                        }

                        // interact with rope link
                        if (_hero.IsGrabbed() && _charactersRopes.Count > 0 && !_hero.State.IsAiming()) {
                            var rope = _charactersRopes.FirstOrDefault();
                            var grabbedInteractable = _hero.GetGrabbedTarget();
                            var ropeLink = grabbedInteractable as RopeLinkController;
                            if (ropeLink != null) {
                                if (navigationDir is NavigationType.Up or NavigationType.Down) {

                                    rope.HandleMoveToNextLink(ropeLink, _hero, navigationDir is NavigationType.Up);

                                } else if (navigationDir is not (NavigationType.None)) {
                                    var isNavRightByFacingRight = facingRight != null ? facingRight == true ? true : false : false;

                                    rope.HandleSwingMommentum(ropeLink, isNavRightByFacingRight); 
                                    //ropeLink.TryPerformInteraction(actionInput, navigationDir); //xxxxxxxxxxxxxxxx
                                }
                            }
                        } else if(_hero.State.CurrentState is not (ObjectState.RopeClinging or ObjectState.RopeClimbing)) {
                            
                            var collSurfaceLayer = _hero.GetSurfaceCollLayer();
                            if (collSurfaceLayer > -1 && HelperFunctions.IsObjectInLayerMask(collSurfaceLayer, ref _whatIsClingable)) {

                                var closestWall = _hero._solidColliders.FirstOrDefault();
                                if (closestWall != null) {
                                    ApplyClingingAction(_hero, closestWall);
                                }
                            
                            } else if (_hero.State.IsClinging() && !_hero.State.IsTouchingAnySurface() && _logic.InputLogic.IsNavigationDirValidAsDown(navigationDir)) {

                                var xOffsetFromWall = new Vector2(0, -0.2f);
                                var clingPointCloserToTopEdge = (Vector2)_hero.transform.position + xOffsetFromWall;
                                _hero.SetAsClinging(null, clingPointCloserToTopEdge);
                            }
                        }

                    }
                }

                /*if (_inputInterpreterLogic.TryPerformInputAction(actionInput, false, false, _hero,
                    out var isActionPerformed, out var forceDir, out var moveToPosition, out var interactedCollider)) {

                }*/
            }
            
            OnTouchPadClick?.Invoke(actionInput);
        }

        private void OnActionEnded(ActionInput actionInput) {

            if (_inputInterpreterLogic.TryPerformInputNavigation(actionInput, false, true, _hero,
                    out var moveSpeed, out Vector2 forceDirNavigation, out var navigationDir)) {

                _hero.DoMove(moveSpeed);
                _hero.SetForceDir(forceDirNavigation);
                _hero.SetNavigationDir(NavigationType.None);
            }

            if (_inputInterpreterLogic.TryPerformInputAction(actionInput, false, true, _hero,
                    out var isActionPerformed, out var forceDir, out var moveToPosition, out var interactedCollider)) {

                if (isActionPerformed) {

                    var isBlockingEnded = actionInput.ActionType is ActionType.Block && _hero.State.CurrentState is ObjectState.Blocking;
                    var isProjectilingEnded = actionInput.ActionType is ActionType.Projectile && _hero.State.CurrentState is ObjectState.AimingProjectile;
                    var isRopingEnded = actionInput.ActionType is ActionType.Rope && _hero.State.CurrentState is ObjectState.AimingRope;
                    var isJumpAimEnded = actionInput.ActionType is ActionType.Jump && _hero.State.CurrentState is ObjectState.AimingJump;
                    var isSpecial = actionInput.ActionType is ActionType.Special && _hero.State.CurrentState is ObjectState.InterruptibleAnimation;

                    if (isBlockingEnded || isProjectilingEnded || isRopingEnded || isJumpAimEnded || isSpecial) {

                        _hero.State.SetActiveCombatAbility(ActionType.Empty);

                        if (isSpecial) {
                            ApplySpecialAction(_hero, false);

                        } else if (actionInput.NormalizedDirection != Vector2.zero) {

                            if (isBlockingEnded) {
                                ApplyEffectForDuration(_hero, Effect2DType.DodgeActive);
                                ApplyEffectForDurationAndSetRotation(_hero, Effect2DType.DustCloud, _hero.State.ReturnForwardDirByTerrainQuaternion());

                            } else {

                                Vector2 newPos = Vector2.zero;
                                Quaternion rotation = Quaternion.identity;
                                float distancePercentReached = 1f;

                                if (isProjectilingEnded) {
                                    ShootProjectile(_hero, actionInput, ref newPos, ref rotation, ref distancePercentReached);

                                } else if (isRopingEnded) {
                                    ShootRope(_hero, actionInput, newPos, rotation, distancePercentReached);
                                    
                                } else if (isJumpAimEnded) {
                                    // apply jump aim in trajectory dir
                                    _hero.SetJumping(Time.time + .2f);
                                    forceDir = actionInput.NormalizedDirection * _hero.Stats.JumpForce;
                                    ApplyEffectForDurationAndSetRotation(_hero, Effect2DType.DustCloud, _hero.State.ReturnForwardDirByTerrainQuaternion());
                                    //_hero.DoMove(0); // dont think its needed 
                                }
                            }
                        }

                        if (isProjectilingEnded || isRopingEnded || isJumpAimEnded) {
                            _hero.State.StopAiming();
                        }
                    }
                }

                if (forceDir != Vector2.zero) {
                    // might conflict with the DiagnosePlayerInputNavigation when the forceDir is already set by Navigation
                    _hero.SetForceDir(forceDir, true);
                }

                _hero.SetTargetPosition(moveToPosition);

            }

            OnTouchPadUp?.Invoke(actionInput);
        }

        private void ShootProjectile(Character2DController character, ActionInput actionInput, ref Vector2 newPos, ref Quaternion rotation, ref float distancePercentReached) {

            character.TryGetProjectileDirection(actionInput.NormalizedDirection, ref newPos, ref rotation, ref distancePercentReached);

            var projectile = _interactables.GetInstance<ProjectileController>();
            if (projectile == null) {
                return;
            }

            projectile.transform.SetPositionAndRotation(character.LegsPosition, rotation);
            projectile.SetTargetPosition(newPos, distancePercentReached);
            ApplyEffectForDuration(character, Effect2DType.DustCloud);
            //ApplyEffectForDuration(character, Effect2DType.TrailInAir); // todo: other trails for fire ice etc
            ApplyProjectileStateWhenThrown(projectile, character.Stats.Damage, character, () => _interactables.ReleaseInstance(projectile));

        }

        private List<RopeController> _charactersRopes = new();

        private async void ShootRope(Character2DController character, ActionInput actionInput, Vector2 newPos, Quaternion rotation, float distancePercentReached) {

            var hasNoHit = !character.TryGetRopeDirection(actionInput.NormalizedDirection, ref newPos, ref rotation, out var cursorPosition, ref distancePercentReached);

            if (hasNoHit) {
                return;
            }

            var rope = _interactables.GetInstance<RopeController>();
            if (rope == null) {
                _debug.LogWarning("No available rope instance in pool, consider increasing the max items capacity");
                if (_charactersRopes.Count > 0) {
                    rope = _charactersRopes.FirstOrDefault();
                }
            }

            var ropeData = new RopeData(rope.gameObject, new Vector2[2] { newPos, character.LegsPosition }, _whatIsSolid);

            await rope.Initialize(ropeData, character);

            var lastLink = rope.GetLastChainedLink();
            //character.SetTargetPosition(lastLink.GetCenterPosition(), 1);
            rope.HandleConnectInteractableBody(lastLink, character);
            Debug.Log($"Connect a body to last link {lastLink.name}");

            if (!_charactersRopes.Contains(rope)) {
                _charactersRopes.Add(rope);
            }

            //ApplyEffectForDuration(character, Effect2DType.DustCloud);
        }

        private void ApplyClingingAction(Character2DController character, Collider2D coll) {

            /*if (coll is not EdgeCollider2D) {
                return;
            }*/

            // 0.55f is the default hingeJoint2D y offset of the Anchor, the climbSpeed determines the offset when up or down
            var climbSpeed = 0.1f;

            var navType = character.State.NavigationDir;
            var yMovementByNavigation = navType is NavigationType.Up ? climbSpeed : navType is NavigationType.Down ? -climbSpeed : 0;

            var reachedWallBottom = yMovementByNavigation < 0 && character.State.IsCeiling();
            if (reachedWallBottom) {
                character.SetAsClinging(null, character.transform.position);
                return;
            }

            var anchorDistanceFromLegsPosition = (character.transform.position.y - character.LegsPosition.y);
            if (anchorDistanceFromLegsPosition > 0 && navType is NavigationType.Up) {
                _debug.Log($"anchor too far from legs position, climb up ignored {anchorDistanceFromLegsPosition} > 0");
                return;
            }

            var edges = coll ;// as EdgeCollider2D;
            
            var legsPos = character.transform.position;
            legsPos.y += yMovementByNavigation; 
            var edgesClosestPoint = edges.ClosestPoint(legsPos);
            var wallPoint = edgesClosestPoint;
            wallPoint.y = legsPos.y;
            var isWallPositionedRightToThePlayer = wallPoint.x > legsPos.x;

            var isNavTowardsWall = _logic.InputLogic.IsNavigationDirValidAsRight(character.State.NavigationDir) && isWallPositionedRightToThePlayer;
            if (!isNavTowardsWall) {
                isNavTowardsWall = _logic.InputLogic.IsNavigationDirValidAsLeft(character.State.NavigationDir) && !isWallPositionedRightToThePlayer;
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
            wallPoint += xHorizontalOffsetFromWall * character.LegsRadius * _xDistanceFrommClingPoint;
            character.SetAsClinging(edges, wallPoint);

            _debug.DrawLine(edgesClosestPoint, wallPoint, Color.magenta, 1f);
        }

        private void CutRopeLink(IInteractableBody character) {

            var grabbedInteractable = character.GetGrabbedTarget();
            var ropeLink = grabbedInteractable as RopeLinkController;
            if (ropeLink == null) {
                return;
            }

            var rope = _charactersRopes.FirstOrDefault();
            var nextLink = rope?.GetNextChainedLink(ropeLink, 1);
            if (nextLink != null) {
                nextLink.DealDamage(1); // updating the rope is called thru the link event
            }
        }

        private void DisconnectFromRope(IInteractableBody character) {

            var grabbedInteractable = character.GetGrabbedTarget();
            var ropeLink = grabbedInteractable as RopeLinkController;
            if (ropeLink == null) {
                return;
            }

            var rope = _charactersRopes.FirstOrDefault();
            rope.HandleDisconnectInteractableBody(ropeLink, character);
        }

        private void DisconnectFromAnyGrabbing(Character2DController character) {

            DisconnectFromRope(character);
            DisconnectAnyGrabbing(character);
        }

        private void DisconnectAnyGrabbing(Character2DController character) {
            
            //if (character.State.GetClingableObject() != null) { }
            character.SetAsClinging(null, character.LegsPosition);
        }

        private bool CanPerformActionAlive() => _hero.State.CanPerformAction() && _hero.Stats.GetHealthPercentage() > 0;

        private void FaceCharacterTowardsPointByNavigationDir(NavigationType navigationDir) {

            if (_logic.InputLogic.IsNavigationDirValidAsRight(navigationDir)) {
                _hero.FlipCharacterTowardsPoint(true);
            } else if (_logic.InputLogic.IsNavigationDirValidAsLeft(navigationDir)) {
                _hero.FlipCharacterTowardsPoint(false);
            }
        }

        private void AlterJumpDirByStateNavigationDirection(ref Vector2 forceDir, InteractableObject2DState state, bool flipJumpHorizontalWhenClinging) {

            var navigationDir = state.NavigationDir;
            var horizontalDir = _hero.State.GetFacingRightAsInt() * (flipJumpHorizontalWhenClinging ? -1 : 1);

            //_debug.DrawRay(_hero.LegsPosition, forceDir, Color.white, 3);
            if (navigationDir == NavigationType.Up || _hero.State.IsFrontWall()) {
                forceDir = HelperFunctions.RotateVector(forceDir, horizontalDir * -5);

            } else {
                forceDir = HelperFunctions.RotateVector(forceDir, horizontalDir * -18);

            }
            _debug.DrawRay(_hero.LegsPosition, forceDir, Color.white, 3);
        }

        private void AlterThrowDirBasedOnNavigationDirection(ref Vector2 throwDir, NavigationType navigationDir, float pushbackForce, bool isAerial) {

            _debug.DrawRay(_hero.LegsPosition, throwDir, Color.gray, 3);
            if (!_logic.InputLogic.IsNavigationDirValidAsUp(navigationDir)) {
                throwDir = AlterDirectionToMaxForce(throwDir);
            }
            if (isAerial && _logic.InputLogic.IsNavigationDirValidAsDown(navigationDir)) {
                throwDir.y = -Mathf.Abs(throwDir.y);
                if (navigationDir == NavigationType.Down) {
                    throwDir.x *= .5f;
                }
            }
            _debug.DrawRay(_hero.LegsPosition, throwDir, Color.magenta, 3);
        }

/*
        private void Update() {

            
        }

        private void FixedUpdate() {

            //DoTick();

        }*/

        public void SetCameraScreenShake() => _cam.ShakeScreen(1, 0.5f);

        private void CombatLogic(Character2DController attacker, ActionType actionType, Vector2 moveToPosition, Collider2D[] interactedColliders, Vector2 forceDirAction) {

            var newFacingDirection = 0;
            var attackAbility = _logic.AbilitiesLogic.GetAttackAbility(attacker.GetNavigationDir(), actionType, attacker.State.CurrentState);

            if (attacker.Stats.TryGetAttack(ref attackAbility, out var attackProperties)) {

                ValidateAttackCondition(ref attacker, ref attackAbility, ref attackProperties);
                GetAvailableTargets(ref attacker, ref attackProperties, moveToPosition, ref interactedColliders, out var interactableBodies);
                HitTargets(attacker, ref moveToPosition, ref interactableBodies, forceDirAction, ref newFacingDirection, ref attackAbility, ref attackProperties);
            
            } else {
                _debug.LogWarning($"Attack Ability {attackAbility} is missing from attack moves");
            }

            var isNewFacingDirectionSetAndCharacterIsFacingAnything = newFacingDirection != 0;
            if (isNewFacingDirectionSetAndCharacterIsFacingAnything) {

                attacker.FlipCharacterTowardsPoint(newFacingDirection == 1);
                attacker.SetTargetPosition(moveToPosition);
            }

        }

        private void ValidateAttackCondition(ref Character2DController attacker, ref AttackAbility attackAbility, ref AttackBaseStats attackProperties) { 
            
            var attackAvailable = ValidateAttackConditions(attackProperties, attacker.State);

            if (!attackAvailable) {

                if (ValidateAlternativeAttackConditions(ref attackAbility, ref attackProperties, attacker.State)) {
                    attackAvailable = attacker.Stats.TryGetAttack(ref attackAbility, out var alternativeAttackProperties);
                    if (attackAvailable) {
                        attackProperties = alternativeAttackProperties;
                    }

                } else {
                    _debug.Log($"no alternative attack for {attackAbility}");
                }
            }

        }

        private void GetAvailableTargets(ref Character2DController attacker, ref AttackBaseStats attackProperties, Vector2 moveToPosition, ref Collider2D[] interactedColliders, out IInteractableBody[] interactableBodies) {

            interactableBodies = new IInteractableBody[] { };
            SetSingleOrMultipleTargets(attacker, ref interactedColliders, ref interactableBodies );
            
            var canAttackMultiTarget = attackProperties.Properties.Count > 0 && attackProperties.Properties.Contains(AttackProperty.MultiTargetOnSurfaceHit);
            if (canAttackMultiTarget && interactedColliders.Length > 1) {
                attacker.FilterNearbyCharactersAroundHitPointByDistance(ref interactableBodies, moveToPosition, attacker.Stats.MultiTargetsDistance);
            }

        }

        private void SetSingleOrMultipleTargets(Character2DController attacker, ref Collider2D[] interactedColliders, ref IInteractableBody[] targets) {
            
            var grabbedTarget = attacker.GetGrabbedTarget();
            if (grabbedTarget != null) {
                targets = new IInteractableBody[] { grabbedTarget };
                return;
            }

            var latestInteraction = attacker.State.GetLatestInteraction();
            var latestInteractionAvailable = latestInteraction != null;
            bool latestInteractionIsInTargetsList = false;

            var targetsList = new List<IInteractableBody>();

            foreach (var col in interactedColliders) {

                var interactable = col.GetComponent<IInteractable>();

                var interactableBody = interactable?.GetInteractable();

                if (interactableBody == null) {
                    continue;
                }
                
                latestInteractionIsInTargetsList = latestInteractionIsInTargetsList ? true : 
                    latestInteractionAvailable && interactableBody == latestInteraction;

                targetsList.Add(interactableBody);
            }
            
            if (latestInteractionAvailable && latestInteractionIsInTargetsList ) { //&&
                //targetsList.Take(2).Contains(latestInteraction)) {

                var interactionPositionedRight = attacker.LegsPosition.x < latestInteraction.GetCenterPosition().x;
                if (interactionPositionedRight && _logic.InputLogic.IsNavigationDirValidAsRight(attacker.State.NavigationDir) ||
                    !interactionPositionedRight && _logic.InputLogic.IsNavigationDirValidAsLeft(attacker.State.NavigationDir) ||
                    attacker.State.IsFacingRight() == interactionPositionedRight) {

                    targetsList = targetsList.OrderBy(item => item != latestInteraction).ToList();

                }
            }

            targets = targetsList.ToArray();
            return;
        }

        private void HitTargets(Character2DController attacker, ref Vector2 moveToPosition, ref IInteractableBody[] interactedColliders, Vector2 forceDirAction, ref int newFacingDirection, ref AttackAbility attackAbility, ref AttackBaseStats attackProperties) {

            var canMultiHit = attackProperties.Properties.Count > 0 && attackProperties.Properties.Contains(AttackProperty.MultiTargetOnSurfaceHit);
            IInteractableBody latestInteraction = null;

            foreach (var col in interactedColliders) {

                AttackData attackStats = new AttackData(attacker.Stats.Damage,
                    interactedColliders.Length < 2 ? forceDirAction : GetSingleHitForceDir(attacker.Stats.PushbackForce, col.GetCenterPosition(), moveToPosition));
               
                var validAttack = TryPerformCombat(attacker, ref attackAbility, ref attackProperties, moveToPosition, col, attackStats);
                
                if (validAttack != 0 && newFacingDirection == 0) {
                    
                    newFacingDirection = validAttack;
                    moveToPosition = !col.IsGrabbed() ? col.GetCenterPosition() : moveToPosition;
                    latestInteraction = col;

                    if (!canMultiHit) {
                        break;
                    }
                } 
            }

            if (latestInteraction != null) {

                attacker.State.SetInterruptibleAnimation(Time.time + attackCooldownDuration);

                if (latestInteraction.GetHpPercent() <= 0) {
                    latestInteraction = null;
                }
            }
            attacker.State.MarkLatestInteraction(latestInteraction);
            
        }

        private static Vector2 GetSingleHitForceDir(float pushbackForce, Vector2 startPosition, Vector2 endPosition) {
            return (startPosition - endPosition).normalized * pushbackForce;
        }

        private int TryPerformCombat(Character2DController attacker, ref AttackAbility attackAbility, ref AttackBaseStats attackProperties, Vector2 moveToPosition, IInteractableBody interactedCollider, AttackData attackStats) {

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
                _debug.LogWarning($"Invalid foe state {foe.GetTransform().gameObject.name}");
                return facingRightOrLeftTowardsPoint;
            }
            var attackAvailable = foeValidState;
            if (attackAvailable) {

                facingRightOrLeftTowardsPoint = foe.GetCenterPosition().x > attacker.LegsPosition.x ? 1 : -1;
                
                var properties = attackProperties.Properties;
                var isFoeBlocking = foeState == ObjectState.Blocking;

                if (isAttackAction) {

                    if (foe.GetCurrentState() is ObjectState.Jumping or ObjectState.Falling &&
                        (properties.Contains(AttackProperty.PushDownOnHit) ||
                        properties.Contains(AttackProperty.PushDownOnBlock) && isFoeBlocking)) {

                        attackStats.ForceDir.y = - Mathf.Abs(attackStats.ForceDir.y);
                        ApplyForceOnFoeWithDelay(foe, attackStats, (attacker.Stats.AttackDurationInMilliseconds) );

                        // todo: make the aerial attack turn into grab to groundsmash, and turn aerial grab into throwing the opponent
                        // ApplyProjectileStateWhenThrown(foe, attackStats.Damage, interactedCollider);

                    } else if (properties.Contains(AttackProperty.PushBackOnHit) ||
                        properties.Contains(AttackProperty.PushBackOnBlock) && isFoeBlocking) {

                        var foeCharacter = foe as Character2DController;
                        if (foeCharacter != null && ( _gameplayLogic.IsStateConsideredAsGrounded(foeState) || foeCharacter.State.CanMoveOnSurface()) ) {
                            _debug.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.cyan, 4);
                            var upwardDirByTerrain =
                                (foeCharacter.State.ReturnForwardDirByTerrainQuaternion()) * Quaternion.AngleAxis(180, new Vector3(0, 0, 1));
                            _debug.DrawRay(foe.GetCenterPosition(), upwardDirByTerrain * Vector3.up, Color.grey, 4);
                            // Convert the Vector2 to a Quaternion
                            Quaternion forceRotation = Quaternion.Euler(0, 0, Mathf.Atan2(attackStats.ForceDir.y, attackStats.ForceDir.x) * Mathf.Rad2Deg).normalized;

                            // Choose the interpolation method based on the dot product
                            float dotProduct = Quaternion.Dot(upwardDirByTerrain, forceRotation);
                            Quaternion averageDirBetweenGroundAndHit;
                            averageDirBetweenGroundAndHit = Quaternion.Slerp(upwardDirByTerrain, forceRotation, 0.5f);
                            // checking Quaternion for particular directions to validate if adding 180 degrees are needed, to correctly push foes based on groundDir and forceDir
                            _debug.LogWarning($"dotProduct {dotProduct}");
                            
                            bool facingAwayButNotPerpendicular = (dotProduct > -0.95f && dotProduct < -0.7f || dotProduct > -0.4f && dotProduct < -0.02f);//(dotProduct > -0.95f && dotProduct < -0.02f);
                            bool facingSimilarDirectionNotAligned = (dotProduct > 0.02f && dotProduct < 0.43f);
                            bool almostFacingAwayButNotPerpendicular = (dotProduct < 0 && dotProduct > -0.23f);
                            if (facingAwayButNotPerpendicular || facingSimilarDirectionNotAligned || almostFacingAwayButNotPerpendicular) {
                                averageDirBetweenGroundAndHit *= Quaternion.AngleAxis(180, new Vector3(0, 0, 1));
                                _debug.LogWarning($"{facingAwayButNotPerpendicular} || {facingSimilarDirectionNotAligned} || {almostFacingAwayButNotPerpendicular}");
                            }
                            attackStats.ForceDir = averageDirBetweenGroundAndHit.normalized * (Vector2.one * 0.5f * foeCharacter.Stats.PushbackForce);
                        }
                        ApplyForceOnFoeWithDelay(foe, attackStats, (attacker.Stats.AttackDurationInMilliseconds) );

                    } else if (properties.Contains(AttackProperty.PushUpOnHit) ||
                                properties.Contains(AttackProperty.PushUpOnBlock) && isFoeBlocking) {
                        
                        if ((attackStats.ForceDir.normalized.y) < 0.75f) { // 0.75f is the threshold to account is angle that is not 45 angle
                            _debug.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.yellow, 4);
                            attackStats.ForceDir.y = attacker.Stats.PushbackForce;
                        }

                        attackStats.ForceDir.x *= 0.05f;

                        ApplyForceOnFoeWithDelay(foe, attackStats, (attacker.Stats.AttackDurationInMilliseconds));

                    } else if (properties.Contains(AttackProperty.PushDiagonalOnHit) ||
                                properties.Contains(AttackProperty.PushDiagonalOnBlock) && isFoeBlocking) {
                    
                        //_visualDebug.Log($"forceDirAction.y {forceDirAction.ForceDir.normalized.y} during PushDiagonal");

                        if ((attackStats.ForceDir.normalized.y) < 0.1f) {
                            _debug.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.yellow, 4);
                            attackStats.ForceDir.y = attacker.Stats.PushbackForce;
                        }
                        attackStats.ForceDir.x *= 0.5f;

                        ApplyForceOnFoeWithDelay(foe, attackStats, (attacker.Stats.AttackDurationInMilliseconds));

                    }
                        
                } else if (_logic.AbilitiesLogic.IsAbilityAGrab(attackAbility)) {

                    var grabbingRopeLink = foe is RopeLinkController ? foe as RopeLinkController : null;

                    //var abovePosition = new Vector3(moveToPosition.x, moveToPosition.y + attacker.LegsRadius, 0);
                    var isGroundSmash = properties.Contains(AttackProperty.GrabToGroundSmash) ||
                        foe.GetCurrentState() is ObjectState.Jumping or ObjectState.Falling;
                    var grabPointOffset = (isGroundSmash) ? Vector2.down : Vector2.up;
                    var endPosition = moveToPosition + grabPointOffset * (attacker.LegsRadius);
                    var attackerAsInteractable = (IInteractableBody)attacker;

                    var isThrowingGrabbedFoe = attackerAsInteractable.IsGrabbing() && foe.IsGrabbed();
                    if (isThrowingGrabbedFoe) {

                        AlterThrowDirBasedOnNavigationDirection(ref attackStats.ForceDir, attacker.State.NavigationDir, attacker.Stats.PushbackForce, _gameplayLogic.IsStateConsideredAsAerial(attacker.State.CurrentState));
                        ApplyGrabOnFoeWithDelay(foe, null, foe.GetTransform().position);
                        ApplyForceOnFoeWithDelay(foe, attackStats, attacker.Stats.AttackDurationInMilliseconds);
                        attackerAsInteractable.SetAsGrabbing(null);
                        ApplyProjectileStateWhenThrown(foe, attackStats.Damage, attackerAsInteractable);

                    } else {
                        
                        foe.SetTargetPosition(endPosition);

                        _cam.RemoveFromTargetList(foe.GetTransform());
                        ApplyGrabOnFoeWithDelay(foe, attacker, endPosition, attacker.Stats.AttackDurationInMilliseconds);
                        attackerAsInteractable.SetAsGrabbing(foe);
                        _debug.DrawRay(moveToPosition, Vector2.up * 10, Color.grey, 4);
                        _debug.DrawRay(endPosition, Vector2.up * 10, Color.yellow, 4);
                        
                        if (grabbingRopeLink != null) {
                            
                            var rope = _charactersRopes.FirstOrDefault(); // Grabbing the closest rope link
                            rope?.HandleConnectInteractableBody(grabbingRopeLink, attacker);

                        } else if (isGroundSmash) {

                            ApplyGrabThrowWhenGrounded(foe, attacker, attackStats);

                        } else if (properties.Contains(AttackProperty.GrabAndPickUp)) {

                        } else {

                            var directionTowardsFoe = (endPosition.x >= attacker.LegsPosition.x) ? 1 : -1;

                            if (properties.Contains(AttackProperty.StunOnGrabber)) {

                                attackStats.ForceDir.x = -directionTowardsFoe * 0.3f * Mathf.Abs(attackStats.ForceDir.y);
                                ApplyGrabThrowWhenGrounded(foe, attacker, attackStats);
                            
                            } else {

                                attackStats.ForceDir.x = directionTowardsFoe * Mathf.Abs(attackStats.ForceDir.y);
                                attackStats.ForceDir.y *= 0.4f;
                                ApplyGrabThrowWhenGrounded(foe, attacker, attackStats);
                            }
                        }

                    }

                    if (properties.Contains(AttackProperty.PenetrateDodge)) {

                    }

                    if (properties.Contains(AttackProperty.StunResist)) {

                    }

                }

            }

            return facingRightOrLeftTowardsPoint;
        }

        private async void ApplySpecialAction(Character2DController character, bool isActive) {

            character.State.SetChargingSuper(isActive);

            if (!isActive) {
                return;
            }

            while (character != null && character.State.isChargingSuper() && character.CanPerformSuper()) {

                if (character.TryPerformSuper()) {
                    var superActionProperties = true;
                    PerformSuper(character, superActionProperties);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

        }

        // todo: make the function get properties and iterate on each one to generate a super, currently only gets true instead of property
        private void PerformSuper(Character2DController character, bool superActionProperties) {

            if (superActionProperties == true) {
                // revive 1 hp when character is not moving
                character.DealDamage(-1);
                ApplyEffectForDuration(character, Effect2DType.GainHp);
            }
        }

        private async void ApplyAimingAction(Character2DController character, ActionInput actionInput) {

            var actionType = actionInput.ActionType;

            var state = character.State;

            var isRope = actionType is ActionType.Rope;
            var isProjectile = actionType is ActionType.Projectile;
            var isJumpAim = actionType is ActionType.Jump;
            
            var validAimingAction = isRope || isProjectile || isJumpAim;

            if (!validAimingAction || validAimingAction && state.IsAiming()) {
                return;
            }

            _gameplayUtils ??= GetController<GamePlayUtils>();
            if (_gameplayUtils == null) {
                return;
            }

            state.SetAiming(10f);

            Vector2 newPos = Vector2.zero;
            float distancePercentReached = 1;
            Quaternion rotation = Quaternion.identity;
            Vector3[] linePoints = null;

            bool hasAimDir, hasHitData = false;
            
            while (state.IsAiming() && character != null ) {

                hasAimDir = actionInput.NormalizedDirection != Vector2.zero;

                if (isRope) {
                    hasHitData = character.TryGetRopeDirection(actionInput.NormalizedDirection, ref newPos, ref rotation, out var cursorPos, ref distancePercentReached);
                    newPos = cursorPos;

                } else if (isProjectile) {
                    hasHitData = character.TryGetProjectileDirection(actionInput.NormalizedDirection, ref newPos, ref rotation, ref distancePercentReached);
                    newPos = character.LegsPosition;

                } else if (isJumpAim) {
                    linePoints = new Vector3[] { Vector3.zero, Vector2.one };
                    var forceDir = actionInput.NormalizedDirection * character.Stats.JumpForce;
                    hasHitData = character.TryGetJumpTrajectory(forceDir, ref newPos, ref linePoints);
                    newPos = character.LegsPosition;
                }

                if (hasAimDir) {
                    _gameplayUtils.ActivateUtil(actionType, newPos, rotation, hasHitData, linePoints);

                } else {
                    _gameplayUtils.DeactivateUtil(actionType);

                }

                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }

            _gameplayUtils.DeactivateUtil(actionType);

        }

        private void ApplyEffectForDuration(Character2DController character, Effect2DType effectType, List<Effect2DType> stopWhenAnyEffectStarts = null) {
            ApplyEffectForDurationAndSetRotation(character, effectType, Quaternion.identity, stopWhenAnyEffectStarts);
        }

        private void ApplyEffectForDurationAndSetRotation(Character2DController character, Effect2DType effectType, Quaternion initialRotation, List<Effect2DType> stopWhenAnyEffectStarts = null) {
            
            _effects ??= GetController<EffectsController>();

            var effectOverrideData = character.GetEffectOverride(effectType);
            var effectData = effectOverrideData != null ? effectOverrideData : _gameplayLogic.GetEffects(effectType);
            _effects?.PlayEffect(effectData, character.transform, initialRotation, stopWhenAnyEffectStarts);
        }

        private async void ApplyProjectileStateWhenThrown(IInteractableBody grabbed, int damage, IInteractableBody thrower, Func<Task> actionOnEnd = null) {

            _debug.Log("trying as damager -> damage: " + damage);

            var _bodyDamager = (grabbed as Character2DController);

            if (damage == 0) {
                return;
            }

            _bodyDamager?.SetProjectileState(true);

            while (thrower != null && grabbed != null && (grabbed.GetVelocity().magnitude < 1 || grabbed.IsGrabbed()) ) {
                await Task.Delay(TimeSpan.FromMilliseconds(20));
            }
            if (thrower == null) {
                return;
            }

            var grabbedVelocity = grabbed.GetVelocity();
            _debug.Log("Started as damager -> Mag: " + grabbedVelocity.magnitude);

            var damagees = new List<Collider2D>() { thrower.GetCollider() };

            var legsPosition = grabbed.GetCenterPosition();
            var grabbedColl = grabbed.GetCollider();
            var hasCircleColliderAsDamagerLayer = grabbedColl as CircleCollider2D;
            var legsRadius = hasCircleColliderAsDamagerLayer != null ? hasCircleColliderAsDamagerLayer.radius : 1;
            Vector2 objClosestPosition;
            Vector3 dirFromCharacterToFoe;
            RaycastHit2D blockingObjectsInAttackDirection;

            while (_gameRunning && grabbedVelocity.magnitude > 1) {

                //_debug.Log("as damager -> Mag: " + grabbedVelocity.magnitude);
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

                        objClosestPosition = interactable.ClosestPoint(legsPosition);
                        dirFromCharacterToFoe = (objClosestPosition - (Vector2)legsPosition).normalized;
                        blockingObjectsInAttackDirection = Physics2D.Raycast(legsPosition,
                    dirFromCharacterToFoe,
                    Vector2.Distance(objClosestPosition, legsPosition), _whatIsSolid);

                        if (blockingObjectsInAttackDirection.collider != null) {
                            continue;
                        }

                        damagees.Add(interactable);
                        
                        var dir = Vector2.up;
                        var directionTowardsFoe = (objClosestPosition.x > legsPosition.x) ? 1 : -1;
                        dir.x = directionTowardsFoe;
                        var newPositionToSetOnFixedUpdate = objClosestPosition + (Vector2)dir.normalized * -(legsRadius);
                        // todo: damage decrease based on velocity?
                        _debug.Log($"damage on {interactableBody.GetTransform().gameObject.name} by {thrower.GetTransform().gameObject.name}");
                        interactableBody.DealDamage(damage);
                        interactableBody.ApplyForce(dir * grabbedVelocity * 0.7f); // 0.7f is an impact damage decrease
                        if (interactableBody is Character2DController) {
                            ApplyEffectForDuration((Character2DController)interactableBody, Effect2DType.ImpactMed);
                        }

                        _debug.DrawLine(objClosestPosition, newPositionToSetOnFixedUpdate, Color.white, 3);
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(10));

                grabbedVelocity = grabbed.GetVelocity();
            }

            _bodyDamager?.SetProjectileState(false);

            if (actionOnEnd == null) {
                return;
            }

            try {
                await actionOnEnd();
            }
            finally { }
        }

        private bool ValidateAttackConditions(AttackBaseStats attack, InteractableObject2DState characterState) {

            var attackerState = characterState.CurrentState;
            if (attack.Ability == AttackAbility.RollAttack) { _debug.Log($"roll attack data: {characterState.Velocity.magnitude}"); }

            foreach (var condition in attack.CharacterStateConditions) {

                switch (condition) {
                    case ObjectState.Running when !_logic.GameplayLogic.IsStateConsideredAsRunning(attackerState, characterState.Velocity.magnitude):
                    case ObjectState.Jumping or ObjectState.Falling when !(_logic.GameplayLogic.IsStateConsideredAsAerial(attackerState)):
                    case ObjectState.Grounded when !_logic.GameplayLogic.IsStateConsideredAsGrounded(attackerState):
                    case ObjectState.Crouching when attackerState != ObjectState.Crouching:
                    case ObjectState.StandingUp when attackerState != ObjectState.StandingUp:
                    case ObjectState.Blocking when attackerState != ObjectState.Blocking:
                    case ObjectState.Grabbing when attackerState != ObjectState.Grabbing:
                        return false;
                }

            }
            
            return true;
        }

        private bool ValidateAlternativeAttackConditions(ref AttackAbility attackAbility, ref AttackBaseStats attack, InteractableObject2DState characterState) {

            var state = characterState.CurrentState;
            _debug.Log($"checking matching alternative Attack for {attackAbility}");
            foreach (var condition in attack.CharacterStateConditions) {
                //_debug.Log($"checking alt condition {condition} is considered ? {characterState.Velocity.magnitude}");
                switch (condition) {

                    case ObjectState.Running when !_logic.GameplayLogic.IsStateConsideredAsRunning(characterState.CurrentState, characterState.Velocity.magnitude):
                        attackAbility = AttackAbility.LightAttackAlsoDefaultAttack;
                        return true;

                    case var _ when condition is ObjectState.StandingUp or ObjectState.Crouching && _logic.GameplayLogic.IsStateConsideredAsAerial(characterState.CurrentState):
                        if (attackAbility is AttackAbility.MediumAttack or AttackAbility.HeavyAttack) {
                            attackAbility = AttackAbility.AerialAttack;
                            return true;

                        } else if (attackAbility is AttackAbility.MediumGrab or AttackAbility.HeavyGrab) {
                            attackAbility = AttackAbility.AerialGrab;
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
                    /*case CharacterState.Falling or CharacterState.Jumping or CharacterState.Grounded
                        when opponentState is (CharacterState.Falling or CharacterState.Jumping or CharacterState.Grounded) :
                        return true;*/
                    
                    case ObjectState.Alive when interactable.GetHpPercent() == 0:
                    case ObjectState.Running when opponentState != ObjectState.Running:
                    case ObjectState.Jumping or ObjectState.Falling when !(_logic.GameplayLogic.IsStateConsideredAsAerial(opponentState)):
                    case ObjectState.Grounded when !_logic.GameplayLogic.IsStateConsideredAsGrounded(opponentState) && !interactable.IsGrabbed():
                    case ObjectState.Crouching when opponentState != ObjectState.Crouching:
                    case ObjectState.StandingUp when opponentState != ObjectState.StandingUp:
                    case ObjectState.Blocking when opponentState != ObjectState.Blocking:
                    case ObjectState.Grabbing when opponentState != ObjectState.Grabbing:
                        return false;
                }

            }

            return true;
        }

        private async void ApplyForceOnFoeWithDelay(IInteractableBody damageableBody, AttackData attackStats, int delayInMilliseconds = 0) {

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
            
            damageableBody.DealDamage(attackStats.Damage);
            damageableBody.ApplyForce(attackStats.ForceDir);

            var damageableCharacter = damageableBody as Character2DController;
            if (damageableCharacter != null) {
                ApplyEffectForDuration((Character2DController)damageableBody, Effect2DType.ImpactLight);
            }

            _debug.DrawRay(damageableBody.GetCenterPosition(), attackStats.ForceDir, Color.red, 4);

            var distanceFromAttacker = damageableBody.GetCenterPosition() - _hero.LegsPosition;
            while (damageableBody != null &&Mathf.Abs(distanceFromAttacker.x) < 15 && Mathf.Abs(distanceFromAttacker.y) < 8
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

        private async void ApplyGrabThrowWhenGrounded(IInteractableBody grabbed, IInteractableBody grabber, AttackData attackStats) {

            await Task.Delay(150);

            while (grabber.GetCurrentState() is not (ObjectState.Grounded or ObjectState.Running or ObjectState.Crouching or ObjectState.StandingUp)) {

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
