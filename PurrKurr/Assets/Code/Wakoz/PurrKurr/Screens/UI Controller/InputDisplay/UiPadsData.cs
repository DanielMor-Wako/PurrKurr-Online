using System;
using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Enums;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay {
    public class UiPadsData {
        
        public UiPadData MovementPadConfig { get; private set; }
        public UiPadData JumpPadConfig { get; private set; }
        public UiPadData AttackPadConfig { get; private set; }
        public UiPadData BlockPadConfig { get; private set; }
        public UiPadData GrabPadConfig { get; private set; }
        public UiPadData ProjectilePad { get; private set; }
        public UiPadData RopePadConfig { get; private set; }
        public UiPadData SupurrPadConfig { get; private set; }

        public UiPadsData(List<UiPadData> configs) {

            foreach (var config in configs) {
                switch (config.actionType) {
                    
                    case Definitions.ActionType.Movement: {
                        MovementPadConfig = config;
                        break;
                    }

                    case Definitions.ActionType.Jump:
                        JumpPadConfig = config;
                        break;

                    case Definitions.ActionType.Attack:
                        AttackPadConfig = config;
                        break;

                    case Definitions.ActionType.Block:
                        BlockPadConfig = config;
                        break;

                    case Definitions.ActionType.Grab:
                        GrabPadConfig = config;
                        break;

                    case Definitions.ActionType.Projectile:
                        ProjectilePad = config;
                        break;

                    case Definitions.ActionType.Rope:
                        RopePadConfig = config;
                        break;

                    case Definitions.ActionType.Special:
                        SupurrPadConfig = config;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            FillEmptyConfigAsUnavailable();
        }

        private void FillEmptyConfigAsUnavailable() {

            MovementPadConfig ??= new UiPadData(Definitions.ActionType.Movement);
            JumpPadConfig ??= new UiPadData(Definitions.ActionType.Jump);
            AttackPadConfig ??= new UiPadData(Definitions.ActionType.Attack);
            BlockPadConfig ??= new UiPadData(Definitions.ActionType.Block);
            GrabPadConfig ??= new UiPadData(Definitions.ActionType.Grab);
            ProjectilePad ??= new UiPadData(Definitions.ActionType.Projectile);
            RopePadConfig ??= new UiPadData(Definitions.ActionType.Rope);
            SupurrPadConfig ??= new UiPadData(Definitions.ActionType.Special);
        }

    }

}