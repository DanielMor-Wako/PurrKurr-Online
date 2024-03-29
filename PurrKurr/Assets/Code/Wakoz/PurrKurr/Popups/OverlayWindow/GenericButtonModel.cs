﻿namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow {

    public enum GenericButtonType {
        Confirm = 0,
        Close = 1,
        Next = 2,
        Back = 3,
        Alternative = 4,
        Maximize = 5,
        Minimize = 6,
    }

    public class GenericButtonModel : Model {

        public GenericButtonData Data;
        
        public bool IsAvailable;

        public GenericButtonModel() { }

        public GenericButtonModel(GenericButtonData data) {
            
            UpdateInternal(data);
        }

        public void UpdateData(GenericButtonData data, bool isAvailable = true) {

            UpdateInternal(data);
            IsAvailable = data != null && isAvailable;
            Changed();
        }

        public bool IsClickable() =>
            Data.ButtonType is GenericButtonType.Confirm or GenericButtonType.Close or GenericButtonType.Alternative
            || IsWithinPageLimits(Data.ButtonType);

        public bool IsWithinPageLimits(GenericButtonType type) {

            var nextOrBack = type is GenericButtonType.Next or GenericButtonType.Back;

            if (!nextOrBack) {
                return false;
            }

            return IsAvailable;
        }

        private void UpdateInternal(GenericButtonData data) {

            Data = data;
        }

    }
}