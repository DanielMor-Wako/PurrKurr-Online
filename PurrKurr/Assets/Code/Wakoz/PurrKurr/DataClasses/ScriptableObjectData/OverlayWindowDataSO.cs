using Code.Wakoz.PurrKurr.Popups.OverlayWindow;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [CreateAssetMenu(fileName = "OverlayWindowData", menuName = "Data/OverlayWindowData")]
    public class OverlayWindowDataSO : ScriptableObject {

        [SerializeField] private List<OverlayWindowData> overlayWindowData;

        public List<OverlayWindowData> GetData() {

            var newList = new List<OverlayWindowData>();
            foreach (var overlayWindowData in overlayWindowData) {
                newList.Add(
                    new OverlayWindowData() { 
                        Title = overlayWindowData.Title, 
                        BodyPicture = overlayWindowData.BodyPicture,
                        BodyContent = overlayWindowData.BodyContent,
                        ButtonsRawData = overlayWindowData.ButtonsRawData,
                        PageCount = overlayWindowData.PageCount
                    }
                );
            }

            return newList;
        }
    }
}
