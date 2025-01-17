using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Logic.GameFlow;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDetection {
    [DefaultExecutionOrder(11)]
    public class InputController : SingleController {

        public event Action<ActionInput> OnTouchPadDown;
        public event Action<ActionInput> OnTouchPadClick;
        public event Action<ActionInput> OnTouchPadUp;

        [SerializeField] private ScreenInputEventTriggers _eventTriggers;

        private readonly Dictionary<int, ActionInput> _touches = new();
#if UNITY_EDITOR
        private readonly Dictionary<int, ActionInput> _keyboardInputs = new();
        private ActionInput _keyboardInputNav;
        private ActionInput _keyboardInputAction;
        [Header("Keyboard input - pads position")]
        [Tooltip("For input action, Using transforms to set the starting position on the screen")]
        [SerializeField] private RectTransform MovementDefaultStartPosForKeyboard;
        [Tooltip("For input movement, Using transforms to set the starting position on the screen")]
        [SerializeField] private RectTransform ActionDefaultStartPosForKeyboard;
#endif

        private ActionInput _movementSingleTouchData;
        private ActionInput _actionSingleTouchData;

        private AbilitiesLogic _abilitiesLogic;

        protected override Task Initialize() {

            _movementSingleTouchData = _actionSingleTouchData = null;
            _abilitiesLogic = GetController<LogicController>().AbilitiesLogic;
            RegisterEvents();

            return Task.CompletedTask;
        }

        protected override void Clean() {

            DeregisterEvents();
        }

        private void RegisterEvents() {

            _eventTriggers.OnTouchPadDown += HandleTouchPadDown;
            _eventTriggers.OnTouchPadUp += HandleTouchPadUp;
        }

        private void DeregisterEvents() {

            _eventTriggers.OnTouchPadDown -= HandleTouchPadDown;
            _eventTriggers.OnTouchPadUp -= HandleTouchPadUp;
        }

        private void Update() {

            UpdateExistingPadTouches();
#if UNITY_EDITOR
            UpdateKeyboardInputs();
#endif
        }

        private void UpdateExistingPadTouches() {

            UpdateTouchPad(_movementSingleTouchData);
            UpdateTouchPad(_actionSingleTouchData);
        }

#if UNITY_EDITOR

        private const KeyCode JumpKey = KeyCode.Space;
        private const KeyCode SpecialKey = KeyCode.LeftShift;
        private const KeyCode AttackKey = KeyCode.X;
        private const KeyCode GrabKey = KeyCode.C;
        private const KeyCode BlockKey = KeyCode.V;
        private const KeyCode ProjectileKey = KeyCode.Z;
        private const KeyCode RopeKey = KeyCode.F;

        private void UpdateKeyboardInputs() {

            var previousInputNav = _keyboardInputNav;
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");

            if (x == 0 && y == 0 && previousInputNav != null) {
                OnTouchPadUp(_keyboardInputNav);
                _keyboardInputNav = null;

            } else if (x != 0 || y != 0) {

                var hasInputNavStarted = false;
                if (_keyboardInputNav == null) {
                    _keyboardInputNav = new ActionInput(Definitions.ActionType.Movement, Definitions.ActionTypeGroup.Navigation, GetScreenCoordinates(MovementDefaultStartPosForKeyboard).position, Time.time, new Vector2(0, 0));
                    hasInputNavStarted = true;
                }

                var newX = _keyboardInputNav.StartPosition.x + (x != 0 ? Mathf.Sign(x) * 10 : 0);
                var newY = _keyboardInputNav.StartPosition.y + (y != 0 ? Mathf.Sign(y) * 10 : 0);
                var endPosition = new Vector2(newX, newY);
                _keyboardInputNav.UpdateSwipe(endPosition, Time.time);

                if (hasInputNavStarted) {
                    OnTouchPadDown(_keyboardInputNav);
                } else {
                    OnTouchPadClick(_keyboardInputNav);
                }

            }


            var previousInputAction = _keyboardInputAction;
            var isActionKeyReleased = Input.GetKeyUp(JumpKey) || Input.GetKeyUp(SpecialKey) || Input.GetKeyUp(AttackKey) || Input.GetKeyUp(GrabKey) || Input.GetKeyUp(BlockKey) || Input.GetKeyUp(ProjectileKey) || Input.GetKeyUp(RopeKey);

            if (isActionKeyReleased && previousInputAction != null) {
                OnTouchPadUp(_keyboardInputAction);
                _keyboardInputAction = null;
            }

            var isNewActionAvailable = _keyboardInputAction == null;

            if (Input.GetKeyDown(JumpKey) && isNewActionAvailable) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Jump, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);

            } else if (Input.GetKey(JumpKey) && !isNewActionAvailable) {
                _keyboardInputAction ??= new ActionInput(Definitions.ActionType.Jump, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                UpdateKeyboardInputActionByInputNav(x, y);

                OnTouchPadClick(_keyboardInputAction);
            }

            if (Input.GetKeyDown(SpecialKey) && isNewActionAvailable) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Special, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);

            } else if (Input.GetKey(SpecialKey) && !isNewActionAvailable) {
                _keyboardInputAction ??= new ActionInput(Definitions.ActionType.Special, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadClick(_keyboardInputAction);
            }

            if (Input.GetKeyDown(AttackKey) && isNewActionAvailable) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Attack, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);

            } else if (Input.GetKey(AttackKey) && !isNewActionAvailable) {
                _keyboardInputAction ??= new ActionInput(Definitions.ActionType.Attack, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadClick(_keyboardInputAction);
            }

            if (Input.GetKeyDown(GrabKey) && isNewActionAvailable) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Grab, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);

            } else if (Input.GetKey(GrabKey) && !isNewActionAvailable) {
                _keyboardInputAction ??= new ActionInput(Definitions.ActionType.Grab, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadClick(_keyboardInputAction);
            }

            if (Input.GetKeyDown(BlockKey) && isNewActionAvailable) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Block, Definitions.ActionTypeGroup.Action, GetScreenCoordinates(ActionDefaultStartPosForKeyboard).position, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);

            } else if (Input.GetKey(BlockKey) && !isNewActionAvailable) {
                _keyboardInputAction ??= new ActionInput(Definitions.ActionType.Block, Definitions.ActionTypeGroup.Action, GetScreenCoordinates(ActionDefaultStartPosForKeyboard).position, Time.time, new Vector2(0, 0));
                UpdateKeyboardInputActionByInputNav(x, y);

                OnTouchPadClick(_keyboardInputAction);
            }

            if (Input.GetKeyDown(ProjectileKey) && isNewActionAvailable) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Projectile, Definitions.ActionTypeGroup.Action, GetScreenCoordinates(ActionDefaultStartPosForKeyboard).position, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);

            } else if (Input.GetKey(ProjectileKey) && !isNewActionAvailable) {
                _keyboardInputAction ??= new ActionInput(Definitions.ActionType.Projectile, Definitions.ActionTypeGroup.Action, GetScreenCoordinates(ActionDefaultStartPosForKeyboard).position, Time.time, new Vector2(0, 0));
                UpdateKeyboardInputActionByInputNav(x, y);

                OnTouchPadClick(_keyboardInputAction);
            }
            
            if (Input.GetKeyDown(RopeKey) && isNewActionAvailable) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Rope, Definitions.ActionTypeGroup.Action, GetScreenCoordinates(ActionDefaultStartPosForKeyboard).position, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);

            } else if (Input.GetKey(RopeKey) && !isNewActionAvailable) {
                _keyboardInputAction ??= new ActionInput(Definitions.ActionType.Rope, Definitions.ActionTypeGroup.Action, GetScreenCoordinates(ActionDefaultStartPosForKeyboard).position, Time.time, new Vector2(0, 0));
                UpdateKeyboardInputActionByInputNav(x, y);

                OnTouchPadClick(_keyboardInputAction);
            }

        }

        private void UpdateKeyboardInputActionByInputNav(float x, float y) {
            var newX = _keyboardInputAction.StartPosition.x + (x != 0 ? Mathf.Sign(x) * 10 : 0);
            var newY = _keyboardInputAction.StartPosition.y + (y != 0 ? Mathf.Sign(y) * 10 : 0);
            var endPosition = new Vector2(newX, newY);
            _keyboardInputAction.UpdateSwipe(endPosition, Time.time);
        }

        Camera cam;

        private Rect GetScreenCoordinates(RectTransform uiElement) {

            cam ??= Camera.main;

            var screenPos1 = cam.WorldToScreenPoint(uiElement.position);
            var screenPos2 = cam.WorldToScreenPoint(uiElement.position + new Vector3(uiElement.rect.width, uiElement.rect.height, 0));

            // Calculate the screen space rectangle
            var result = new Rect(screenPos1.x, screenPos1.y, screenPos2.x - screenPos1.x, screenPos2.y - screenPos1.y);

            return result;
        }
