namespace Code.Wakoz.PurrKurr.UI.Instructions {
    public class OverlayInstructionModel {

        public bool IsActive { get; private set; }
        public OverlayWindowAnimationData _animationData { get; private set; }

        public OverlayInstructionModel(OverlayWindowAnimationData animationData, bool isActive = true) {
            _animationData = animationData;
            SetActiveState(isActive);
        }

        public void SetActiveState(bool isActive) {
            IsActive = isActive;
        }
    }
}