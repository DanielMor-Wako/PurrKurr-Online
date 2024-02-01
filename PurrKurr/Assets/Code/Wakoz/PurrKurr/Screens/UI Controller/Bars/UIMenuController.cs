using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars {
    [DefaultExecutionOrder(11)]
    public class UIMenuController : SingleController {

        [SerializeField] private UIMenuView _view;

        private UIMenuModel _model;

        protected override void Clean() { }

        protected override Task Initialize() {

            _model = new UIMenuModel(false);

            _view.SetModel(_model);

            return Task.CompletedTask;
        }

        public void Toggle() => _model.Toggle();

    }
}
