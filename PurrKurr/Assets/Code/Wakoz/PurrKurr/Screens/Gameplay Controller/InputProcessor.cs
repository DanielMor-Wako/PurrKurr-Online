using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDetection;
using System;
using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller
{
    public sealed class InputProcessor : IDisposable
    {
        public event Action<Character2DController, ActionInput, bool, bool> OnNavigationAction;
        public event Action<Character2DController, ActionInput, bool, bool> OnAbilityAction;

        private GameplayController _gameEvents;
        private InputController _input;
        private InputValidator _validator;

        private Character2DController _mainCharacter;
        
        private ExecutableAction _currentAction;
        private readonly Queue<ExecutableAction> _actionQueue;
        private const int MaxQueueSize = 2; // Current + 1 buffered

        public InputProcessor(GameplayController controller, InputController input) {

            _gameEvents = controller ?? throw new ArgumentNullException(nameof(GameplayController));
            
            _gameEvents.OnNewHero += SetMainHero;

            _input = input ?? throw new ArgumentNullException(nameof(InputController));

            _input.OnTouchPadDown += OnActionStarted;
            _input.OnTouchPadClick += OnActionOngoing;
            _input.OnTouchPadUp += OnActionEnded;
            
            _validator = new InputValidator();
            _currentAction = new ExecutableAction();
            _actionQueue = new();
        }

        public void Dispose() {

            if (_gameEvents != null) {
                
                _gameEvents.OnNewHero -= SetMainHero;
                _gameEvents = null;
            }

            if (_input != null) {

                _input.OnTouchPadDown -= OnActionStarted;
                _input.OnTouchPadClick -= OnActionOngoing;
                _input.OnTouchPadUp -= OnActionEnded;

                _input = null;
            }

            if (_validator != null) {

                _validator?.Dispose();
                _validator = null;
            }

            _actionQueue.Clear();
            _currentAction = null;
        }

        private void SetMainHero(Character2DController newHero) {

            _mainCharacter = newHero ?? throw new ArgumentNullException(nameof(Character2DController));
        }

        public ExecutableAction GetCurrentAction() => _currentAction;

        private void OnActionStarted(ActionInput actionInput) {

            if (_mainCharacter == null || actionInput == null) 
                return;

            HandleNavigation(actionInput, true, false);
            HandleAbilityAction(actionInput, true, false);

            _gameEvents.InvokeTouchPadDown(actionInput);
        }

        private void OnActionOngoing(ActionInput actionInput) {

            if (_mainCharacter == null || actionInput == null) 
                return;

            HandleNavigation(actionInput, false, false);

            _gameEvents.InvokeTouchPadClick(actionInput);
        }

        private void OnActionEnded(ActionInput actionInput) {

            if (_mainCharacter == null || actionInput == null) 
                return;

            HandleNavigation(actionInput, false, true);
            HandleAbilityAction(actionInput, false, true);

            _gameEvents.InvokeTouchPadUp(actionInput);
        }

        private void HandleNavigation(ActionInput navigationInput, bool started, bool ended) {
            
            if (_validator.ValidateNavigation(navigationInput, started, ended)) {
                OnNavigationAction?.Invoke(_mainCharacter, navigationInput, started, ended);
            }

            _currentAction.NavigationInput = navigationInput;
        }

        private void HandleAbilityAction(ActionInput actionInput, bool started, bool ended) {
            
            if (_validator.ValidateAction(actionInput, started, ended)) {
                OnAbilityAction?.Invoke(_mainCharacter, actionInput, started, ended);
            }

            _currentAction.ActionInput = actionInput;
        }
    }
}