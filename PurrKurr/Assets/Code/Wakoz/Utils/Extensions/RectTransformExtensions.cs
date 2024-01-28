using Code.Wakoz.PurrKurr;
using UnityEngine;

namespace Code.Wakoz.Utils.Extensions {
    public static class RectTransformExtensions  {
        
        public static bool ContainsScreenPoint(this RectTransform rTrans, Canvas myCanvas, Vector2 screenPoint) {
            var inCanvasRect = rTrans.rect;
            inCanvasRect.y += rTrans.localPosition.y;
            inCanvasRect.x += rTrans.localPosition.x;
            var checkedPoint = myCanvas.worldCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, myCanvas.planeDistance));
            checkedPoint = myCanvas.transform.InverseTransformPoint(checkedPoint);
            return inCanvasRect.Contains(checkedPoint);
        }

        public static Vector2 GetSizeDelta(this RectTransform rTrans, Vector2 absoluteSize) {
            var anchoredWidth = rTrans.rect.width - rTrans.sizeDelta.x;
            var anchoredHeight = rTrans.rect.height - rTrans.sizeDelta.y;
            return new Vector2(absoluteSize.x - anchoredWidth, absoluteSize.y - anchoredHeight);
        }
        
    }
    
    // save the json fromJson utils
    
    
    public static class DataClassesExtensions  {
        // takes a class and attempts to insert data into a new instance of the class
        public static T Adapt<T, U>(this Data<U> self) where T : Model {
            return (T)System.Activator.CreateInstance(typeof(T), self);
        }
        public static T Adapt<T, U>(this U self) where T : Data<U> where U : Contract {
            return (T)System.Activator.CreateInstance(typeof(T), self);
        }
    }
    public class Contract {}
    public abstract class Data<T> {

        public Data(bool doNone = false) {
            if(!doNone) {
                InitializeNoData();
            }
        }
        public Data(T serverData) {
            if(serverData == null) {
                Debug.LogWarning(typeof(T).Name + " is null");
                InitializeNoData();
            } else {
                Initialize(serverData);
            }
        }
        protected abstract void Initialize(T serverData);
        protected abstract void InitializeNoData();
    }
    
}