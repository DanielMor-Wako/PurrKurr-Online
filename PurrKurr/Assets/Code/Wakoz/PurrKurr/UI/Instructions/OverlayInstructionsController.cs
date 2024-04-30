using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.UI.Instructions {

    [DefaultExecutionOrder(12)]
    public sealed class OverlayInstructionsController : SingleController {

        [SerializeField] private OverlayInstructionsView _view;

        private OverlayInstructionsModel _model;

        protected override void Clean() {

            StopInstructions();
        }

        protected override Task Initialize() {

            _model = new OverlayInstructionsModel();
            _view.SetModel(_model);

            return Task.CompletedTask;
        }

        public void StartInstruction(OverlayInstructionModel newInstruction) {

            _model.StartAnimation(newInstruction);
        }

        public void StopInstructions() {

            _model.StopAnimations();
        }

    }
}