using Code.Wakoz.PurrKurr.Screens.Ui_Controller;
using System;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.Screens.Gameplay_Controller
{
    public sealed class InputValidator : IDisposable {

        public InputValidator() { }

        public void Dispose() { }

        public bool ValidateNavigation(ActionInput actionInput, bool started, bool ended) {
            
            if (actionInput.ActionGroupType != ActionTypeGroup.Navigation) 
                return false;

            return true;
        }

        public bool ValidateAction(ActionInput actionInput, bool started, bool ended) {

            if (actionInput.ActionGroupType != ActionTypeGroup.Action) 
                return false;

            var isValid = false;

            switch (actionInput.ActionType) {
                case ActionType.Jump:
                case ActionType.Block:
                case ActionType.Projectile:
                case ActionType.Rope:
                case ActionType.Special:
                    isValid = started || ended;
                    break;

                case ActionType.Attack:
                case ActionType.Grab:
                    isValid = started;
                    break;

                default:
                    break;
            }

            return isValid;
        }

    }
}