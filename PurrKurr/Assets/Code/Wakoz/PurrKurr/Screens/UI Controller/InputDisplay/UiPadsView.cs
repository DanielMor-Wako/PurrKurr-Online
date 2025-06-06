﻿using System.Collections.Generic;
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

        public PadView GetPadView(Definitions.ActionType ability) 
            => ability switch {

            Definitions.ActionType.Movement => MovementPad,
            Definitions.ActionType.Jump => JumpPad,
            Definitions.ActionType.Attack => AttackPad,
            Definitions.ActionType.Block => BlockPad,
            Definitions.ActionType.Grab => GrabPad,
            Definitions.ActionType.Projectile => ProjectilePad,
            Definitions.ActionType.Rope => RopePad,
            Definitions.ActionType.Special => SuppurrPad,
            Definitions.ActionType.Empty => null,
            _ => null
        };

        public PadView GetPadView(Definitions.ActionType ability, PadView padView) {
            return padView;
        }

        protected override void ModelReplaced() {
            base.ModelReplaced();
            
            InitializePads(Model.Config);
        }
        
        private void InitializePads(UiPadsData data) {

            if (data == null) {
                Debug.LogError("missing pads data");
                return;
            }

            _touchPadModels = new List<PadModel>();
            
            InitPad(MovementPad, data.MovementPadConfig);
            InitPad(JumpPad, data.JumpPadConfig);
            InitPad(AttackPad, data.AttackPadConfig);
            InitPad(BlockPad, data.BlockPadConfig);
            InitPad(GrabPad, data.GrabPadConfig);
            InitPad(ProjectilePad, data.ProjectilePad);
            InitPad(RopePad, data.RopePadConfig);
            InitPad(SuppurrPad, data.SupurrPadConfig);
        }

        private void InitPad(PadView view, UiPadData data) {

            if (data == null) {
                Debug.LogError($"no config set for {view.name}");
                return;
            }
            
            if (view == null) {
                Debug.LogError($"no view set for {data.actionType}");
                return;
            }

            var model = new PadModel(data);
            _touchPadModels.Add(model);
            view.gameObject.SetActive(data.IsAvailable);
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
                    // todo: add leap beneath platform graphic , when on platform collider and player input is down + jump
                    //padModel.SetAlternativeSymbolState(isValidDownInput ? Model.IsOnPlatformState() ? 3 : 1 : 2);
                    return true;
                }

                var state = Model.GetState();
                var isAerialState = Model.IsAerialState();
                var isRopeState = state.CurrentState is Definitions.ObjectState.RopeClimbing or Definitions.ObjectState.RopeClinging;

                if (padModel.Config.actionType == Definitions.ActionType.Attack) {
                    padModel.SetAlternativeSymbolState(
                        isRopeState ? 4 : isAerialState ? 3 : isValidDownInput ? 1 : Model.IsInputDirectionValidAsUp() ? 2 : 0);
                    return true;
                }

                if (padModel.Config.actionType == Definitions.ActionType.Grab) {
                    padModel.SetAlternativeSymbolState(
                        isRopeState ? 4 : isAerialState ? 3 : isValidDownInput ? 1 : Model.IsInputDirectionValidAsUp() ? 2 : 0);
                    return true;
                }

            }

            return false;
        }
    }

}