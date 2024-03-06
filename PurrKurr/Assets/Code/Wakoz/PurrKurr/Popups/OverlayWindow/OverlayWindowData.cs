using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Popups.OverlayWindow {

    [Serializable]
    public class OverlayWindowData {

        public string Title;
        public string BodyContent;
        public Sprite BodyPicture;
        public List<GenericButtonData> ButtonsRawData;
        [HideInInspector] public string PageCount;

    }
}