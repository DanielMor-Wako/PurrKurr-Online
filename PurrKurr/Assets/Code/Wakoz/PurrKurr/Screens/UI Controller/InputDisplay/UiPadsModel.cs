using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.Logic.GameFlow;
using Code.Wakoz.PurrKurr.DataClasses.Characters;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay {

    public class UiPadsModel : Model {

        public bool OnlyUpdateDirtyPadsDisplay { get; private set; }
        public bool OnlyUpdateViewWhenStateChanged { get; private set; }
        
        private readonly IEnumerable<Definitions.ActionType> _availableActions;
        public UiPadsData Config { get; private set; }
        private readonly AbilitiesLogic _abilitiesLogic;
        private readonly InputLogic _inputLogic;
        private readonly GameplayLogic _gameplayLogic;
        private InteractableObject2DState _state;

        public bool IsMovementPad { get; private set; }
        public bool IsJumpPad { get; private set; }
        public bool IsAttackPad { get; private set; }
        public bool IsBlockPad { get; private set; }
        public bool IsGrabPad { get; private set; }
        public bool IsProjectilePad { get; private set; }
        public bool IsRopePad { get; private set; }
        public bool IsSupurrPad { get; private set; }
        
        public Vector2 MovementPadStartPosition { get; private set; }
        public Vector2 MovementPadDirection { get; private set; }
        
        public bool IsAimPad { get; private set; }
        public Vector2 ActionPadStartPosition { get; private set; }
        public Vector2 ActionPadAimDirection { get; private set; }

        private Dictionary<Definitions.ActionType, float> _padsCooldown = new();
        private List<Definitions.ActionType> _dirtyPads = new();
        private List<Definitions.ActionType> _cleanPads = new();

        private List<Definitions.ActionType> _downKeyDependents;
        private List<Definitions.ActionType> _upKeyDependents;
        private List<Definitions.ActionType> _downUpDependentsList;
        private List<Definitions.ActionType> _characterStateDependents;

        public UiPadsModel(Character2DController hero,
            bool onlyUpdateDirtyPadsDisplay = false, bool onlyUpdateViewWhenStateChanged = false) {

            var logic = SingleController.GetController<LogicController>();
            _inputLogic = logic.InputLogic;
            _abilitiesLogic = logic.AbilitiesLogic;
            _gameplayLogic = logic.GameplayLogic;

            _state = hero?.State;
            var stats = hero?.Stats;
            var playerAbilities = new List<Definitions.ActionType>();

            if (stats != null) {
                playerAbilities = stats.GetActions();
            }
            if (playerAbilities.Count == 0) {
                playerAbilities = new List<Definitions.ActionType>() {
                    Definitions.ActionType.Movement,
                    Definitions.ActionType.Jump,
                    Definitions.ActionType.Attack,
                    Definitions.ActionType.Block,
                    Definitions.ActionType.Grab,
                    Definitions.ActionType.Projectile,
                    Definitions.ActionType.Rope,
                    Definitions.ActionType.Special
                };
            }
            _availableActions = playerAbilities;
            Config = _abilitiesLogic.GetTouchPadsConfig(_availableActions);
            
            _downKeyDependents = new List<Definitions.ActionType>();
            _upKeyDependents = new List<Definitions.ActionType>();
            _downUpDependentsList = new List<Definitions.ActionType>();
            _characterStateDependents = new List<Definitions.ActionType>();

            OnlyUpdateDirtyPadsDisplay = onlyUpdateDirtyPadsDisplay;
            OnlyUpdateViewWhenStateChanged = onlyUpdateViewWhenStateChanged;

            if (onlyUpdateDirtyPadsDisplay) {
                RefreshAllPadOnInitialization();
            }
        }

        public void ActivatePad(ActionInput padType) => SetPadActiveState(padType, true);
        public void UpdatePad(ActionInput padType) => UpdatePadState(padType);
        public void DeactivatePad(ActionInput padType) => SetPadActiveState(padType, false);
        public List<Definitions.ActionType> RefreshStateDependentPads(InteractableObject2DState state) {

            _state = state;
            var refreshedDependents = new List<Definitions.ActionType>();

            foreach (Definitions.ActionType actionType in _characterStateDependents) {
                if (actionType == Definitions.ActionType.Empty) {
                    continue;
                }
                AddToCleanPads(actionType);
                refreshedDependents.Add(actionType);
            }

            return refreshedDependents;
        }
        public void RefreshAllPadOnInitialization() {
            
            var allEnums = Enum.GetValues(typeof(Definitions.ActionType));

            foreach (Definitions.ActionType actionType in allEnums) {
                if (actionType == Definitions.ActionType.Empty) {
                    continue;
                }
                AddToCleanPads(actionType);
            }
            
        }
        
        private void SetPadActiveState(ActionInput pad, bool activeState) {

            switch (pad.ActionType) {
                case Definitions.ActionType.Movement:
                    IsMovementPad = activeState;
                    break;

                case Definitions.ActionType.Jump:
                    IsJumpPad = activeState;
                    break;

                case Definitions.ActionType.Attack:
                    IsAttackPad = activeState;
                    break;

                case Definitions.ActionType.Block:
                    IsBlockPad = activeState;
                    break;

                case Definitions.ActionType.Grab:
                    IsGrabPad = activeState;
                    break;

                case Definitions.ActionType.Projectile:
                    IsProjectilePad = activeState;
                    break;

                case Definitions.ActionType.Rope:
                    IsRopePad = activeState;
                    break;

                case Definitions.ActionType.Special:
                    IsSupurrPad = activeState;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(pad), pad, null);
            }
            
            ManagePadsToUpdate(pad.ActionType, activeState);
            
            IsAimPad = activeState && _abilitiesLogic.IsAimPadAvailableForActionOfType(pad.ActionType);

            if (_abilitiesLogic.IsTypeValidAsMovementAndNotActionPad(pad.ActionType)) {
                MovementPadStartPosition = activeState ? pad.StartPosition : Vector2.zero;
                MovementPadDirection = Vector2.zero;
            } else {
                ActionPadStartPosition = activeState ? pad.StartPosition : Vector2.zero;
                ActionPadAimDirection = Vector2.zero;
            }
                
            Changed();
        }

        private void ManagePadsToUpdate(Definitions.ActionType actionType, bool activeState) {

            if (activeState) {
                AddToDirtyPads(actionType);
                
            } else {
                RemoveFromDirtyPads(actionType);
            }
        }

        public bool IsPadDirty(Definitions.ActionType pad) => _dirtyPads.Contains(pad) || _cleanPads.Contains(pad) || _characterStateDependents.Contains(pad);

        private void AddToDirtyPads(Definitions.ActionType pad) {

            if (_dirtyPads.Contains(pad)) {
                return;
            }

            //Debug.Log("marked as dirty pad "+pad);
            _dirtyPads.Add(pad);

            AffectRelevantPads(_downUpDependentsList, true, true);
        }

        private void RemoveFromDirtyPads(Definitions.ActionType pad) {

            if (!_dirtyPads.Contains(pad)) {
                return;
            }

            //Debug.Log("removed dirty pad "+pad);
            AddToCleanPads(pad);
            
            _dirtyPads.Remove(pad);
            AffectRelevantPads(_downUpDependentsList, false, true);
        }

        private void AddToCleanPads(Definitions.ActionType pad) {

            if (_cleanPads.Contains(pad)) {
                return;
            }
            
            //Debug.Log("marked as cleaned pad "+pad);
            _cleanPads.Add(pad);
            
            AffectRelevantPads(_downUpDependentsList, true, false);
        }
        
        public void TryRemoveFromCleanPad(Definitions.ActionType pad)
        {

            if (!_cleanPads.Contains(pad) || _dirtyPads.Contains(pad))
            {
                return;
            }

            //Debug.Log("Cleared clean pad "+pad);
            _cleanPads.Remove(pad);
        }

        public void SetDependents(List<Definitions.ActionType> downKeyDependents, List<Definitions.ActionType> upKeyDependents, List<Definitions.ActionType> characterStateDependents) {
            
            _downKeyDependents = downKeyDependents;
            _upKeyDependents = upKeyDependents;
            _downUpDependentsList = new List<Definitions.ActionType>(_downKeyDependents);
            _downUpDependentsList.AddRange(_upKeyDependents);
            
            _characterStateDependents = characterStateDependents;
        }
        
        private void AffectRelevantPads(List<Definitions.ActionType> dependents, bool useAddVsRemove, bool treatAsDirtyVsClean) {

                foreach (var dependentActionType in dependents) {

                    if (dependentActionType == Definitions.ActionType.Movement || !IsActionAvailable(dependentActionType) ||
                        !useAddVsRemove && IsActionCurrentlyUsed(dependentActionType)) {
                        continue;
                    }
                    
                    if (useAddVsRemove) {
                        if (treatAsDirtyVsClean) {
                            AddToDirtyPads(dependentActionType);
                        } else {
                            AddToCleanPads(dependentActionType);
                        }
                        
                    } else {
                        if (treatAsDirtyVsClean) {
                            RemoveFromDirtyPads(dependentActionType);
                        } else {
                            TryRemoveFromCleanPad(dependentActionType);
                        }
                    }
                }
            //}
        }

        private bool IsActionAvailable(Definitions.ActionType dependentActionType) {
            return _availableActions.Contains(dependentActionType);
        }
        
        private bool IsActionCurrentlyUsed(Definitions.ActionType dependentActionType) {

            return dependentActionType switch {
                Definitions.ActionType.Movement => IsMovementPad,
                Definitions.ActionType.Jump => IsJumpPad,
                Definitions.ActionType.Attack => IsAttackPad,
                Definitions.ActionType.Block => IsBlockPad,
                Definitions.ActionType.Grab => IsGrabPad,
                Definitions.ActionType.Projectile => IsProjectilePad,
                Definitions.ActionType.Rope => IsRopePad,
                Definitions.ActionType.Special => IsSupurrPad,
                _ => false
            };
        }

        private void UpdatePadState(ActionInput touchData) {

            if (touchData.ActionType == Definitions.ActionType.Movement) {
                MovementPadDirection = touchData.NormalizedDirection;
                
            } else {
                ActionPadAimDirection = touchData.NormalizedDirection;
            }
            
            Changed();
        }

        public void SetCooldownForPadType(Definitions.ActionType actionType, float coolDownInPercentage, bool internalUpdate) {

            if (_padsCooldown.ContainsKey(actionType)) {
                _padsCooldown[actionType] = coolDownInPercentage;
                
            } else {
              
                _padsCooldown.Add(actionType, coolDownInPercentage);
            }

            if (internalUpdate) {
                return;
            }
            
            Changed();
        }
        
        public float GetCooldownByPadType(Definitions.ActionType actionType) {
            var result = (_padsCooldown.Where(padCooldown => padCooldown.Key == actionType).
                Select(padCooldown => padCooldown.Value)).FirstOrDefault();

            return result;
        }

        public void SetUsingDirtyPads(bool isActive) => OnlyUpdateDirtyPadsDisplay = isActive;

        public void UpdateViewWhenStateChanged(bool isActive) => OnlyUpdateViewWhenStateChanged = isActive;

        public bool IsTypeValidAsMovementAndNotActionPad(Definitions.ActionType actionType) => 
            _abilitiesLogic.IsTypeValidAsMovementAndNotActionPad(actionType);

        public bool IsActionPadValidForAlternativeState(Definitions.ActionType actionType) => 
            _downKeyDependents.Contains(actionType) || _upKeyDependents.Contains(actionType);

        public bool IsInputDirectionValidAsDown() =>
            _inputLogic.IsInputDirectionValidAsDown(MovementPadDirection.y);

        public bool IsInputDirectionValidAsUp() =>
            _inputLogic.IsInputDirectionValidAsUp(MovementPadDirection.y);

        public Definitions.ActionType GetAttackMoveAsAbility(Definitions.AttackAbility ability) 
            => ability switch {
                Definitions.AttackAbility.LightAttackAlsoDefaultAttack or Definitions.AttackAbility.MediumAttack or Definitions.AttackAbility.HeavyAttack or Definitions.AttackAbility.RollAttack or Definitions.AttackAbility.AerialAttack
                => Definitions.ActionType.Attack,
                Definitions.AttackAbility.LightGrabAlsoDefaultGrab or Definitions.AttackAbility.MediumGrab or Definitions.AttackAbility.HeavyGrab or Definitions.AttackAbility.AerialGrab
                => Definitions.ActionType.Grab,
                Definitions.AttackAbility.LightBlock
                => Definitions.ActionType.Block
            };

        public InteractableObject2DState GetState() => _state;
        public bool IsAerialState() => _state != null ? _gameplayLogic.IsStateConsideredAsAerial(_state.CurrentState) : false;

    }

}