using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.InputDisplay {
    public class UiPadsView : View<UiPadsModel> {

        public PadView MovementPad;
        public PadView JumpPad;
        public PadView AttackPad;
        public PadView BlockPad;
        public PadView GrabPad;
        public PadView ProjectilePad;
        public PadView RopePad;
        public PadView SuppurrPad;

        private List<PadModel> _touchPadModels;

        protected override void ModelReplaced() {
            base.ModelReplaced();
            
            InitializePads(Model.Config);
        }
        
        private void InitializePads(UiPadsData config) {

            if (config == null) {
                Debug.LogError("missing pads data");
                return;
            }

            _touchPadModels = new List<PadModel>();
            
            InitPad(MovementPad, config.MovementPadConfig);
            InitPad(JumpPad, config.JumpPadConfig);
            InitPad(AttackPad, config.AttackPadConfig);
            InitPad(BlockPad, config.BlockPadConfig);
            InitPad(GrabPad, config.GrabPadConfig);
            InitPad(ProjectilePad, config.ProjectilePad);
            InitPad(RopePad, config.RopePadConfig);
            InitPad(SuppurrPad, config.SupurrPadConfig);
        }

        private void InitPad(PadView view, UiPadData config) {

            if (config == null) {
                Debug.LogError($"no config set for {view.name}");
                return;
            }
            
            if (view == null) {
                Debug.LogError($"no view set for {config.actionType}");
                return;
            }

            var model = new PadModel(config);
            _touchPadModels.Add(model);
            view.gameObject.SetActive(config.IsAvailable);
            view.SetModel(model);
        }

        protected override void ModelChanged() {

            UpdatePads();
        }

        private void UpdatePads() {

            if (_touchPadModels == null) {
                return;
            }

            foreach (var pad in _touchPadModels) {
                UpdatePad(pad);
            }
        }

        private void UpdatePad(PadModel padModel) {
            
            if (padModel.Config.actionType == Definitions.ActionType.Empty) {
                return;
            }

            var isActive = padModel.Config.actionType switch {
                    Definitions.ActionType.Movement => Model.IsMovementPad,
                    Definitions.ActionType.Jump => Model.IsJumpPad,
                    Definitions.ActionType.Attack => Model.IsAttackPad,
                    Definitions.ActionType.Block => Model.IsBlockPad,
                    Definitions.ActionType.Grab => Model.IsGrabPad,
                    Definitions.ActionType.Projectile => Model.IsProjectilePad,
                    Definitions.ActionType.Rope => Model.IsRopePad,
                    Definitions.ActionType.Special => Model.IsSupurrPad,
                    _ => false
                };
            
            if (Model.OnlyUpdateDirtyPadsDisplay) {
                if (!Model.IsPadDirty(padModel.Config.actionType)) {
                    return;
                }
                Model.TryRemoveFromCleanPad(padModel.Config.actionType);
            }
            
            var hasUpdatedAlternativeState = UpdateAlternativePadStates(padModel);
            var hasUpdateCooldown = UpdatePadCooldown(padModel);
            var hasChanges = hasUpdatedAlternativeState || hasUpdateCooldown;

            var isMovementPad = Model.IsTypeValidAsMovementAndNotActionPad(padModel.Config.actionType);
            
            padModel.SetState(isActive,
                isMovementPad ? Model.MovementPadStartPosition : Model.ActionPadStartPosition,
                isMovementPad ? Model.MovementPadDirection : Model.ActionPadAimDirection,
                Model.OnlyUpdateViewWhenStateChanged && hasChanges);
        }

        private bool UpdatePadCooldown(PadModel padModel) {
            
            var retVal = Model.GetCooldownByPadType(padModel.Config.actionType);
            padModel.SetCooldown(retVal);
            return retVal > 0;
        }

        private bool UpdateAlternativePadStates(PadModel padModel) {

            if (Model.IsActionPadValidForAlternativeState(padModel.Config.actionType)) {

                var isValidDownInput = Model.IsInputDirectionValidAsDown();
                
                if (padModel.Config.actionType == Definitions.ActionType.Jump) {
                    padModel.UpdateAimAvailability(isValidDownInput);
                    padModel.SetAlternativeSymbolState(isValidDownInput ? 1 : 2);
                    return true;
                }
                
                if (padModel.Config.actionType == Definitions.ActionType.Attack) {
                    padModel.SetAlternativeSymbolState(
                        Model.IsAerialState() ? 3 : isValidDownInput ? 1 : Model.IsInputDirectionValidAsUp() ? 2 : 0);
                    return true;
                }

                if (padModel.Config.actionType == Definitions.ActionType.Grab) {
                    padModel.SetAlternativeSymbolState(
                        Model.IsAerialState() ? 3 : isValidDownInput ? 1 : Model.IsInputDirectionValidAsUp() ? 2 : 0);
                    return true;
                }

            }

            return false;
        }

    }

}