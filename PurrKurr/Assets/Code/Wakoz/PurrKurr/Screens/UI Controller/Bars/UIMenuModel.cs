namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller.Bars {
    public class UIMenuModel : Model {

        public bool IsOpen { get; private set; }

        public UIMenuModel(bool isOpen) {
            UpdateUiStatInternal(isOpen);
        }

        public void Toggle() {

            UpdateUiStatInternal(!IsOpen);
            Changed();
        }

        private void UpdateUiStatInternal(bool isOpen) {

            IsOpen = isOpen;
        }

    }
}
