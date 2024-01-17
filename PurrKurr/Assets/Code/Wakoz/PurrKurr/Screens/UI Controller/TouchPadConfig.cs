using Code.Wakoz.PurrKurr.DataClasses.Enums;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller {
    public class TouchPadConfig {

        public Definitions.ActionType actionType { get; private set; }
        public bool IsAvailable { get; private set; }
        public bool IsAimAvailable { get; private set; } // alternative state
        
        public TouchPadConfig(Definitions.ActionType actionType, bool isAvailable = false, bool isAimAvailable = false) {

            this.actionType = actionType;
            IsAvailable = isAvailable;
            IsAimAvailable = isAimAvailable;
        }
        
        public void UpdateAimAvailability(bool isAimAvailable) {

            IsAimAvailable = isAimAvailable;
        }
    }

}