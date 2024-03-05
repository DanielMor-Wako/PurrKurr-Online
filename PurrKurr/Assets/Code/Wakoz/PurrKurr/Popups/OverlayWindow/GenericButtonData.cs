using System;

namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow {
    public class GenericButtonData {

        public GenericButtonType ButtonType { get; private set; }
        public string ButtonText { get; private set; }

        public Action ClickedAction;

        public GenericButtonData(GenericButtonType buttonType, string text, Action clickedAction = null) {

            ButtonType = buttonType;
            UpdateData(text, clickedAction);
        }

        public void UpdateData(string text, Action clickedAction = null) {
            ButtonText = text;
            ClickedAction = clickedAction;
        }
    }
}