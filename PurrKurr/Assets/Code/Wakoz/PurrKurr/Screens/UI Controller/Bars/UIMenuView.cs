using Code.Wakoz.PurrKurr.Views;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars {
    public class UIMenuView : View<UIMenuModel> {

        [SerializeField] private MultiStateView _view;

        protected override void ModelChanged() {

            UpdateMenu();
        }

        private void UpdateMenu() {

            if (_view == null) {
                return;
            }

            _view.ChangeState(Model.IsOpen ? 1 : 0);
        }
    }
}
