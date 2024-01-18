using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay {
    public class PadModel : Model {

        public UiPadData Config;
        public bool IsActive { get; private set; }
        public Vector2 AimStartPos { get; private set; }
        public Vector2 AimDir { get; private set; }
        public bool IsAimActive => AimDir != Vector2.zero && Config.IsAimAvailable;
        public int SymbolState { get; private set; }
        public float CooldownPercent { get; private set; }

        public PadModel(UiPadData config) {

            Config = config;
        }

        public void UpdateAimAvailability(bool isAvailable) {

            Config.UpdateAimAvailability(isAvailable);
            //Changed();
        }
        
        public void SetAlternativeSymbolState(int alternativeState) {

            SymbolState = alternativeState;
            //Changed();
        }
        
        public void SetState(bool isActive, Vector2 aimStartPos, Vector2 aimDir, bool onlyUpdateViewIfChangesExist) {

            // todo: enable the entire code chunk , and only update display pads if they have chagned, including the symbolState
            var hasChanges = IsActive != isActive || AimStartPos != aimStartPos || AimDir != aimDir;
            
            var changesLog = "";
            if (IsActive != isActive) {changesLog+=$"IsActive ";}
            if (AimDir != aimDir) {changesLog+=$"AimStartPos ";}
            if (AimStartPos != aimStartPos) {changesLog+=$"AimStartPos ";}
            
            changesLog = $"IsActive {IsActive != isActive} || AimStartPos {AimStartPos != aimStartPos} || AimDir {AimDir != aimDir}";
            IsActive = isActive;
            AimStartPos = aimStartPos;
            AimDir = aimDir;
            
            if (!hasChanges && onlyUpdateViewIfChangesExist) {
                return;
            }

            /*if (Config.actionType == DataClasses.Enums.Definitions.ActionType.Grab) {
                Debug.Log($"{Config.actionType} is updating, state: hasChanges? {hasChanges} , SelectiveUpdate? {onlyUpdateViewIfChangesExist}");
                Debug.Log(changesLog);
            }*/

            Changed();
        }

        public void SetCooldown(float cooldownLeftInPercentage) {
            CooldownPercent = cooldownLeftInPercentage;
        }
        
    }

}