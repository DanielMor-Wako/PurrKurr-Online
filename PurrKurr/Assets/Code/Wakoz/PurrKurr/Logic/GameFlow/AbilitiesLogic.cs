using System.Collections.Generic;
using System.Linq;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Logic.GameFlow {
    public class AbilitiesLogic : SOData<AbilitiesDataSO> {
        public AbilitiesLogic(string assetName) : base(assetName) { }
        
        private InputLogic _inputLogic;

        private List<ActionPadData> _actionPadsData = new();
        
        protected override void Init() {
            
            _inputLogic = SingleController.GetController<LogicController>().InputLogic;
            _actionPadsData = Data.GetActionPadsData();
        }

        public TouchPadsConfig GetTouchPadsConfig(IEnumerable<Definitions.ActionType> padTypesToRetrieve) {

            var actionPads = (from actionPad in padTypesToRetrieve
                from actionPadData in _actionPadsData
                where actionPadData.ActionType == actionPad
                select new TouchPadConfig(actionPadData.ActionType,
                    true,
                    actionPadData.PadType == Definitions.PadType.Flexible)).ToList();

            return new TouchPadsConfig(actionPads);
        }

        public bool IsAimPadAvailableForActionOfType(Definitions.ActionType actionType) {

            foreach (var padData in _actionPadsData.Where(padData => padData.ActionType == actionType)) {
                if (padData.PadType == Definitions.PadType.Flexible) {
                    return true;
                }
            }

            return false;
        }

        public bool IsTypeValidAsMovementAndNotActionPad(Definitions.ActionType actionType) {

            // shortcut because the only nav as an action is movement 
            return actionType == Definitions.ActionType.Movement;
            
            /*foreach (var padData in _actionPadsData.Where(padData => padData.ActionType == actionType)) {
                if (padData.ActionTypeGroup == Definitions.ActionTypeGroup.Navigation) {
                    return true;
                }
            }

            return false;*/
        }

        public Definitions.ActionTypeGroup GetActionTypeGroup(Definitions.ActionType actionType) {
            
            foreach (var padData in _actionPadsData.Where(padData => padData.ActionType == actionType)) {
                return padData.ActionTypeGroup;
            }

            return 0;
        }
        
        public Definitions.SwipeDistanceType GetSwipeTypeByActionType(Definitions.ActionType actionType) {
            return _actionPadsData.Where(padData => padData.ActionType == actionType).Select(padData => padData.SwipeDistanceType).FirstOrDefault();
            /*foreach (var padData in _actionPadsData.Where(padData => padData.ActionType == actionType)) {
                return padData.SwipeDistanceType;
            }
            return Definitions.SwipeDistanceType.Short;*/
        }
        
        public Vector2 GetSwipeDistanceForActionPad(Definitions.ActionType actionType) {
            return new Vector2(_inputLogic.GetSwipeDistance(GetSwipeTypeByActionType(actionType)), _inputLogic.GetSwipeDistanceLong());
        }
        
        public float GetSwipeVelocityForActionPad(Definitions.ActionType actionType) {
            return _inputLogic.MinSwipeVelocity;
        }
        
        public Definitions.AttackAbility GetAttackAbility(Definitions.NavigationType navigationDirection, Definitions.ActionType actionType, Definitions.CharacterState characterState) {
            
            var attack = actionType == Definitions.ActionType.Attack;
            var block = actionType == Definitions.ActionType.Block;
            var grab = actionType == Definitions.ActionType.Grab;
            
            return navigationDirection switch {
                _ when attack && characterState is Definitions.CharacterState.Running => Definitions.AttackAbility.RollAttack,
                _ when attack && characterState is Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling => Definitions.AttackAbility.AerialAttack,
                _ when attack && _inputLogic.IsNavigationDirValidAsDown(navigationDirection) => Definitions.AttackAbility.HeavyAttack,
                _ when attack && _inputLogic.IsNavigationDirValidAsUp(navigationDirection) => Definitions.AttackAbility.MediumAttack,
                _ when attack => Definitions.AttackAbility.LightAttackAlsoDefaultAttack,
                _ when grab && _inputLogic.IsNavigationDirValidAsDown(navigationDirection) => Definitions.AttackAbility.HeavyGrab,
                _ when grab && _inputLogic.IsNavigationDirValidAsUp(navigationDirection) => Definitions.AttackAbility.MediumGrab,
                _ when grab && characterState is Definitions.CharacterState.Jumping or Definitions.CharacterState.Falling => Definitions.AttackAbility.AerialGrab,
                _ when grab => Definitions.AttackAbility.LightGrabAlsoDefaultGrab,
                _ => Definitions.AttackAbility.LightBlock
            };
        }

        public bool IsAbilityAnAttack(Definitions.AttackAbility ability) =>
            ability is
                Definitions.AttackAbility.LightAttackAlsoDefaultAttack or
                Definitions.AttackAbility.MediumAttack or
                Definitions.AttackAbility.HeavyAttack or
                Definitions.AttackAbility.AerialAttack or
                Definitions.AttackAbility.RollAttack;

        public bool IsAbilityAGrab(Definitions.AttackAbility ability) =>
            ability is
                Definitions.AttackAbility.LightGrabAlsoDefaultGrab or
                Definitions.AttackAbility.MediumGrab or
                Definitions.AttackAbility.HeavyGrab or
                Definitions.AttackAbility.AerialGrab;
        

    }
}