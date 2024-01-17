using System;
using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Enums;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller {
    public class TouchPadsConfig {
        
        public TouchPadConfig MovementPadConfig { get; private set; }
        public TouchPadConfig JumpPadConfig { get; private set; }
        public TouchPadConfig AttackPadConfig { get; private set; }
        public TouchPadConfig BlockPadConfig { get; private set; }
        public TouchPadConfig GrabPadConfig { get; private set; }
        public TouchPadConfig ProjectilePad { get; private set; }
        public TouchPadConfig RopePadConfig { get; private set; }
        public TouchPadConfig SupurrPadConfig { get; private set; }

        public TouchPadsConfig(List<TouchPadConfig> configs)
        {
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
            MovementPadConfig ??= new TouchPadConfig(Definitions.ActionType.Movement);
            JumpPadConfig ??= new TouchPadConfig(Definitions.ActionType.Jump);
            AttackPadConfig ??= new TouchPadConfig(Definitions.ActionType.Attack);
            BlockPadConfig ??= new TouchPadConfig(Definitions.ActionType.Block);
            GrabPadConfig ??= new TouchPadConfig(Definitions.ActionType.Grab);
            ProjectilePad ??= new TouchPadConfig(Definitions.ActionType.Projectile);
            RopePadConfig ??= new TouchPadConfig(Definitions.ActionType.Rope);
            SupurrPadConfig ??= new TouchPadConfig(Definitions.ActionType.Special);
        }

    }

}