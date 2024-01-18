using System;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDetection {
    public class ScreenInputEventTriggers : MonoBehaviour {

        public event Action<Definitions.ActionType> OnTouchPadDown;
        public event Action<Definitions.ActionType> OnTouchPadUp;

        private void TouchPadDown(Definitions.ActionType actionType) => OnTouchPadDown?.Invoke(actionType);
        private void TouchPadUp(Definitions.ActionType actionType) => OnTouchPadUp?.Invoke(actionType);
        

        public void MovementPadDown() => TouchPadDown(Definitions.ActionType.Movement);
        public void MovementPadUp() => TouchPadUp(Definitions.ActionType.Movement);

        public void JumpPadDown() => TouchPadDown(Definitions.ActionType.Jump);
        public void JumpPadUp() => TouchPadUp(Definitions.ActionType.Jump);
        
        public void AttackPadDown() => TouchPadDown(Definitions.ActionType.Attack);
        public void AttackPadUp() => TouchPadUp(Definitions.ActionType.Attack);
        
        public void BlockPadDown() => TouchPadDown(Definitions.ActionType.Block);
        public void BlockPadUp() => TouchPadUp(Definitions.ActionType.Block);
        
        public void GrabPadDown() => TouchPadDown(Definitions.ActionType.Grab);
        public void GrabPadUp() => TouchPadUp(Definitions.ActionType.Grab);
        
        public void ProjectilePadDown() => TouchPadDown(Definitions.ActionType.Projectile);
        public void ProjectilePadUp() => TouchPadUp(Definitions.ActionType.Projectile);
        
        public void RopePadDown() => TouchPadDown(Definitions.ActionType.Rope);
        public void RopePadUp() => TouchPadUp(Definitions.ActionType.Rope);
        
        public void SpecialPadDown() => TouchPadDown(Definitions.ActionType.Special);
        public void SpecialPadUp() => TouchPadUp(Definitions.ActionType.Special);

    }

}