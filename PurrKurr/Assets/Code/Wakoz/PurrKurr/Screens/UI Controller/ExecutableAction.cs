using System;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller
{
    [Serializable]
    public class ExecutableAction
    {
        public ActionInput NavigationInput ;// { get; private set; }
        public ActionInput ActionInput ;// { get; private set; }
        public AttackAbility Variant { get; private set; }

        public ExecutableAction(ActionInput navigationInput = null, ActionInput actionInput = null, AttackAbility variant = AttackAbility.None) {
            
            NavigationInput = navigationInput;
            ActionInput = actionInput;
            Variant = variant;
        }

        public void SetVariant(AttackAbility newVariant) {
        
            // Allow modification for state-based adjustments
            Variant = newVariant;
        }
    }
}