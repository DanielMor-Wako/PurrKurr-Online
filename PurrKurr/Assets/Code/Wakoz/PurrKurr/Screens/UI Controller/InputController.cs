using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Logic.GameFlow;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller {
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
        private void UpdateKeyboardInputs() {

            var previousInputNav = _keyboardInputNav;
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxis("Vertical");

            if (x == 0 && previousInputNav != null) {
                OnTouchPadUp(_keyboardInputNav);
                _keyboardInputNav = null;

            } else if (x != 0 || y != 0) {
                _keyboardInputNav = new ActionInput(Definitions.ActionType.Movement, Definitions.ActionTypeGroup.Navigation, Vector2.zero, Time.time, new Vector2(0, 0));
                var newX = _keyboardInputNav.StartPosition.x + (x != 0 ? Mathf.Sign(x) * 10 : 0);
                var newY = _keyboardInputNav.StartPosition.y + (y != 0 ? Mathf.Sign(y) * 10 : 0);
                var endPosition = new Vector2(newX , newY);
                _keyboardInputNav.UpdateSwipe(endPosition, Time.time);
                OnTouchPadDown(_keyboardInputNav);
            }

            var previousInputAction = _keyboardInputAction;

            if (Input.GetKeyUp(KeyCode.Space) && previousInputAction != null) {
                OnTouchPadUp(_keyboardInputAction);
                _keyboardInputAction = null;

            } else if (Input.GetKeyDown(KeyCode.Space)) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Jump, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);
            }

            if (Input.GetKeyUp(KeyCode.X) && previousInputAction != null) {
                OnTouchPadUp(_keyboardInputAction);
                _keyboardInputAction = null;

            } else if (Input.GetKeyDown(KeyCode.X)) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Attack, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);
            }

            if (Input.GetKeyUp(KeyCode.C) && previousInputAction != null) {
                OnTouchPadUp(_keyboardInputAction);
                _keyboardInputAction = null;

            } else if (Input.GetKeyDown(KeyCode.C)) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Block, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);
            }
            
            if (Input.GetKeyUp(KeyCode.V) && previousInputAction != null) {
                OnTouchPadUp(_keyboardInputAction);
                _keyboardInputAction = null;

            } else if (Input.GetKeyDown(KeyCode.V)) {
                _keyboardInputAction = new ActionInput(Definitions.ActionType.Grab, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, new Vector2(0, 0));
                OnTouchPadDown(_keyboardInputAction);
            }
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

            var touches = GetTouches();

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

            var touches = GetTouches();
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

        private IEnumerable<Touch> GetTouches() {
            
            var retVal = new List<Touch>();
            for(var i = 0; i < Input.touchCount; ++i) {
                retVal.Add(Input.GetTouch(i));
            }
            return retVal;
        }

        private ActionInput GetPadTouchData(Definitions.ActionType actionType, Vector2 inputPosition) {
            
            // todo: combine the two calls to abilities logic into a single call, might combine them into PadTouchConfig, PadTouch
            var minMaxSwipeDistance = _abilitiesLogic.GetSwipeDistanceForActionPad(actionType);
            var minSwipeVelocity = _abilitiesLogic.GetSwipeVelocityForActionPad(actionType);
            var actionGroupType = _abilitiesLogic.GetActionTypeGroup(actionType);
            return new ActionInput(actionType, actionGroupType, inputPosition, Time.time, minMaxSwipeDistance, minSwipeVelocity);
        }

        private ActionInput GetSingleTouchByPadType(Definitions.ActionTypeGroup groupType) {
            return groupType == Definitions.ActionTypeGroup.Navigation ? ref _movementSingleTouchData : ref _actionSingleTouchData;
        } 

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
