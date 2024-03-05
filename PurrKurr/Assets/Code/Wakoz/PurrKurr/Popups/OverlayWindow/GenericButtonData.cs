using System;

namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow {

    [Serializable]
    public class GenericButtonData {

        public GenericButtonType ButtonType;
        public string ButtonText;

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