using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.UI.Instructions {
    public class OverlayInstructionsView : View<OverlayInstructionsModel> {

        [SerializeField] List<OverlayInstructionView> _instructionAnimators;

        private Dictionary<OverlayInstructionModel, OverlayInstructionView> _activeInstructions;

        protected override void ModelReplaced() {
            base.ModelReplaced();

            _activeInstructions = new();
        }

        protected override void ModelChanged() {

            UpdateView();
        }

        private void UpdateView() {

            if (Model.HasActiveInstructions()) {
                ShowInsturctions();
            } else {
                HideInsturctions();
            }

        }

        private void ShowInsturctions() {

            for (int i = 0; i < Model.ActiveInstructions.Count; i++) {

                OverlayInstructionModel model = Model.ActiveInstructions[i];
                if (!_activeInstructions.ContainsKey(model) || _activeInstructions[model] == null) {
                    var animator = GetAvailableAnimator();
                    _activeInstructions.Add(model, animator);
                }

                _activeInstructions[model].UpdateAnimatorValues(model);
            }
        }

        private OverlayInstructionView GetAvailableAnimator() {

            foreach (var animator in _instructionAnimators) {

                if (_activeInstructions.ContainsValue(animator)) {
                    continue;
                }

                return animator;
            }

            return null;
        }

        private void HideInsturctions() {

            foreach (var activeItem in _activeInstructions) {
                var animator = activeItem.Value;
                if (animator != null) {
                    animator.UpdateActiveState(false);
                }
            }

            _activeInstructions.Clear();
        }
    }
}