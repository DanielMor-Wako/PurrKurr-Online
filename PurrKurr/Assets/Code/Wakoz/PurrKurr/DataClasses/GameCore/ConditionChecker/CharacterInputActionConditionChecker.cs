using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.ConditionChecker
{
    public class CharacterInputActionConditionChecker : CharacterConditionChecker<ActionInputMatcher>
    {
        /// <summary>
        /// Checks for the input of the player, navigation and action
        /// Todo: Currently the binding to gameEvents occur with:
        ///     UpdateCurrentValue for binding to events, and HasMetCondition to deregister
        ///     Eventuallt this class should derive from a base class that has different implementation for checking conditions that is event-based
        /// </summary>

        private GameplayController _inputEvents;
        private bool _isConditionsMet, _isNavigationConditionMet, _isActionConditionMet;
        
        private InputLogic _logic;

        public override bool HasMetCondition()
        {
            if (_isConditionsMet)
            {
                if (_inputEvents != null)
                {
                    _inputEvents.OnTouchPadClick -= HandleInput;
                    _inputEvents = null;
                    _isConditionsMet = _isNavigationConditionMet = _isActionConditionMet = false;
                    return true;
                }
            }
            return _isConditionsMet;
        }

        public override void UpdateCurrentValue()
        {
            if (_inputEvents == null)
            {
                _inputEvents = SingleController.GetController<GameplayController>();
                if (_inputEvents != null )
                {
                    _inputEvents.OnTouchPadClick += HandleInput;
                }
            }
        }

        private void HandleInput(ActionInput input)
        {
            if (_isConditionsMet)
            {
                return;
            }

            HandleNavigationInput(input.ActionType, input.NormalizedDirection);
            HandleActionInput(input.ActionType, input.NormalizedDirection);

            _isConditionsMet = _isNavigationConditionMet && _isActionConditionMet;
        }

        private void HandleNavigationInput(Definitions.ActionType actionType, Vector2 normalizedDirection)
        {
            if (!ValueToMatch.respondToNavigation)
            {
                _isNavigationConditionMet = true;
                return;
            }

            if (actionType == Definitions.ActionType.Movement)
            {
                _logic ??= SingleController.GetController<LogicController>().InputLogic;
                if (_logic.GetInputDirection(normalizedDirection) == ValueToMatch.navType)
                {
                    _isNavigationConditionMet = true;
                }
            }
        }

        private void HandleActionInput(Definitions.ActionType actionType, Vector2 normalizedDirection)
        {
            if (!ValueToMatch.respondToAction)
            {
                _isActionConditionMet = true;
                return;
            }

            if (actionType == ValueToMatch.action)
            {
                if (normalizedDirection.x == ValueToMatch.xActionDirection &&
                    normalizedDirection.y == ValueToMatch.yActionDirection)
                {
                    _isActionConditionMet = true;
                }
            }
        }
    }

    [Serializable]
    public class ActionInputMatcher
    {
        [Header("> Navigation Configuration")]
        [Space(10)]

        public bool respondToNavigation = true;
        public Definitions.NavigationType navType;

        [Header("> Action Configuration")]
        [Space(10)]
        
        [Tooltip("When true, it includes the action config for mathcing value\nWhen false, it ignore action")]
        public bool respondToAction = true;
        public Definitions.ActionType action;
        [Range(-1, 1)] public float xActionDirection = 0;
        [Range(-1, 1)] public float yActionDirection = 0;
    }
}