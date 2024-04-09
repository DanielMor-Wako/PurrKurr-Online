using Code.Wakoz.PurrKurr.DataClasses.Enums;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger {
    public class OverlayWindowConditionalCharacterState : OverlayWindowCondition<Definitions.ObjectState> {

        /// <summary>
        /// Holds character state condition to match, 
        /// if the condition is met, the window passes to the next page or quit if its the last page
        /// </summary>
        
        private Definitions.ObjectState _currentState;

        public override bool HasMetCondition() {
            return _currentState == ValueToMatch;
        }

        public override void UpdateCurrentValue() {
            _currentState = TargetBody.GetCurrentState();
        }
    }
}