#endif

        private void HandleTouchPadDown(Definitions.ActionType actionType) {
            
            if (TryAddTouch(actionType, out var touch)) {
                OnTouchPadDown?.Invoke(touch);
            }
        }

        private void UpdateTouchPad(ActionInput padSingleTouchData) {
            
            if (padSingleTouchData == null) {
                return;
            }
           
            var wasKilled = false;
            if (TryUpdateTouch(padSingleTouchData.ActionType, out var touch, false, ref wasKilled)) {
                if (!wasKilled) {
                    OnTouchPadClick?.Invoke(touch);
                    
                } else {
                    OnTouchPadUp?.Invoke(touch);
                }
            }
        }

        private void HandleTouchPadUp(Definitions.ActionType actionType) {
            var wasKilled = false;
            if (TryUpdateTouch(actionType, out var touch, true, ref wasKilled)) {
                OnTouchPadUp?.Invoke(touch);
            }
        }

        private bool TryAddTouch(Definitions.ActionType actionType, out ActionInput newTouchData) {

            var touches = Input.touches;

            foreach (var touch in touches) {
                
                 if (touch.phase is TouchPhase.Began &&
                    IsAvailableForSingleTouch(actionType) &&
                    !_touches.ContainsKey(touch.fingerId)) {
                     
                    _touches[touch.fingerId] = GetPadTouchData(actionType, touch.position);

                    newTouchData = _touches[touch.fingerId];
                    
                    return LockSingleTouch(_touches[touch.fingerId]);
                 }
            }
            
            newTouchData = null;
            return false;
        }

        private bool TryUpdateTouch(Definitions.ActionType actionType, out ActionInput newTouchData, bool killAfterUpdate, ref bool wasKilled) {

            var touches = Input.touches;
            foreach (var touch in touches) {
                
                // todo: check if adding the condition of touch.phase is TouchPhase.Began, solves the quick min jump delay bug
                if ( (killAfterUpdate || touch.phase is TouchPhase.Ended ||
                       !killAfterUpdate && touch.phase is TouchPhase.Stationary or TouchPhase.Moved or TouchPhase.Began) &&
                    _touches.ContainsKey(touch.fingerId)) {

                    if (_touches[touch.fingerId].ActionType != actionType) {
                        //Debug.LogError($"Touch {_touches[touch.fingerId].PadType} was supposed to be {padType}, but is not");
                        continue;
                    }
                    
                    if(_touches[touch.fingerId].UpdateSwipe(touch.position, Time.time)) {
                        //Debug.Log($"swipe detected, dir {_touches[touch.fingerId].NormalizedDirection}");
                    }

                    newTouchData = _touches[touch.fingerId];

                    if (!killAfterUpdate) {
                        wasKilled = false; //touch.phase is TouchPhase.Ended;
                        return true;
                    }
                    
                    var success = UnlockSingleTouch(_touches[touch.fingerId]);
                    _touches.Remove(touch.fingerId);

                    wasKilled = true;
                    return success;
                }
            }

            newTouchData = null;
            return false;
        }

        private ActionInput GetPadTouchData(Definitions.ActionType actionType, Vector2 inputPosition) 
        {
            var minMaxSwipeDistance = _abilitiesLogic.GetSwipeDistanceForActionPad(actionType);
            var minSwipeVelocity = _abilitiesLogic.GetSwipeVelocityForActionPad(actionType);
            var actionGroupType = _abilitiesLogic.GetActionTypeGroup(actionType);
            return new ActionInput(actionType, actionGroupType, inputPosition, Time.time, minMaxSwipeDistance, minSwipeVelocity);
        }

        private ActionInput GetSingleTouchByPadType(Definitions.ActionTypeGroup groupType) 
            => groupType == Definitions.ActionTypeGroup.Navigation ? ref _movementSingleTouchData : ref _actionSingleTouchData;

        private bool LockSingleTouch(ActionInput touchData) {

            if (GetSingleTouchByPadType(touchData.ActionGroupType) != null) {
                return false;
            }

            if (touchData.ActionGroupType == Definitions.ActionTypeGroup.Navigation) {    
                _movementSingleTouchData = touchData;
            } else {
                _actionSingleTouchData = touchData;
            }

            return true;
        }

        private bool UnlockSingleTouch(ActionInput touchData) {
            
            var singleScreen = GetSingleTouchByPadType(touchData.ActionGroupType);

            if (singleScreen?.ActionType != touchData.ActionType) {
                return false;
            }

            if (touchData.ActionGroupType == Definitions.ActionTypeGroup.Navigation) {        
                _movementSingleTouchData = null;
            } else {
                _actionSingleTouchData = null;
            }

            return true;
        }

        private bool IsAvailableForSingleTouch(Definitions.ActionType actionType) =>
            GetSingleTouchByPadType(actionType) == null;

        private ActionInput GetSingleTouchByPadType(Definitions.ActionType actionType) {
            return _abilitiesLogic.IsTypeValidAsMovementAndNotActionPad(actionType) ? ref _movementSingleTouchData : ref _actionSingleTouchData;
        }
    }

}
