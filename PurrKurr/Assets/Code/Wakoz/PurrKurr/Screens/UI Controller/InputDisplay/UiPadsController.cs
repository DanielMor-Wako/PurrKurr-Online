using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDetection;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay {
    [DefaultExecutionOrder(12)]
    public class UiPadsController : SingleController {

        [SerializeField] private bool _onlyUpdateItemsThatHaveChanges = true;
        [SerializeField] private bool _onlyUpdateItemWhenStateChanged = true;
        [SerializeField] private UiPadsView _view;
        [SerializeField] private bool _testScreenInputs;
        
        private UiPadsModel _model;
        
        private InputController _screenInput;
        private GameplayController _playerInput;

        protected override Task Initialize() {
            
            if (_testScreenInputs) {
                RegisterScreenEvents();
            }
            
            return Task.CompletedTask;
        }
        
        protected override void Clean() {
            
            DeregisterScreenEvents();
            DeregisterCharacterEvents();
        }

        public void Init(Character2DController hero = null) {

            _model = new UiPadsModel(hero, _onlyUpdateItemsThatHaveChanges, _onlyUpdateItemWhenStateChanged);

            if (hero?.Stats != null) {
                
                DefineDependents(hero.Stats, out var downKeyDependents, out var upKeyDependents, out var characterStateDependents);

                _model.SetDependents(downKeyDependents, upKeyDependents, characterStateDependents);
            }

            _view.SetModel(_model);
        }
        
        public bool TryBindToCharacter(GameplayController character) {

            if (character == null) {
                Debug.LogError("Touch display has no available character events for input");
                return false;
            }

            if (_screenInput != null) {
                DeregisterScreenEvents();
            }

            DeregisterCharacterEvents();

            _playerInput = character;

            return RegisterCharacterEvents(character);
        }
        

        private void DefineDependents(CharacterStats stats, 
            out List<Definitions.ActionType> downKeyDependents, out List<Definitions.ActionType> upKeyDependents, out List<Definitions.ActionType> characterStateDependents) {

            downKeyDependents = new List<Definitions.ActionType>();
            upKeyDependents = new List<Definitions.ActionType>();
            characterStateDependents = new List<Definitions.ActionType>();

            var attacks = stats.GetAttacks();
            var attackProperty = new AttackBaseStats();

            foreach (var attack in attacks) {

                if (!stats.GetAttackProperties(attack, ref attackProperty)) {
                    continue;
                }

                if (attackProperty.CharacterStateConditions.Contains(Definitions.ObjectState.Crouching)) {
                    downKeyDependents.Add(_model.GetAttackMoveAsAbility(attackProperty.Ability));
                
                } else if (attackProperty.CharacterStateConditions.Contains(Definitions.ObjectState.StandingUp)) {
                    upKeyDependents.Add(_model.GetAttackMoveAsAbility(attackProperty.Ability));
                }

                if (attackProperty.CharacterStateConditions.Contains(Definitions.ObjectState.Jumping)) {
                    characterStateDependents.Add(_model.GetAttackMoveAsAbility(attackProperty.Ability));
                }
            }

            
            var abilities = stats.GetAbilities();

            foreach (var ability in abilities) {

                if (ability == Definitions.CharacterAbility.AimJump) {
                    downKeyDependents.Add(Definitions.ActionType.Jump);
                }
            }
        }

        private bool RegisterCharacterEvents(GameplayController character) {

            _playerInput.OnTouchPadDown += OnPadDown;
            _playerInput.OnTouchPadClick += OnPadClicked;
            _playerInput.OnTouchPadUp += OnPadUp;
            _playerInput.OnStateChanged += OnStateChanged;
            
            return true;
        }


        private void OnStateChanged(InteractableObject2DState state) {
            
            var stateDependents = _model.RefreshStateDependentPads(state);
            foreach (var i in stateDependents) {
                // todo: cache the related action input
                var relatedAtionInput = new ActionInput(i, Definitions.ActionTypeGroup.Action, Vector2.zero, Time.time, Vector2.zero, 0);
                OnPadClicked(relatedAtionInput);
            }
            
        }

        private void DeregisterCharacterEvents() {

            if (_playerInput == null) {
                return;
            }
            
            _playerInput.OnTouchPadDown -= OnPadDown;
            _playerInput.OnTouchPadClick -= OnPadClicked;
            _playerInput.OnTouchPadUp -= OnPadUp;
            _playerInput.OnStateChanged -= OnStateChanged;

            _playerInput = null;
        }
        
        private void RegisterScreenEvents() {

            if (_screenInput != null) {
                return;
            }
            
            _screenInput = SingleController.GetController<InputController>();

            if (_screenInput == null) {
                Debug.LogError("Touch display has no available screen events for input");
                return;
            }
            
            _screenInput.OnTouchPadDown += OnPadDown;
            _screenInput.OnTouchPadClick += OnPadClicked;
            _screenInput.OnTouchPadUp += OnPadUp;
        }

        private void DeregisterScreenEvents() {

            if (_screenInput == null) {
                return;
            }
            
            _screenInput.OnTouchPadDown -= OnPadDown;
            _screenInput.OnTouchPadClick -= OnPadClicked;
            _screenInput.OnTouchPadUp -= OnPadUp;

            _screenInput = null;
        }

        private void OnPadDown(ActionInput touchData) {
            _model.ActivatePad(touchData);
        }

        private List<ActionInput> TouchesDelayedUpdate;
        private void OnPadClicked(ActionInput touchData) {
            TouchesDelayedUpdate ??= new List<ActionInput>();
            if (!TouchesDelayedUpdate.Contains(touchData)) {
                TouchesDelayedUpdate.Add(touchData);
            }
        }

        private void FixedUpdate() {
            
            TouchesDelayedUpdate ??= new List<ActionInput>();
            if (TouchesDelayedUpdate.Count == 0) {
                return;
            }
            
            foreach (var touch in TouchesDelayedUpdate) {
                _model.UpdatePad(touch);
            }
            TouchesDelayedUpdate.Clear();
        }

        private void OnPadUp(ActionInput touchData) {
            _model.DeactivatePad(touchData);
        }
        
        public void SetCooldown(Definitions.ActionType actionType, float percentage) {
            _model.SetCooldownForPadType(actionType, percentage, false);
        }
        
        #region UI Graphic Settings
        private void SetGraphicsUsingDirtyPads(bool isActive) {
            Debug.Log($"Using: {(isActive ? "Smart" : "Stupid")} graphic display update");
            _onlyUpdateItemsThatHaveChanges = isActive;

            if (_model == null || _view == null) {
                return;
            }

            _model.SetUsingDirtyPads(isActive);
        }
        
        [ContextMenu("Use Smart Update Display - Only update interacted pads and relevant pads")]
        public void UseSmartUpdateDisplay() {
            SetGraphicsUsingDirtyPads(true);
        }
        
        [ContextMenu("Use Stupid Update Display - Update all pads continuously")]
        public void UseStupidUpdateDisplay() {
            SetGraphicsUsingDirtyPads(false);
        }
        
        private void UpdateViewWhenStateChanged(bool isActive) {
            Debug.Log($"Using: Update item View - "+(isActive ? "Only when state changed" : "Always update"));
            _onlyUpdateItemWhenStateChanged = isActive;

            if (_model == null || _view == null) {
                return;
            }

            _model.UpdateViewWhenStateChanged(isActive);
        }
        
        [ContextMenu("Only Updates the display view of items that new changes")]
        public void UpdateViewOnlyWhenStateChanged() {
            UpdateViewWhenStateChanged(true);
        }
        
        [ContextMenu("Keep refreshing the display view regardless of changes")]
        public void UpdateViewRegardlessWhenStateChanged() {
            UpdateViewWhenStateChanged(false);
        }
        #endregion
        
    }
}