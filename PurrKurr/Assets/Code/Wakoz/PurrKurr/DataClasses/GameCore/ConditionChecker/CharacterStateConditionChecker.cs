using Code.Wakoz.PurrKurr.DataClasses.Enums;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.ConditionChecker
{
    public class CharacterStateConditionChecker : CharacterConditionChecker<Definitions.ObjectState>
    {
        /// <summary>
        /// Holds character state condition as a value to match against
        /// </summary>
        
        private Definitions.ObjectState _currentState;

        public override bool HasMetCondition()
        {
            return _currentState == ValueToMatch;
        }

        public override void UpdateCurrentValue()
        {
            _currentState = TargetBody.GetCurrentState();
        }
    }
}