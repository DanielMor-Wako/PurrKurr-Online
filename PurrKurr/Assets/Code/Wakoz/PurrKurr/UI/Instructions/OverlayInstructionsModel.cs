using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.UI.Instructions {
    public class OverlayInstructionsModel : Model {

        public List<OverlayInstructionModel> ActiveInstructions;

        public OverlayInstructionsModel() {

            ActiveInstructions = new();
        }

        public bool HasActiveInstructions() => ActiveInstructions.Count > 0;

        public void StartAnimation(OverlayInstructionModel instructionAnimation) {

            ActiveInstructions.Add(instructionAnimation);
            Changed();
        }

        public void StopAnimations() {

            if (ActiveInstructions == null || ActiveInstructions.Count == 0) {
                return;
            }

            ActiveInstructions.Clear();
            Changed();
            
        }
    }
}