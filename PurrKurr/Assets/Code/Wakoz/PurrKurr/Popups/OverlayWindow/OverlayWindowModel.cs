using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow {
    public class OverlayWindowModel : Model {

        public string Title { get; private set; }
        public string BodyContent { get; private set; }
        public Sprite BodyPicture { get; private set; }
        public List<GenericButtonData> ButtonsRawData { get; private set; }

        public bool HasTitle => !string.IsNullOrEmpty(Title);
        public bool HasBody => HasBodyContent || HasBodyPicture;
        public bool HasBodyContent => !string.IsNullOrEmpty(BodyContent);
        public bool HasBodyPicture => BodyPicture != null;
        public bool HasFooter => ButtonsRawData != null && ButtonsRawData.Count > 0;

        public bool IsActiveWindow() => HasTitle || HasBody || HasFooter;

        public OverlayWindowModel(string title = null, string bodyContent = null, Sprite bodyPicture = null, List<GenericButtonData> buttons = null) {

            UpdateInternal(title, bodyContent, bodyPicture, buttons);
        }

        private void UpdateInternal(string title = null, string bodyContent = null, Sprite bodyPicture = null, List<GenericButtonData> buttonsData = null) {

            Title = title;
            BodyContent = bodyContent;
            BodyPicture = bodyPicture;
            ButtonsRawData = buttonsData;
        }

        public void UpdateDisplay(string title = null, string bodyContent = null, Sprite bodyPicture = null, List<GenericButtonData> buttons = null) {

            UpdateInternal(title, bodyContent, bodyPicture, buttons);

            Changed();
        }

    }
}