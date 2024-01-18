using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay {
    public class AimPadModel : Model {

        public Vector2 AimDir;
        public bool IsActive => AimDir != Vector2.zero;
        public Definitions.SwipeDistanceType SwipeDistanceTypeType;

        public AimPadModel(Vector2 aimDir, Definitions.SwipeDistanceType swipeDistanceTypeType) {
            AimDir = aimDir;
            SwipeDistanceTypeType = swipeDistanceTypeType;
        }

        public void UpdateDir(Vector2 aimDir) {
            
            AimDir = aimDir;
            Changed();
        }

    }

}