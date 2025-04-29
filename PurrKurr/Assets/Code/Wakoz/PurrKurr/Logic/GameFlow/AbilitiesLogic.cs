using System.Collections.Generic;
using System.Linq;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay;
using UnityEngine;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.Logic.GameFlow
{
    /// <summary>
    /// logic used for touch pads data
    /// </summary>
    public sealed class AbilitiesLogic : ScriptableAsset<AbilitiesDataSO>
    {
        public AbilitiesLogic(string assetName, string assetPrefix) : base(assetName, assetPrefix) { }

        private InputLogic _inputLogic;

        private List<ActionPadData> _actionPadsData = new();

        protected override void Init()
        {
            _inputLogic = SingleController.GetController<LogicController>().InputLogic;
            _actionPadsData = Data.GetActionPadsData();
        }

        public UiPadsData GetTouchPadsConfig(IEnumerable<ActionType> padTypesToRetrieve)
        {
            var actionPads = (from actionPad in padTypesToRetrieve
                              from actionPadData in _actionPadsData
                              where actionPadData.ActionType == actionPad
                              select new UiPadData(actionPadData.ActionType,
                                  true,
                                  actionPadData.PadType == PadType.Flexible)).ToList();

            return new UiPadsData(actionPads);
        }

        public bool IsAimPadAvailableForActionOfType(ActionType actionType)
        {
            foreach (var padData in _actionPadsData.Where(padData => padData.ActionType == actionType))
            {
                if (padData.PadType == PadType.Flexible)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsTypeValidAsMovementAndNotActionPad(ActionType actionType)
        {
            // shortcut because the only nav as an action is movement 
            return actionType == ActionType.Movement;
            /*foreach (var padData in _actionPadsData.Where(padData => padData.ActionType == actionType)) {
                if (padData.ActionTypeGroup == Definitions.ActionTypeGroup.Navigation) {
                    return true;
                }
            }

            return false;*/
        }

        public ActionTypeGroup GetActionTypeGroup(ActionType actionType)
        {
            foreach (var padData in _actionPadsData.Where(padData => padData.ActionType == actionType))
            {
                return padData.ActionTypeGroup;
            }

            return 0;
        }

        public SwipeDistanceType GetSwipeTypeByActionType(ActionType actionType) 
            => _actionPadsData.Where(padData => padData.ActionType == actionType).Select(padData => padData.SwipeDistanceType).FirstOrDefault();

        public Vector2 GetSwipeDistanceForActionPad(ActionType actionType)
            => new Vector2(_inputLogic.GetSwipeDistance(GetSwipeTypeByActionType(actionType)), _inputLogic.SwipeDistanceLong);

        public float GetSwipeVelocityForActionPad(ActionType actionType)
            => _inputLogic.MinSwipeVelocity;

        public AttackAbility GetAttackAbility(NavigationType navigationDirection, ActionType actionType, ObjectState characterState)
        {
            var attack = actionType == ActionType.Attack;
            var block = actionType == ActionType.Block;
            var grab = actionType == ActionType.Grab;

            return navigationDirection switch
            {
                _ when attack && characterState is ObjectState.Running => AttackAbility.RollAttack,
                _ when attack && characterState is ObjectState.Jumping or ObjectState.Falling => AttackAbility.AerialAttack,
                _ when attack && _inputLogic.IsNavigationDirValidAsDown(navigationDirection) => AttackAbility.HeavyAttack,
                _ when attack && _inputLogic.IsNavigationDirValidAsUp(navigationDirection) => AttackAbility.MediumAttack,
                _ when attack => AttackAbility.LightAttackAlsoDefaultAttack,
                _ when grab && _inputLogic.IsNavigationDirValidAsDown(navigationDirection) => AttackAbility.HeavyGrab,
                _ when grab && _inputLogic.IsNavigationDirValidAsUp(navigationDirection) => AttackAbility.MediumGrab,
                _ when grab && characterState is ObjectState.Jumping or ObjectState.Falling => AttackAbility.AerialGrab,
                _ when grab => AttackAbility.LightGrabAlsoDefaultGrab,
                _ when block => AttackAbility.LightBlock
            };
        }
        /// <summary>
        /// Returns the ability type of an ability as Attack Grab or Block
        /// When ability is not 'None', Uses the index as a devider to find the result by index range.
        /// 0 - 4 (attack), 5 - 8 (grab), 9 > ... (block)
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public ActionType GetAbilityType(AttackAbility ability) {
            if (ability is AttackAbility.None) return ActionType.Empty;
            
            var index = (int)ability;
            switch (index) {
                case >= BlockIndexMinRange:
                    return ActionType.Block;
                case >= GrabIndexMinRange:
                    return ActionType.Grab;
                case >= AttackIndexMinRange:
                    return ActionType.Attack;
                default:
                    return ActionType.Empty;
            }
        }
        private const int BlockIndexMinRange = 9; // ref (int)AttackAbility.LightBlock;
        private const int GrabIndexMinRange = 5; // ref (int)AttackAbility.LightGrabAlsoDefaultGrab;
        private const int AttackIndexMinRange = 0; // ref (int)AttackAbility.LightAttackAlsoDefaultAttack;

        public bool IsAbilityAnAttack(AttackAbility ability) =>
            ability is
                AttackAbility.LightAttackAlsoDefaultAttack or
                AttackAbility.MediumAttack or
                AttackAbility.HeavyAttack or
                AttackAbility.AerialAttack or
                AttackAbility.RollAttack;

        public bool IsAbilityAGrab(AttackAbility ability) =>
            ability is
                AttackAbility.LightGrabAlsoDefaultGrab or
                AttackAbility.MediumGrab or
                AttackAbility.HeavyGrab or
                AttackAbility.AerialGrab;

        public bool IsAbilityBlock(AttackAbility ability) =>
            ability is
            AttackAbility.LightBlock;
    }
}