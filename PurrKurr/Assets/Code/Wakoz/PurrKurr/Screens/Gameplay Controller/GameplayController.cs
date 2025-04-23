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
using Code.Wakoz.PurrKurr.Screens.CameraSystem;
using Code.Wakoz.PurrKurr.Screens.Init;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDetection;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;
using Code.Wakoz.PurrKurr.Screens.Levels;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors;
using Code.Wakoz.PurrKurr.Popups.OverlayWindow;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger;
using Code.Wakoz.PurrKurr.Screens.Shaker;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors;
using Code.Wakoz.PurrKurr.DataClasses.Effects;
using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.PurrKurr.Screens.Objectives;
using Code.Wakoz.PurrKurr.Agents;
using Code.Wakoz.PurrKurr.Screens.Notifications;
using Code.Wakoz.PurrKurr.DataClasses.GamePlayUtils;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using UnityEngine.TextCore.Text;
using Code.Wakoz.PurrKurr.DataClasses.Enums;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller
{
    [DefaultExecutionOrder(14)]
    public sealed class GameplayController : SingleController {

        private const float _MinVelocityForFlip = 5;

        [SerializeField][Min(0)] private int _startingLevelIndex = 0;
        [SerializeField][Min(0)] private float _attackRadius = 3f;
        [SerializeField] private float _xDistanceFromClingPoint = 0.8f;

        // todo: use Dispatcher for the gameEvents
        public event Action<ActionInput> OnTouchPadDown;
        public event Action<ActionInput> OnTouchPadClick;
        public event Action<ActionInput> OnTouchPadUp;
        public event Action<ActionType, Vector2, Quaternion, bool, Vector3[]> OnAimingAction;
        public event Action<ActionType> OnAimingActionEnd;
        public event Action<Character2DController> OnNewHero;
        public event Action OnLevelStart;
        public event Action<Vector3> OnHeroReposition;
        public event Action<Vector2> OnCameraFocusPoint;
        public event Action<List<Transform>> OnCameraFocusTargets;
        public event Action<InteractableObject2DState> OnStateChanged;
        public event Action<List<DisplayedableStatData>> OnStatsChanged;
        public event Action<float, float> OnTimerChanged;
        public event Action<DetectionZoneTrigger> OnHeroEnterDetectionZone;
        public event Action<DetectionZoneTrigger> OnHeroExitDetectionZone;
        //public event Action<DetectionZoneTrigger> OnHeroConditionCheck;
        public event Action<DetectionZoneTrigger> OnHeroConditionMet;
        public event Action<IObjective, ObjectiveSequenceDataSO> OnObjectiveMissionStarted;
        public event Action<Collider2D> OnInteractablesEntered;
        public event Action<Collider2D> OnInteractablesExited;
        public event Action<Character2DController, IInteractableBody[], float, AttackData, AttackAbility, Vector2, AttackBaseStats> OnNewInteraction;
        public event Action<IInteractableBody> OnInteractableDestoyed;
        public event Action OnHeroDeath;
        public event Action<EffectData, Transform, Quaternion> OnNewEffect;
        public event Action OnHideGuidanceMarker;
        public event Action<ObjectiveMarker> OnShowGuidanceMarker;
        public event Action<NotificationData> OnNewNotification;
        public event Action<float> OnTimeScaleChanged;
        public event Action OnTimeScaleDefault;

        public HandlersCoordinator Handlers { get; private set; }
        [SerializeField] private CameraController _camController;
        [SerializeField] private Character2DController _mainHero;
        private Character2DController _hero;
        private TimelineHandler _timelineHandler;

        private bool _firstTimeInitialized = false;
        private bool _heroInitialized = false;
        private bool _gameRunning = false;

        private LogicController _logic;
        private InputProcessor _inputProcessor;
        public CharacterActionExecuter CharacterActionExecuter { get;  private set; }
        private UIController _ui;
        private GameplayLogic _gameplayLogic;
        private LevelsController _levelsController;
        private InteractablesController _interactables;
        private DebugDrawController _debugDraw;

        private LayerMask _whatIsSolid;
        private LayerMask _whatIsSolidForProjectile;
        private LayerMask _whatIsPlatform;
        private LayerMask _whatIsClingable;
        private LayerMask _whatIsSurface;
        private LayerMask _whatIsCharacter;
        private LayerMask _whatIsDamageable;

        protected override void Clean() {

            _inputProcessor?.Dispose();
            CharacterActionExecuter?.Dispose();

            _gameRunning = false;

            if (_hero == null) {
                return;
            }
            _hero.OnStateChanged -= OnStateChanged;
            _hero.OnUpdatedStats -= UpdateOrInitUiDisplayForCharacter;
            _hero.OnColliderEntered -= OnInteractablesEntered;
            _hero.OnColliderExited -= OnInteractablesExited;

            Handlers.Dispose();
        }

        protected override Task Initialize() {

            _camController ??= Camera.main.GetComponent<CameraController>();
            
            _logic = GetController<LogicController>();
            // handle inputs and generates ExecutableAction
            _inputProcessor = new InputProcessor(this, GetController<InputController>());
            // adjust ExecutableAction by hero context (state) and executes
            CharacterActionExecuter = new CharacterActionExecuter(this, _logic.InputLogic, _logic.GameplayLogic, _inputProcessor);
            InitHandlers();
            _levelsController = GetController<LevelsController>();
            _interactables = GetController<InteractablesController>();
            _debugDraw = GetController<DebugDrawController>();

            _ui ??= GetController<UIController>();
            if (_ui.TryBindToCharacterController(this, _inputProcessor))
            {
                //RegisterInputEvents(); // moved to _inputValidator
            }
            else
            {
                _ui = null;
                Debug.LogWarning("Something went wrong, there is no active ref to PadsDisplayController");
            }

            _gameplayLogic = _logic.GameplayLogic;
            _whatIsSurface = _gameplayLogic.GetSurfaces();
            _whatIsSolid = _gameplayLogic.GetSolidSurfaces();
            _whatIsSolidForProjectile = _gameplayLogic.GetSolidSurfacesForProjectile();
            _whatIsPlatform = _gameplayLogic.GetPlatformSurfaces();
            _whatIsClingable = _gameplayLogic.GetClingableSurfaces();
            _whatIsCharacter = _gameplayLogic.GetDamageables();
            _whatIsDamageable = _gameplayLogic.GetDamageables();

            InitLevel();

            TryInitHero(_mainHero);

            _levelsController.RefreshSpritesOrder(_hero);

            _gameRunning = true;

            return Task.CompletedTask;
        }

        private void InitHandlers()
        {
            Handlers = new HandlersCoordinator();
            InitUpdateProcessHandlers();
            InitBindableHandlers();
        }

        private void InitUpdateProcessHandlers()
        {
            var handlers = new IUpdateProcessHandler[]
            {
                new ShakeHandler(),
                new CameraHandler(_camController),
                new AgentsHandler(),
            };
            Handlers.AddHandlers(handlers);
        }

        private void InitBindableHandlers()
        {
            _timelineHandler = new TimelineHandler(this);

            var bindableHandlers = new IBindableHandler[]
            {
                _timelineHandler,
                new ObjectivesHandler(this, GetController<ObjectivesController>()),
                new ObjectivesMissionHandler(this, GetController<LevelsController>()),
                new GuidanceMarkerHandler(this, GetController<UiGuidanceController>()),
                new CharacterStatsHandler(this)
            };
            Handlers.AddBindableHandlers(bindableHandlers);
        }

        private void InitBindableHandlersForMainHero()
        {
            var bindableHandlers = new List<IBindableHandler>
            {
                new CameraCharacterHandler(Handlers.GetHandler<CameraHandler>(), this, _camController.CameraFocusTransform, _hero.transform),
            };
            Handlers.AddBindableHandlers(bindableHandlers);
        }

        private void InitLevel() {

            _levelsController.InitInteractablePools(_interactables);

#if !UNITY_EDITOR
            _startingLevelIndex = 0;
#endif
            LoadLevel(_startingLevelIndex);
        }

        private bool IsInteractableBodyTheMainHero(IInteractable interactable)
        {
            if (interactable == null)
                return false;

            return IsInteractableBodyTheMainHero(interactable.GetInteractableBody());
        }

        private bool IsInteractableBodyTheMainHero(IInteractableBody interactable)
        {
            if (interactable == null)
                return false;

            return interactable == (IInteractableBody)_hero;
        }

        public void InvokeGuidamceMarker(ObjectiveMarker nextMarker = null) {

            var target = nextMarker?.MarkerRef;

            if (target == null) {
                OnHideGuidanceMarker?.Invoke();
                return;
            }

            OnShowGuidanceMarker?.Invoke(nextMarker);
        }

        public void InvokeNewNotification(string message, float durationInSeconds = 5) {
            OnNewNotification?.Invoke(new NotificationData(message, durationInSeconds));
        }

        public void InvokeUpdateTimer(float secondsLeft, float totalSeconds) {
            OnTimerChanged?.Invoke(secondsLeft, totalSeconds);
        }

        public void InvokeHideTimer() {
            OnTimerChanged?.Invoke(0, 1);
        }

        public bool OnExitDetectionZone(DetectionZoneTrigger zone, Collider2D triggeredCollider)
        {
            if (!IsInteractableBodyTheMainHero(triggeredCollider.GetComponent<IInteractable>()))
                return false;

            OnHeroExitDetectionZone?.Invoke(zone);

            return true;
        }

        public bool OnEnterDetectionZone(DetectionZoneTrigger zone, Collider2D triggeredCollider)
        {
            var interactable = triggeredCollider.GetComponent<IInteractable>();
            if (!IsInteractableBodyTheMainHero(interactable))
                return false;

            SetButtonsActionByDetectionZone(zone);
            CheckForConditionalGameEvent(interactable.GetInteractableBody(), zone);

            OnHeroEnterDetectionZone?.Invoke(zone);

            return true;
        }

        // todo: move this to a handler script of windowOverlayButtonsHandler
        private void SetButtonsActionByDetectionZone(DetectionZoneTrigger zone)
        {
            var windowPageData = zone.GetWindowData();

            switch (zone)
            {
                case DoorController door:

                    for (var i = 0; i < windowPageData.Count; i++)
                    {
                        var page = windowPageData[i];
                        var confirmButtons =
                            page.ButtonsRawData.Where(obj => obj.ButtonType == GenericButtonType.Confirm).FirstOrDefault();

                        if (confirmButtons != null)
                        {
                            var nextDoor = door.GetNextDoor();
                            var nextDoorPosition = nextDoor != null ? nextDoor.gameObject : null;
                            confirmButtons.ClickedAction = () => LoadLevel(door.GetRoomIndex(), nextDoorPosition);
                        }
                    }

                break;

                case ObjectiveZoneTrigger objectiveTrigger:

                    for (var i = 0; i < windowPageData.Count; i++)
                    {
                        OverlayWindowData page = windowPageData[i];
                        var confirmButtons =
                            page.ButtonsRawData.Where(obj => obj.ButtonType is GenericButtonType.Confirm or GenericButtonType.Close).FirstOrDefault();

                        if (confirmButtons != null)
                    {
                        var objectiveData = Handlers.GetHandler<ObjectivesHandler>()
                            .GetObjectiveByUniqueId(objectiveTrigger.SequenceData.UniqueId);

                        confirmButtons.ClickedAction = () => InvokeStartObjectiveMission(objectiveData, objectiveTrigger.SequenceData);
                    }
                }

                break;

                case OverlayWindowTrigger windowTrigger:
                    for (var i = 0; i < windowPageData.Count; i++)
                    {
                        OverlayWindowData page = windowPageData[i];
                        var confirmButtons =
                            page.ButtonsRawData.Where(obj => obj.ButtonType is GenericButtonType.Confirm or GenericButtonType.Close).FirstOrDefault();

                        if (confirmButtons != null)
                        {
                            confirmButtons.ClickedAction = () => windowTrigger.TurnOffWindow();
                        }
                    }

                break;
            }

        }

        private void InvokeStartObjectiveMission(IObjective objectiveData, ObjectiveSequenceDataSO sequencSOData)
        {
            OnObjectiveMissionStarted?.Invoke(objectiveData, sequencSOData);
        }

        private void CheckForConditionalGameEvent(IInteractableBody character, DetectionZoneTrigger zone, int pageIndex = 0) {

            if (character == null || zone is not OverlayWindowTrigger)
                return;

            var windowTrigger = zone as OverlayWindowTrigger;

            var condition = windowTrigger.GetCondition(pageIndex);
             if (condition != null) {
                var applySlowMotion = windowTrigger.IsAffectingTimeScale();
                if (applySlowMotion)
                {
                    OnTimeScaleChanged?.Invoke(0);
                }
                condition.StartCheck(character,
                    () =>
                    {
                        if (applySlowMotion)
                        {
                            OnTimeScaleDefault?.Invoke();
                        }
                        OnHeroConditionMet?.Invoke(zone);
                    });
            }
        }

        private void LoadLevel(int levelToLoad, GameObject heroSpawnPoint = null)
        {
            var newPosition = Vector3.zero;
            var hasSpawnPoint = heroSpawnPoint != null;

            if (hasSpawnPoint)
            {
                newPosition = heroSpawnPoint.transform.position;
                _hero.transform.position = newPosition + (Vector3.up * _hero.LegsRadius *.5f );
                _hero.SetTargetPosition(newPosition + (Vector3.up * _hero.LegsRadius ));
            }

            var isLeadingToDoorWithinTheSameLevel = _levelsController.CurrentLevelIndex == levelToLoad;
            if (isLeadingToDoorWithinTheSameLevel) {
                return;
            }

            // clean up
            InvokeGuidamceMarker(null);

            // set up
            _levelsController.LoadLevel(levelToLoad);
            OnLevelStart?.Invoke();
            var levelObjectivesData = _levelsController.GetLevel(levelToLoad).GetObjectives();
            Handlers.GetHandler<ObjectivesHandler>().Initialize(levelObjectivesData);
            if (hasSpawnPoint)
            {
                OnHeroReposition?.Invoke(newPosition);
            }
            _levelsController.NotifyAllTaggedObjects();
            
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

        public void UnlockHeroAbilities(List<ActionType> characterAbilities) {
            _hero.Stats.SetUnlockedAbilities(characterAbilities);
            _hero.RefreshStats();
        }

        public void SetNewHero(Character2DController hero) => TryInitHero(hero);

        private bool TryInitHero(Character2DController hero)
        {

            if (hero == null)
            {
                return false;
            }

            if (_hero != null && _heroInitialized)
            {
                _hero.OnStateChanged -= OnStateChanged;
                _hero.OnUpdatedStats -= UpdateOrInitUiDisplayForCharacter;
                _hero.OnColliderEntered -= OnInteractablesEntered;
                _hero.OnColliderExited -= OnInteractablesExited;
            }

            _hero = hero;

            InitBindableHandlersForMainHero();

            OnNewHero?.Invoke(_hero);

            _hero.OnStateChanged += OnStateChanged;
            _hero.OnUpdatedStats += UpdateOrInitUiDisplayForCharacter;
            _hero.OnColliderEntered += OnInteractablesEntered;
            _hero.OnColliderExited += OnInteractablesExited;

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
        // TODO: use this for ref to process inputs on main hero
/*
        private void OnActionStarted(ActionInput actionInput) {
            
            if (IsAlive(_hero)) {

                if (CanPerformAction()) {
                    if (_inputValidator.ValidateNavigation(actionInput, true, false, _hero,
                            out var moveSpeed, out Vector2 forceDirNavigation, out var navigationDir)) {

                        _hero.DoMove(moveSpeed);
                        _hero.SetForceDir(forceDirNavigation);
                        _hero.SetNavigationDir(navigationDir);
                        if (_hero.IsGrabbed()) {
                            FaceCharacterByNavigationDir(navigationDir);
                        }
                    }

                    if (_inputValidator.ValidateAction(actionInput, true, false, _hero,
                            out var isActionPerformed, out var forceDir, out var moveToPosition, out var interactedColliders)) {

                        if (isActionPerformed) {

                            if (interactedColliders != null && !_hero.State.IsGrabbed()) {
                                CombatLogic(_hero, actionInput.ActionType, moveToPosition, interactedColliders);

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
                                DisconnectFromAnyGrabbing(_hero);
                                AlterJumpDirByStateNavigationDirection(ref forceDir, _hero.State);
                                _hero.SetJumping(Time.time + .05f);
                                _hero.SetForceDir(forceDir);
                                _hero.DoMove(0); // might conflict with the TryPerformInputNavigation when the moveSpeed is already set by Navigation
                                ApplyEffectOnCharacter(_hero, Effect2DType.DustCloud, _hero.State.ReturnForwardDirByTerrainQuaternion());

                            } else if (actionInput.ActionType == ActionType.Block) {
                                ApplyEffectOnCharacter(_hero, Effect2DType.BlockActive);
                            }

                            _hero.State.SetActiveCombatAbility(actionInput.ActionType);
                        }

                    }

                // todo: replace the condition so it allows for stacking future actions (CanPerformAction() || CanOnlyStackActions())
                }*//* else if (CanOnlyStackActions()) {
                    //RegisterInteractionEvent();
                }*//*

            } else {

                _hero.DoMove(0);
            }
            
            OnTouchPadDown?.Invoke(actionInput);
        }

        private void OnActionOngoing(ActionInput actionInput) {
            
            if (IsAlive(_hero) && CanPerformAction()) {

                if (_inputValidator.ValidateNavigation(actionInput, false, false, _hero, 
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

                *//*if (_inputInterpreterLogic.TryPerformInputAction(actionInput, false, false, _hero,
                    out var isActionPerformed, out var forceDir, out var moveToPosition, out var interactedCollider)) {

                }*//*
            }
            
            OnTouchPadClick?.Invoke(actionInput);
        }

        private void OnActionEnded(ActionInput actionInput) {

            if (_inputValidator.ValidateNavigation(actionInput, false, true, _hero,
                    out var moveSpeed, out Vector2 forceDirNavigation, out var navigationDir)) {

                _hero.DoMove(moveSpeed);
                _hero.SetForceDir(forceDirNavigation);
                _hero.SetNavigationDir(NavigationType.None);
            }

            if (_inputValidator.ValidateAction(actionInput, false, true, _hero,
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
                                ApplyEffectOnCharacter(_hero, Effect2DType.DodgeActive);
                                ApplyEffectOnCharacter(_hero, Effect2DType.DustCloud, _hero.State.ReturnForwardDirByTerrainQuaternion());

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
                                    _hero.SetJumping(Time.time + .1f);
                                    forceDir = actionInput.NormalizedDirection * _hero.Stats.JumpForce;
                                    ApplyEffectOnCharacter(_hero, Effect2DType.DustCloud, _hero.State.ReturnForwardDirByTerrainQuaternion());
                                }
                            }
                        }

                        if (isProjectilingEnded || isRopingEnded || isJumpAimEnded) {
                            _hero.State.StopAiming();
                        }
                    }
                }

                if (forceDir != Vector2.zero) {
                    _hero.SetForceDir(forceDir, true);
                }

                _hero.SetTargetPosition(moveToPosition);

            }

            OnTouchPadUp?.Invoke(actionInput);
        }
*/
        public void ShootProjectile(Character2DController character, ActionInput actionInput, ref Vector2 newPos, ref Quaternion rotation, ref float distancePercentReached) {

            character.TryGetProjectileDirection(actionInput.NormalizedDirection, ref newPos, ref rotation, ref distancePercentReached);

            var projectile = _interactables.GetInstance<ProjectileController>();
            if (projectile == null) {
                return;
            }

            character.State.SetInterruptibleAnimation(Time.time + 0.3f);

            projectile.transform.SetPositionAndRotation(character.LegsPosition, rotation);
            projectile.SetTargetPosition(newPos, distancePercentReached);
            ApplyEffectOnCharacter(character, Effect2DType.DustCloud);
            //ApplyEffectForDuration(character, Effect2DType.TrailInAir); // todo: other trails for fire ice etc
            ApplyProjectileStateWhenThrown(projectile, character.Stats.Damage, character, () => _interactables.ReleaseInstance(projectile));

        }

        private List<RopeController> _charactersRopes = new();

        public List<RopeController> CharactersRope => _charactersRopes;

        public async void ShootRope(Character2DController character, ActionInput actionInput, Vector2 newPos, Quaternion rotation, float distancePercentReached) {

            var hasNoHit = !character.TryGetRopeDirection(actionInput.NormalizedDirection, ref newPos, ref rotation, out var cursorPosition, ref distancePercentReached);

            if (hasNoHit) {
                return;
            }

            var rope = _interactables.GetInstance<RopeController>();
            if (rope == null) {
                Debug.LogWarning("No available rope instance in pool, consider increasing the max items capacity");
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
            SetCameraScreenShake(0.12f, 0.15f);
        }

        private void ApplyClingingAction(Character2DController character, Collider2D coll) {

            /*if (coll is not EdgeCollider2D) {
                return;
            }*/

            var navType = character.State.NavigationDir;
            var facingRightAsInt = character.State.GetFacingRightAsInt();
            var horizontalNavigationDirAsInt = _logic.InputLogic.GetNavigationDirAsFacingRightInt(navType);
            //var isPlayerMovingTowardsWall = horizontalNavigationDirAsInt == 1 && _hero.State.IsFrontWall();

            if (!character.State.IsClinging() && _hero.State.IsFrontWall() 
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
            wallPoint += xHorizontalOffsetFromWall * character.LegsRadius * _xDistanceFromClingPoint;
            character.SetAsClinging(edges, wallPoint);

            Debug.DrawLine(edgesClosestPoint, wallPoint, Color.magenta, 1f);
        }

        public void CutRopeLink(IInteractableBody character) {

            var grabbedInteractable = character.GetGrabbedTarget();
            var ropeLink = grabbedInteractable as RopeLinkController;
            if (ropeLink == null) {
                return;
            }

            // updating the rope is called thru the link event
            var rope = _charactersRopes.FirstOrDefault();
            var nextLink = rope?.GetNextChainedLink(ropeLink, 1);
            if (nextLink != null) {
                nextLink.DealDamage(1); 
            };
        }

        public void DisconnectFromRope(IInteractableBody character) {

            var grabbedInteractable = character.GetGrabbedTarget();
            var ropeLink = grabbedInteractable as RopeLinkController;
            if (ropeLink == null) {
                return;
            }

            var rope = _charactersRopes.FirstOrDefault();
            rope.HandleDisconnectInteractableBody(ropeLink, character);
        }

        public void DisconnectFromAnyGrabbing(Character2DController character) {

            DisconnectFromRope(character);
            DisconnectAnyGrabbing(character);
        }

        private void DisconnectAnyGrabbing(Character2DController character) {
            
            //if (character.State.GetClingableObject() != null) { }
            character.SetAsClinging(null, character.LegsPosition);
        }

        private bool CanPerformAction() => _hero.State.CanPerformAction();
        private bool CanOnlyStackActions() => _hero.State.CurrentState is ObjectState.Attacking or ObjectState.InterruptibleAnimation;

        private void FaceCharacterByNavigationDir(NavigationType navigationDir) {

            if (_logic.InputLogic.IsNavigationDirValidAsRight(navigationDir)) {
                _hero.FlipCharacterTowardsPoint(true);
            } else if (_logic.InputLogic.IsNavigationDirValidAsLeft(navigationDir)) {
                _hero.FlipCharacterTowardsPoint(false);
            }
        }

        public void AlterJumpDirByStateNavigationDirection(ref Vector2 forceDir, InteractableObject2DState state) {
            
            var navigationDir = state.NavigationDir;
            var isNavDirUpOrNone = navigationDir is NavigationType.Up or NavigationType.None;

            var facingDir =
                _logic.InputLogic.IsNavigationDirValidAsLeft(navigationDir) ? -1
                : _logic.InputLogic.IsNavigationDirValidAsRight(navigationDir) ? 1
                : _hero.State.GetFacingRightAsInt();

            // todo: flip horizontal when player infront of wall?
            var flipJumpHorizontalWhenClinging = false ;// _hero.State.IsFrontWall() && !isNavDirUpOrNone && facingDir == _hero.State.GetFacingRightAsInt();

            var horizontalDir = facingDir * (flipJumpHorizontalWhenClinging ? -1 : 1);

            //Debug.DrawRay(_hero.LegsPosition, forceDir, Color.white, 3);
            if (isNavDirUpOrNone) {
                forceDir = HelperFunctions.RotateVector(forceDir, horizontalDir * -5);

            } else {
                forceDir = HelperFunctions.RotateVector(forceDir, horizontalDir * -18);

            }
            _debugDraw.DrawRay(_hero.LegsPosition, forceDir, Color.white, 3);
        }

        private void AlterThrowDirBasedOnNavigationDirection(ref Vector2 throwDir, ref int facingRightOrLeftTowardsPoint, NavigationType navigationDir, bool isAerial) {

            switch (navigationDir) {

                case NavigationType.None:
                    break;
                
                case NavigationType.Up:
                    throwDir = HelperFunctions.RotateVector(throwDir, 25 * _hero.State.GetFacingRightAsInt());
                    break;

                case NavigationType.Down:
                    throwDir = HelperFunctions.RotateVector(throwDir, 45 * _hero.State.GetFacingRightAsInt());
                    break;

                case NavigationType.Left:
                    throwDir = HelperFunctions.RotateVector(throwDir, 45);
                    facingRightOrLeftTowardsPoint = -1;
                    break;
                
                case NavigationType.Right:
                    throwDir = HelperFunctions.RotateVector(throwDir, -45);
                    facingRightOrLeftTowardsPoint = 1;
                    break;
                
                case NavigationType.UpRight:
                    throwDir = HelperFunctions.RotateVector(throwDir, -25);
                    facingRightOrLeftTowardsPoint = 1;
                    break;
                
                case NavigationType.UpLeft:
                    throwDir = HelperFunctions.RotateVector(throwDir, 25);
                    facingRightOrLeftTowardsPoint = -1;
                    break;
                
                case NavigationType.DownRight:
                    throwDir = HelperFunctions.RotateVector(throwDir, -100);
                    facingRightOrLeftTowardsPoint = 1;
                    break;
                
                case NavigationType.DownLeft:
                    throwDir = HelperFunctions.RotateVector(throwDir, 100);
                    facingRightOrLeftTowardsPoint = -1;
                    break;
            }

            _debugDraw.DrawRay(_hero.LegsPosition, throwDir, Color.gray, 3);
            if (!_logic.InputLogic.IsNavigationDirValidAsUp(navigationDir)) {
                throwDir = AlterDirectionToMaxForce(throwDir);
            }
            if (isAerial && _logic.InputLogic.IsNavigationDirValidAsDown(navigationDir)) {
                throwDir.y = -Mathf.Abs(throwDir.y);
                if (navigationDir == NavigationType.Down) {
                    throwDir.x *= .5f;
                }
            }
            _debugDraw.DrawRay(_hero.LegsPosition, throwDir, Color.magenta, 3);
        }

        private List<TimelineEvent> startedEvents = new();
        private List<TimelineEvent> endedEvents = new();

        private void Update() {
            
            if (_timelineHandler == null) {
                return;
            }

            _timelineHandler.UpdateEvents(Time.time, ref startedEvents, ref endedEvents);

            // todo: move this to InteractionHandler responsibility as Tick method
            if (startedEvents != null && startedEvents.Count > 0) {

                foreach (var e in startedEvents) {

                    if (e.Targets.Length > 0) {
                        var facingRightWhileMoving = e.Targets.FirstOrDefault().GetCenterPosition().x > e.Attacker.LegsPosition.x ? true : false;
                        e.Attacker.FlipCharacterTowardsPoint(facingRightWhileMoving);
                    }
                    e.Attacker.SetTargetPosition(e.InteractionPosition);
                    e.Attacker.State.SetMoveAnimation(Time.time + e.AttackProperties.ActionDuration);
                    e.Attacker.State.SetCombatTime(
                        _logic.AbilitiesLogic.GetAbilityType(e.InteractionType),
                        Time.time + e.AttackProperties.ActionDuration);

                }
            }

            if (endedEvents != null && endedEvents.Count > 0) {

                foreach (var e in endedEvents) {

                    var newFacingDirection = 0;
                    var hitPosition = e.InteractionPosition;

                    IInteractableBody[] nearbyTargets = e.Targets;
                    /*if (_logic.AbilitiesLogic.IsAbilityAnAttack(e.InteractionType)) {

                        var distanceLimiter = _attackRadius;
                        nearbyTargets = e.Targets.Where(
                            obj => Vector2.Distance(obj.GetCenterPosition(), hitPosition) < distanceLimiter).OrderBy(obj
                                    => Vector2.Distance(obj.GetCenterPosition(), hitPosition)).ToArray();

                        if (nearbyTargets.Length == 0 && e.Targets != null && e.Targets.Length > 0) {
                            Debug.LogWarning($"could not find any targets after filtering by distance -> {_attackRadius}");
                            // nearbyTargets = e.Targets;

                        }

                    } else {
                        nearbyTargets = e.Targets;
                    }*/

                    e.Attacker.State.SetInterruptibleAnimation(Time.time + e.AttackProperties.ActionCooldownDuration);
                    /*e.Attacker.State.SetCombatTime(
                        _logic.AbilitiesLogic.GetAbilityType(e.InteractionType), 
                        Time.time + e.AttackProperties.ActionCooldownDuration);*/

                    HitTargets(e.Attacker, ref hitPosition, nearbyTargets, ref newFacingDirection, e.InteractionType, e.AttackProperties);

                    if (newFacingDirection != 0) {

                        e.Attacker.FlipCharacterTowardsPoint(newFacingDirection == 1);
                        //e.Attacker.SetTargetPosition(moveToPos);
                    }

                }
            }

            Handlers.UpdateHandlers();
        }

        /*private void FixedUpdate()
        {
            //DoTick();
        }*/

        public void InvokeTouchPadDown(ActionInput action) => OnTouchPadDown?.Invoke(action);
        public void InvokeTouchPadClick(ActionInput action) => OnTouchPadClick?.Invoke(action);
        public void InvokeTouchPadUp(ActionInput action) => OnTouchPadUp?.Invoke(action);

        public void SetCameraScreenShake(float duration = 0.3f, float intensity = 0.4f)
        {
            var camTransform = _camController.transform;
            var curve = AnimationCurve.Linear(0, 0, 1, 0);
            curve.AddKey(0.3f, 1);
            Handlers.GetHandler<ShakeHandler>().TriggerShake(camTransform, 
                new ShakeData(ShakeStyle.Circular, duration, intensity, curve, 
                new Vector3(camTransform.position.x, camTransform.position.y, -10)));
        }

        public void CombatLogic(Character2DController attacker, ActionType actionType) {

            var newFacingDirection = 0;
            var attackAbility = _logic.AbilitiesLogic.GetAttackAbility(attacker.GetNavigationDir(), actionType, attacker.State.CurrentState);

            if (attacker.Stats.TryGetAttack(ref attackAbility, out var attackProperties)) {

                Collider2D[] interactedColliders = attacker.NearbyInteractables(); //.Where(o => o.GetComponent<IInteractable>()?.GetInteractableBody()?.GetHpPercent() > 0).FirstOrDefault();
                ValidateAttackCondition(ref attacker, ref attackAbility, ref attackProperties);
                GetAvailableTargets(ref attacker, ref attackProperties, ref interactedColliders, out var interactableBodies);
                //GetCombatNewPos(ref attacker, ref actionType, ref interactableBodies, out var moveToPosition);
                RegisterInteractionEvent(attacker/*, ref moveToPosition*/, ref interactableBodies, ref newFacingDirection, ref attackAbility, ref attackProperties);
            
            } else {
                Debug.LogWarning($"Attack Ability {attackAbility} is missing from attack moves");
            }

            /*var isNewFacingDirectionSetAndCharacterIsFacingAnything = newFacingDirection != 0;
            if (isNewFacingDirectionSetAndCharacterIsFacingAnything) {

                attacker.FlipCharacterTowardsPoint(newFacingDirection == 1);
                attacker.SetTargetPosition(moveToPosition);
            }*/

        }

        private void GetCombatNewPos(ref Character2DController character, ref ActionType actionType, ref IInteractableBody[] closestInteractables, out Vector2 moveToPosition) {

            moveToPosition = character.LegsPosition;

            if (closestInteractables == null || closestInteractables.Length == 0) {
                return;
            }

            var closestFoePosition = closestInteractables.FirstOrDefault().GetCollider().ClosestPoint(character.LegsPosition);
            
            switch (actionType) {

                    //var dir = Vector2.up;//((Vector3)closestFoePosition - character.LegsPosition);
                    //moveToPosition = closestFoePosition + Vector2.up * (character.LegsRadius * .5f);
                    //Debug.DrawLine(character.LegsPosition, moveToPosition, Color.green, 3); // todo: move drawline to gameplaycontroller
                    //break;

                case Definitions.ActionType.Grab:
                case Definitions.ActionType.Attack:

                    var legsPosition = character.LegsPosition;
                    var dir = ((Vector3)closestFoePosition - legsPosition);
                    var directionTowardsFoe = (closestFoePosition.x > legsPosition.x) ? 1 : -1;
                    if (dir == Vector3.zero) {
                        dir = new Vector3(directionTowardsFoe, 0, 0);
                    }
                    moveToPosition = closestFoePosition + (Vector2)dir.normalized * -(character.LegsRadius);
                    Debug.DrawLine(legsPosition, moveToPosition, Color.green, 3); // todo: move drawline to gameplaycontroller
                    dir = closestFoePosition - moveToPosition;

                    break;

                case Definitions.ActionType.Block:

                    
                    //var dir = Vector2.up;//((Vector3)closestFoePosition - character.LegsPosition);

                    moveToPosition = character.LegsPosition;// + Vector2.up * (character.LegsRadius);
                    Debug.DrawLine(character.LegsPosition, moveToPosition, Color.green, 3); // todo: move drawline to gameplaycontroller
                    break;

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
                    Debug.Log($"no alternative attack for {attackAbility}");
                }
            }

        }

        private void GetAvailableTargets(ref Character2DController attacker, ref AttackBaseStats attackProperties, ref Collider2D[] interactedColliders, out IInteractableBody[] interactableBodies) {

            interactableBodies = new IInteractableBody[] { };
            SetSingleOrMultipleTargets(attacker, ref interactedColliders, ref interactableBodies);

            var canAttackMultiTarget = attackProperties.Properties.Count > 0 && attackProperties.Properties.Contains(AttackProperty.MultiTargetOnSurfaceHit);
            if (canAttackMultiTarget && interactedColliders.Length > 1) {
                attacker.FilterNearbyCharactersAroundHitPointByDistance(ref interactableBodies, interactableBodies.FirstOrDefault().GetCenterPosition(), attacker.Stats.MultiTargetsDistance);
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

                var interactableBody = interactable?.GetInteractableBody();

                if (interactableBody == null) {
                    continue;
                }
                
                latestInteractionIsInTargetsList = latestInteractionIsInTargetsList ? true : 
                    latestInteractionAvailable && interactableBody == latestInteraction;

                targetsList.Add(interactableBody);
            }
            // todo: add section to focus on the last interactable interaction 
            /*if (latestInteractionAvailable && latestInteractionIsInTargetsList) {

                var interactionPositionedRight = attacker.LegsPosition.x < latestInteraction.GetCenterPosition().x;
                if (interactionPositionedRight && _logic.InputLogic.IsNavigationDirValidAsRight(attacker.State.NavigationDir) ||
                    !interactionPositionedRight && _logic.InputLogic.IsNavigationDirValidAsLeft(attacker.State.NavigationDir) ||
                    attacker.State.IsFacingRight() == interactionPositionedRight) {

                    targetsList = targetsList.OrderBy(item => item != latestInteraction).ToList();

                }
            }*/

            targets = targetsList.ToArray();
            return;
        }

        private bool IsAlive(IInteractableBody interactableBody) => interactableBody.GetHpPercent() > 0;

        private void RegisterInteractionEvent(Character2DController attacker/*, ref Vector2 moveToPosition*/, ref IInteractableBody[] interactedColliders, ref int newFacingDirection, ref AttackAbility attackAbility, ref AttackBaseStats attackProperties) {

            var canMultiHit = attackProperties.Properties.Count > 0 && attackProperties.Properties.Contains(AttackProperty.MultiTargetOnSurfaceHit);
            IInteractableBody latestInteraction = null;
            List<IInteractableBody> validInteractables = new();

            AttackData attackStats = null;

            foreach (var col in interactedColliders) {
                
                var validAttack = TryPerformCombat(attacker, ref attackAbility, ref attackProperties, col);
                
                if (validAttack != 0 && newFacingDirection == 0) {

                    newFacingDirection = validAttack;
                    latestInteraction = col;
                    validInteractables.Add(col);

                    if (!canMultiHit) {
                        break;
                    }
                } 
            }
            var v = validInteractables.ToArray();
            var abilityType = _logic.AbilitiesLogic.GetAbilityType(attackAbility);
            GetCombatNewPos(ref attacker, ref abilityType, ref v, out var moveToPosition);

            OnNewInteraction?.Invoke(attacker, v, Time.time, attackStats, attackAbility, moveToPosition, attackProperties);
            
            attacker.State.MarkLatestInteraction(latestInteraction);
        }

        private void HitTargets(Character2DController attacker, ref Vector2 moveToPosition, IInteractableBody[] interactedColliders, ref int newFacingDirection, AttackAbility attackAbility, AttackBaseStats attackProperties) {

            var canMultiHit = attackProperties.Properties.Count > 0 && attackProperties.Properties.Contains(AttackProperty.MultiTargetOnSurfaceHit);
            IInteractableBody latestInteraction = null;

            attacker.State.SetInterruptibleAnimation(Time.time + attackProperties.ActionCooldownDuration);

            if (interactedColliders.Length == 0) {
                PerformCombat(attacker, ref attackAbility, ref attackProperties, moveToPosition, null);
                return;
            }

            foreach (var col in interactedColliders) {

                var validAttack = PerformCombat(attacker, ref attackAbility, ref attackProperties, moveToPosition, col);

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
                
                if (latestInteraction.GetHpPercent() <= 0) {
                    latestInteraction = null;
                }
            }
            attacker.State.MarkLatestInteraction(latestInteraction);
        }

        private Vector2 GetAttackDirection(float pushbackForce, Vector2 startPosition, Vector2 endPosition, float legsRadius) {

            //var closestColl = closestColliders.FirstOrDefault();
            //var closestFoePosition = closestColl.ClosestPoint(startPosition);
            var dir = (endPosition - startPosition);
            var directionTowardsFoe = (endPosition.x > startPosition.x) ? 1 : -1;
            if (dir == Vector2.zero) {
                dir = new Vector3(directionTowardsFoe, 0);
            }
            var endPosOffsetByBodyRadius = endPosition + (Vector2)dir.normalized * -(legsRadius);
            Debug.DrawLine(startPosition, endPosOffsetByBodyRadius, Color.green, 3); // todo: move drawline to gameplaycontroller
            dir = endPosition - endPosOffsetByBodyRadius;
            var attackDir = dir.normalized * pushbackForce;

            return attackDir;
        }

        private int TryPerformCombat(Character2DController attacker, ref AttackAbility attackAbility, ref AttackBaseStats attackProperties, IInteractableBody interactedCollider) {

            var facingRightOrLeftTowardsPoint = 0;

            if (interactedCollider == null) {
                return facingRightOrLeftTowardsPoint;
            }

            var isBlockAction = _logic.AbilitiesLogic.IsAbilityBlock(attackAbility);
            if (isBlockAction) {
                return attacker.State.GetFacingRightAsInt();
            }

            var isGrabAction = _logic.AbilitiesLogic.IsAbilityAGrab(attackAbility);
            var isAttackAction = !isGrabAction && _logic.AbilitiesLogic.IsAbilityAnAttack(attackAbility);
            if (!isAttackAction && !isGrabAction) {
                return facingRightOrLeftTowardsPoint;
            }

            if (isGrabAction)
            {
                if (interactedCollider.GetTransform().GetComponent<Rigidbody2D>() == null || 
                attackProperties.Properties.Contains(AttackProperty.GrabAndPickUp) && interactedCollider.GetTransform().GetComponent<AnchorHandler>() == null)
                {
                    Debug.LogWarning($"Invalid pick-up, {attackAbility} prevented on {interactedCollider.GetTransform().gameObject.name}");
                    return facingRightOrLeftTowardsPoint;
                }
            }

            var foeValidState = ValidateOpponentConditions(attackProperties, interactedCollider);
            if (!foeValidState) {
                Debug.LogWarning($"Invalid state, {attackAbility} prevented on {interactedCollider.GetTransform().gameObject.name}");
                return facingRightOrLeftTowardsPoint;
            }

            facingRightOrLeftTowardsPoint = interactedCollider.GetCenterPosition().x > attacker.LegsPosition.x ? 1 : -1;
            return facingRightOrLeftTowardsPoint;
        }

        private int PerformCombat(Character2DController attacker, ref AttackAbility attackAbility, ref AttackBaseStats attackProperties, Vector2 moveToPosition, IInteractableBody interactedCollider) {
            
            var facingRightOrLeftTowardsPoint = attacker.State.GetFacingRightAsInt();

            var isBlockAction = _logic.AbilitiesLogic.IsAbilityBlock(attackAbility);
            if (isBlockAction) {
                ApplyEffectOnCharacter(attacker, Effect2DType.BlockActive);
            }

            if (interactedCollider == null) {
                return facingRightOrLeftTowardsPoint;
            }

            var foe = interactedCollider;
            var foeState = foe.GetCurrentState();

            if (foeState is ObjectState.Dodging) {
                return facingRightOrLeftTowardsPoint;
            }

            var isAttackAction = _logic.AbilitiesLogic.IsAbilityAnAttack(attackAbility);
            
            facingRightOrLeftTowardsPoint = foe.GetCenterPosition().x > attacker.LegsPosition.x ? 1 : -1;
                
            var properties = attackProperties.Properties;
            var isFoeBlocking = foeState is ObjectState.Blocking;

            if (isAttackAction) {
                
                AttackData attackStats = new AttackData(attacker.Stats.Damage, GetAttackDirection(attacker.Stats.PushbackForce, moveToPosition, interactedCollider.GetCollider().ClosestPoint(attacker.GetCenterPosition()), attacker.LegsRadius));

                //_timelineEventManager.RegisterInteractionEvent(attacker, new IInteractableBody[] { foe }, Time.time, attacker.Stats.AttackDurationInMilliseconds, attackStats, attackAbility);

                if (foe.GetCurrentState() is ObjectState.Jumping or ObjectState.Falling &&
                    (properties.Contains(AttackProperty.PushDownOnHit) ||
                    properties.Contains(AttackProperty.PushDownOnBlock) && isFoeBlocking)) {

                    if (properties.Contains(AttackProperty.PushDownOnHit) && isFoeBlocking) 
                    {
                        ApplyEffect(foe, Effect2DType.ImpactOnBlock);
                        return facingRightOrLeftTowardsPoint; 
                    }
                    attackStats.ForceDir.y = -Mathf.Abs(attackStats.ForceDir.y);
                    ApplyForce(foe, attackStats);
                    ApplyEffect(foe, Effect2DType.ImpactCritical);
                    DealDamageAndNotifyAndDisconnectIfGrabbedIsDead(foe, attackStats.Damage, attacker);
                    Handlers.GetHandler<ShakeHandler>().TriggerShake(foe.GetCharacterRigTransform(), new ShakeData(ShakeStyle.VerticalCircular));

                    // todo: make the aerial attack turn into grab to groundsmash, and turn aerial grab into throwing the opponent
                    // ApplyProjectileStateWhenThrown(foe, attackStats.Damage, interactedCollider);

                } else if (properties.Contains(AttackProperty.PushBackOnHit) ||
                    properties.Contains(AttackProperty.PushBackOnBlock) && isFoeBlocking) {

                    var foeCharacter = foe as Character2DController;

                    if (properties.Contains(AttackProperty.PushBackOnHit) && isFoeBlocking)
                    {
                        ApplyEffect(foe, Effect2DType.ImpactOnBlock);
                        //stun foe with animation cooldown
                        if (isFoeBlocking) {

                            AttackData parryActionStats = new AttackData(0, Vector2.up * attacker.Stats.PushbackForce);
                            parryActionStats.ForceDir = HelperFunctions.RotateVector(parryActionStats.ForceDir, 5 * facingRightOrLeftTowardsPoint);
                            attacker.ApplyForce(parryActionStats.ForceDir);
                            //foe.SetTargetPosition(foe.GetCenterPosition());
                            var curve = AnimationCurve.Linear(0, 0, 1, 0);
                            curve.AddKey(0.25f, 1);
                            Handlers.GetHandler<ShakeHandler>().TriggerShake(attacker.GetCharacterRigTransform(), new ShakeData(ShakeStyle.HorizontalCircular, 1f, 0.3f, curve));
                            ApplyStunEffectOnCharacter(attacker, 1);
                            attacker.State.SetStunnedAnimation(Time.time + 1);
                            attacker.DoMove(0);
                        }
                        return facingRightOrLeftTowardsPoint;
                    }

                    if (foeCharacter != null && (_gameplayLogic.IsStateConsideredAsGrounded(foeState) || foeCharacter.State.CanMoveOnSurface())) {
                        _debugDraw.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.cyan, 4);
                        var upwardDirByTerrain =
                            (foeCharacter.State.ReturnForwardDirByTerrainQuaternion()) * Quaternion.AngleAxis(180, new Vector3(0, 0, 1));
                        _debugDraw.DrawRay(foe.GetCenterPosition(), upwardDirByTerrain * Vector3.up, Color.grey, 4);
                        // Convert the Vector2 to a Quaternion
                        Quaternion forceRotation = Quaternion.Euler(0, 0, Mathf.Atan2(attackStats.ForceDir.y, attackStats.ForceDir.x) * Mathf.Rad2Deg).normalized;

                        // Choose the interpolation method based on the dot product
                        float dotProduct = Quaternion.Dot(upwardDirByTerrain, forceRotation);
                        Quaternion averageDirBetweenGroundAndHit;
                        averageDirBetweenGroundAndHit = Quaternion.Slerp(upwardDirByTerrain, forceRotation, 0.5f);
                        // checking Quaternion for particular directions to validate if adding 180 degrees are needed, to correctly push foes based on groundDir and forceDir
                        Debug.LogWarning($"dotProduct {dotProduct}");

                        bool facingAwayButNotPerpendicular = (dotProduct > -0.95f && dotProduct < -0.7f || dotProduct > -0.4f && dotProduct < -0.02f);//(dotProduct > -0.95f && dotProduct < -0.02f);
                        bool facingSimilarDirectionNotAligned = (dotProduct > 0.02f && dotProduct < 0.43f);
                        bool almostFacingAwayButNotPerpendicular = (dotProduct < 0 && dotProduct > -0.23f);
                        if (facingAwayButNotPerpendicular || facingSimilarDirectionNotAligned || almostFacingAwayButNotPerpendicular) {
                            averageDirBetweenGroundAndHit *= Quaternion.AngleAxis(180, new Vector3(0, 0, 1));
                            Debug.LogWarning($"{facingAwayButNotPerpendicular} || {facingSimilarDirectionNotAligned} || {almostFacingAwayButNotPerpendicular}");
                        }
                        attackStats.ForceDir = averageDirBetweenGroundAndHit.normalized * (Vector2.one * 0.5f * foeCharacter.Stats.PushbackForce);
                    }
                    ApplyForce(foe, attackStats);
                    ApplyEffect(foe, Effect2DType.ImpactLight);
                    DealDamageAndNotifyAndDisconnectIfGrabbedIsDead(foe, attackStats.Damage, (IInteractableBody)attacker);
                    Handlers.GetHandler<ShakeHandler>().TriggerShake(foe.GetCharacterRigTransform(), new ShakeData(ShakeStyle.HorizontalCircular));

                } else if (properties.Contains(AttackProperty.PushUpOnHit) ||
                            properties.Contains(AttackProperty.PushUpOnBlock) && isFoeBlocking) {

                    if ((attackStats.ForceDir.normalized.y) < 0.75f) { // 0.75f is the threshold to account is angle that is not 45 angle
                        _debugDraw.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.yellow, 4);
                        attackStats.ForceDir.y = attacker.Stats.PushbackForce;
                    }

                    attackStats.ForceDir.x *= 0.05f;

                    //stun foe with animation cooldown - longer when foe is blocking
                    var foeCharacter = foe as Character2DController;
                    if (foeCharacter != null) {
                        var stunDuration = properties.Contains(AttackProperty.PushUpOnBlock) && isFoeBlocking ?
                            1.5f : 0.5f;
                        if (foeCharacter != null) {
                            ApplyStunEffectOnCharacter(foeCharacter, stunDuration);
                            foeCharacter.State.SetStunnedAnimation(Time.time + stunDuration);
                            foeCharacter.DoMove(0);
                        }
                    }

                    ApplyForce(foe, attackStats);
                    ApplyEffect(foe, Effect2DType.ImpactHeavy);
                    DealDamageAndNotifyAndDisconnectIfGrabbedIsDead(foe, attackStats.Damage, (IInteractableBody)attacker);
                    var curve = AnimationCurve.Linear(0, 0, 1, 0);
                    curve.AddKey(0.25f, 1);
                    Handlers.GetHandler<ShakeHandler>().TriggerShake(foe.GetCharacterRigTransform(), new ShakeData(ShakeStyle.VerticalCircular, 0.15f, 0.4f, curve));

                } else if (properties.Contains(AttackProperty.PushDiagonalOnHit) ||
                            properties.Contains(AttackProperty.PushDiagonalOnBlock) && isFoeBlocking) {

                    //_visualDebug.Log($"forceDirAction.y {forceDirAction.ForceDir.normalized.y} during PushDiagonal");

                    if (properties.Contains(AttackProperty.PushDiagonalOnBlock) && isFoeBlocking) {
                        ApplyEffect(foe, Effect2DType.ImpactOnBlock);
                        return facingRightOrLeftTowardsPoint;
                    }

                    //stun foe with animation cooldown
                    var foeCharacter = foe as Character2DController;
                    if (foeCharacter != null) {
                        ApplyStunEffectOnCharacter(foeCharacter, 0.35f);
                        foeCharacter.State.SetStunnedAnimation(Time.time + 0.35f);
                        foeCharacter.DoMove(0);
                    }

                    if ((attackStats.ForceDir.normalized.y) < 0.1f) {
                        _debugDraw.DrawRay(foe.GetCenterPosition(), attackStats.ForceDir, Color.yellow, 4);
                        attackStats.ForceDir.y = attacker.Stats.PushbackForce;
                    }
                    attackStats.ForceDir.x *= 0.5f;

                    ApplyForce(foe, attackStats);
                    ApplyEffect(foe, Effect2DType.ImpactMed);
                    DealDamageAndNotifyAndDisconnectIfGrabbedIsDead(foe, attackStats.Damage, attacker);
                    Handlers.GetHandler<ShakeHandler>().TriggerShake(foe.GetCharacterRigTransform(), new ShakeData(ShakeStyle.Random));

                }

            } else if (_logic.AbilitiesLogic.IsAbilityAGrab(attackAbility)) {

                var grabbingRopeLink = foe is RopeLinkController ? foe as RopeLinkController : null;

                //var abovePosition = new Vector3(moveToPosition.x, moveToPosition.y + attacker.LegsRadius, 0);
                var isGroundSmash = properties.Contains(AttackProperty.GrabToGroundSmash) ||
                    foe.GetCurrentState() is ObjectState.Jumping or ObjectState.Falling;
                var grabPointOffset = (isGroundSmash) ? Vector2.down : Vector2.up;
                var endPosition = (Vector2)attacker.GetCenterPosition() + grabPointOffset * (attacker.LegsRadius);
                var attackerAsInteractable = (IInteractableBody)attacker;

                //attacker.State.SetInterruptibleAnimation(Time.time + attackProperties.ActionCooldownDuration); // throw cooldown, might be duplicated in the PerformCombat

                var isThrowingGrabbedFoe = attackerAsInteractable.IsGrabbing() && foe.IsGrabbed();
                if (isThrowingGrabbedFoe) {

                    AttackData throwStats = new AttackData(attacker.Stats.Damage, Vector2.up * attacker.Stats.PushbackForce);

                    facingRightOrLeftTowardsPoint = attacker.State.GetFacingRightAsInt();
                    AlterThrowDirBasedOnNavigationDirection(ref throwStats.ForceDir, ref facingRightOrLeftTowardsPoint, attacker.State.NavigationDir, _gameplayLogic.IsStateConsideredAsAerial(attacker.State.CurrentState));
                    ApplyGrabOnFoe(foe, null, foe.GetTransform().position);
                    ApplyForce(foe, throwStats);
                    ApplyEffect(foe, Effect2DType.DustCloud);
                    attackerAsInteractable.SetAsGrabbing(null);
                    _ = ApplyProjectileStateWhenThrown(foe, throwStats.Damage, attackerAsInteractable);
                    Handlers.GetHandler<ShakeHandler>().TriggerShake(foe.GetCharacterRigTransform(), new ShakeData(ShakeStyle.Circular));

                    //_timelineEventManager.RegisterInteractionEvent(attacker, new IInteractableBody[] { foe }, Time.time, attacker.Stats.AttackDurationInMilliseconds, attackStats, attackAbility);

                } else {

                    AttackData grabActionStats = new AttackData(attacker.Stats.Damage, Vector2.up * attacker.Stats.PushbackForce);
                    
                    facingRightOrLeftTowardsPoint = attacker.State.GetFacingRightAsInt();

                    //foe.SetTargetPosition(endPosition);

                    _debugDraw.DrawRay(moveToPosition, Vector2.up * 10, Color.grey, 4);
                    _debugDraw.DrawRay(endPosition, Vector2.up * 10, Color.yellow, 4);

                    //_timelineEventManager.RegisterInteractionEvent(attacker, new IInteractableBody[] { foe }, Time.time, attacker.Stats.AttackDurationInMilliseconds, attackStats, attackAbility);

                    if (grabbingRopeLink != null) {

                        var rope = _charactersRopes.FirstOrDefault(); // Grabbing the closest rope link
                        rope?.HandleConnectInteractableBody(grabbingRopeLink, attacker);

                    } else if (isGroundSmash) {

                        foe.SetTargetPosition(endPosition);
                        ApplyGrabOnFoe(foe, attacker, endPosition);
                        attackerAsInteractable.SetAsGrabbing(foe);

                        ApplyGrabThrowWhenGrounded(foe, attacker, grabActionStats);

                    } else if (properties.Contains(AttackProperty.GrabAndPickUp)) {

                        foe.SetTargetPosition(endPosition);
                        ApplyGrabOnFoe(foe, attacker, endPosition);
                        attackerAsInteractable.SetAsGrabbing(foe);

                    } else {

                        var directionTowardsFoe = attacker.State.GetFacingRightAsInt();

                        if (properties.Contains(AttackProperty.StunOnGrabber)) {

                            grabActionStats.ForceDir.x = -directionTowardsFoe * 0.3f * Mathf.Abs(grabActionStats.ForceDir.y);
                            //ApplyGrabThrowWhenGrounded(foe, attacker, grabActionStats);

                        } else {
                            grabActionStats.ForceDir = HelperFunctions.RotateVector(grabActionStats.ForceDir, 60 * -directionTowardsFoe);
                        }

                        foe.GetTransform().transform.position = endPosition;
                        ApplyForce(foe, grabActionStats);
                        ApplyEffect(foe, Effect2DType.DustCloud);
                        attackerAsInteractable.SetAsGrabbing(null);
                        //_ = ApplyProjectileStateWhenThrown(foe, grabActionStats.Damage, attackerAsInteractable);
                    }

                }

                if (properties.Contains(AttackProperty.PenetrateDodge)) {

                }

                if (properties.Contains(AttackProperty.StunResist)) {

                }

            } else if (_logic.AbilitiesLogic.IsAbilityBlock(attackAbility)) {
                
                facingRightOrLeftTowardsPoint = attacker.State.GetFacingRightAsInt();
                /*// todo: stun foe with animation cooldown
                if (foe.GetCurrentState() is ObjectState.Attacking &&
                    properties.Contains(AttackProperty.StunAttacker)) {

                    AttackData parryActionStats = new AttackData(0, Vector2.up * attacker.Stats.PushbackForce);
                    foe.ApplyForce(parryActionStats.ForceDir);
                    //foe.SetTargetPosition(foe.GetCenterPosition());
                    ApplyEffect(foe, Effect2DType.Stunned);
                    var curve = AnimationCurve.Linear(0, 0, 1, 0);
                    curve.AddKey(0.25f, 1);
                    Handlers.GetHandler<ShakeHandler>().TriggerShake(foe.GetCharacterRigTransform(), new ShakeData(ShakeStyle.VerticalCircular, 1f, 0.3f, curve));
                    var foeCharacter = foe as Character2DController;
                    foeCharacter.State.SetMoveAnimation(Time.time + 1);
                }*/
            }

            return facingRightOrLeftTowardsPoint;
        }

        private void DealDamageAndNotifyAndDisconnectIfGrabbedIsDead(IInteractableBody foe, int damage, IInteractableBody attacker)
        {
            if (foe == null)
            {
                return;
            }

            var wasAlive = foe.GetHpPercent() > 0;
            if (IsInteractableBodyTheMainHero(foe))
            {
                SetCameraScreenShake();
            }

            foe.DealDamage(damage);
            var foeController = foe as Character2DController;
            if (foeController != null)
            {
                foeController.State.MarkLatestInteraction(attacker);
            }
            Debug.Log($"damage on {foe.GetTransform().gameObject.name} by {attacker.GetTransform().gameObject.name}");
            
            var interactableIsDead = foe.GetHpPercent() <= 0;

            if (interactableIsDead && wasAlive
                 // && attacker == _mainHero as IInteractableBody
                )
            {
                OnInteractableDestoyed?.Invoke(foe);
                if (IsInteractableBodyTheMainHero(foe))
                {
                    OnHeroDeath?.Invoke();
                }
            }

            if (interactableIsDead && foe.IsGrabbed())
            {
                attacker.SetAsGrabbing(null);
                ApplyGrabOnFoe(foe, null, foe.GetCenterPosition());
            }
        }

        public async void ApplySpecialAction(Character2DController character, bool isActive) {

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
                character.DealDamage(-10);
                ApplyEffectOnCharacter(character, Effect2DType.GainHp);
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
                    InvokeAimingAction(actionType, newPos, rotation, linePoints, hasHitData);

                } else {
                    InvokeAimingActionEnd(actionType);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(5));
            }

            InvokeAimingActionEnd(actionType);
        }

        public void InvokeAimingAction(ActionType actionType, Vector2 newPos, Quaternion rotation, Vector3[] linePoints, bool hasHitData) {
            OnAimingAction?.Invoke(actionType, newPos, rotation, hasHitData, linePoints);
        }

        public void InvokeAimingActionEnd(ActionType actionType) {
            OnAimingActionEnd?.Invoke(actionType);
        }

        List<Collider2D> damagees = new() ;
        private async Task ApplyProjectileStateWhenThrown(IInteractableBody grabbed, int damage, IInteractableBody thrower, Func<Task> actionOnEnd = null) {

            Debug.Log("trying as damager -> damage: " + damage);

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
            Debug.Log("Started as damager -> Mag: " + grabbedVelocity.magnitude);

            //var damagees = new List<Collider2D>() { thrower.GetCollider() };
            damagees.Clear();
            damagees.Add(thrower.GetCollider());

            var legsPosition = grabbed.GetCenterPosition();
            var grabbedColl = grabbed.GetCollider();
            var hasCircleColliderAsDamagerLayer = grabbedColl as CircleCollider2D;
            var legsRadius = hasCircleColliderAsDamagerLayer != null ? hasCircleColliderAsDamagerLayer.radius : 1;
            Vector2 objClosestPosition;
            Vector3 dirFromCharacterToFoe;
            RaycastHit2D blockingObjectsInAttackDirection;

            while (_gameRunning && grabbedVelocity.magnitude > 1) {

                //Debug.Log("as damager -> Mag: " + grabbedVelocity.magnitude);
                legsPosition = grabbed.GetCenterPosition();

                var solidOutRadiusObjectsColliders =
                Physics2D.OverlapCircleAll(legsPosition, legsRadius + 0.25f, _whatIsDamageable).
                    Where(collz => collz.gameObject != grabbedColl.gameObject && collz.gameObject != thrower.GetTransform().gameObject && !damagees.Contains(collz)).ToArray();

                foreach (var interactable in solidOutRadiusObjectsColliders)
                {

                    var damagee = interactable.GetComponent<IInteractable>();
                    if (damagee == null)
                    {
                        continue;
                    }

                    var interactableBody = damagee.GetInteractableBody();
                    if (interactableBody != null && !damagees.Contains(interactable)) {

                        objClosestPosition = interactable.ClosestPoint(legsPosition);
                        dirFromCharacterToFoe = (objClosestPosition - (Vector2)legsPosition).normalized;
                        blockingObjectsInAttackDirection = Physics2D.Raycast(legsPosition,
                    dirFromCharacterToFoe,
                    Vector2.Distance(objClosestPosition, legsPosition), _whatIsSolidForProjectile);

                        if (blockingObjectsInAttackDirection.collider != null) {
                            continue;
                        }

                        damagees.Add(interactable);
                        
                        if (interactableBody is Character2DController)
                        {
                            var isDodging = interactable.attachedRigidbody.isKinematic;
                            if (isDodging)
                            {
                                continue;
                            }
                            else if (interactableBody.GetCurrentState() is ObjectState.Blocking)
                            {
                                ApplyEffectOnCharacter((Character2DController)interactableBody, Effect2DType.ImpactOnBlock);
                                continue;
                            }

                            ApplyEffectOnCharacter((Character2DController)interactableBody, Effect2DType.ImpactMed);
                        }

                        var dir = Vector2.up;
                        var directionTowardsFoe = (objClosestPosition.x > legsPosition.x) ? 1 : -1;
                        dir.x = directionTowardsFoe;
                        var newPositionToSetOnFixedUpdate = objClosestPosition + (Vector2)dir.normalized * -(legsRadius);

                        interactableBody.ApplyForce(dir * grabbedVelocity * 0.7f); // 0.7f is an impact damage decrease

                        DealDamageAndNotifyAndDisconnectIfGrabbedIsDead(interactableBody, damage, thrower);

                        _debugDraw.DrawLine(objClosestPosition, newPositionToSetOnFixedUpdate, Color.white, 3);
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
            if (attack.Ability == AttackAbility.RollAttack) { Debug.Log($"roll attack data: {characterState.Velocity.magnitude}"); }

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
            Debug.Log($"checking matching alternative Attack for {attackAbility}");
            foreach (var condition in attack.CharacterStateConditions) {
                //Debug.Log($"checking alt condition {condition} is considered ? {characterState.Velocity.magnitude}");
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
                    case ObjectState.Alive when interactable.GetHpPercent() <= 0:
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

        private void ApplyForce(IInteractableBody damageableBody, AttackData attackStats, IInteractableBody attacker = null) {

            //foeRigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
                
            if (damageableBody == null) {
                return;
            }

            //foeRigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

            // disconnect any grabbing
            /*var isGrabbed = damageableBody.IsGrabbed();
            if (isGrabbed)
            {
                if (attacker != null && attacker.IsGrabbing())
                {
                    attacker.SetAsGrabbing(null);
                }
                ApplyGrabOnFoe(damageableBody, null, damageableBody.GetCenterPosition());
            }*/

            damageableBody.ApplyForce(attackStats.ForceDir);
            _debugDraw.DrawRay(damageableBody.GetCenterPosition(), attackStats.ForceDir, Color.red, 4);
        }

        public void ApplyEffect(IInteractableBody damageableBody, Effect2DType effectOnImpact = Effect2DType.None)
        {

            if (damageableBody == null || effectOnImpact == Effect2DType.None)
            {
                return;
            }

            var damageableCharacter = damageableBody as Character2DController;
            if (damageableCharacter != null)
            {
                ApplyEffectOnCharacter((Character2DController)damageableBody, effectOnImpact);
            }
            else
            {
                OnNewEffect?.Invoke(_gameplayLogic.GetEffectData(effectOnImpact), damageableBody.GetTransform(), Quaternion.identity);
            }
        }

        public void ApplyEffectOnCharacter(Character2DController character, Effect2DType effectType)
        {
            ApplyEffectOnCharacter(character, effectType, Quaternion.identity);
        }

        public void ApplyEffectOnCharacter(Character2DController character, Effect2DType effectType, Quaternion initialRotation)
        {
            var effectData = character.GetEffectOverride(effectType) ?? _gameplayLogic.GetEffectData(effectType);
            OnNewEffect?.Invoke(effectData, character.transform, initialRotation);
        }

        public void ApplyStunEffectOnCharacter(Character2DController character, float durationInSeconds = 0) {

            var effectData = _gameplayLogic.GetEffectData(Effect2DType.Stunned);
            effectData.DurationInSeconds = durationInSeconds;
            OnNewEffect?.Invoke(effectData, character.transform, Quaternion.identity);
        }

        private Vector2 AlterDirectionToMaxForce(Vector2 forceDirAction) {

            var xDirection = forceDirAction.x * 1.4f;
            var yDirection = forceDirAction.y * 0.55f;
            return new Vector2(xDirection, yDirection);
        }

        private void ApplyGrabOnFoe(IInteractableBody grabbed, IInteractableBody grabber, Vector2 position) {

            if (grabbed == null) {
                return;
            }

            grabbed.SetAsGrabbed(grabber, position);
        }

        private async void ApplyGrabThrowWhenGrounded(IInteractableBody grabbed, IInteractableBody grabber, AttackData attackStats) {

            //await Task.Delay(150);

            while (grabber.GetCurrentState() is not (ObjectState.Grounded or ObjectState.Running or ObjectState.Crouching or ObjectState.StandingUp)) {

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
                
            if (grabbed == null || !grabber.IsGrabbing()) {
                return;
            }
            
            grabber.SetAsGrabbing(null);
            ApplyGrabOnFoe(grabbed, null, grabbed.GetCenterPosition());
            ApplyForce(grabbed, attackStats);
            ApplyEffect(grabbed, Effect2DType.ImpactCritical);
            DealDamageAndNotifyAndDisconnectIfGrabbedIsDead(grabbed, attackStats.Damage, grabber); // was grabbed.DealDamage(attackStats.Damage);
            _ = ApplyProjectileStateWhenThrown(grabbed, attackStats.Damage, grabber);
            Handlers.GetHandler<ShakeHandler>().TriggerShake(grabbed.GetCharacterRigTransform(), new ShakeData(ShakeStyle.Random));
        }

    }

}
