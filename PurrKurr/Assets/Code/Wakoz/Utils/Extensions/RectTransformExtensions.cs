using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    // todo: splits to different files
    
    
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

    
    
    
    public static class HelperFunctions {

        public static string GetString(this byte[] self) {
            return Encoding.UTF8.GetString(self);
        }

        public static string ToBase64(this string self) {
            var plainTextBytes = Encoding.UTF8.GetBytes(self);

            return Convert.ToBase64String(plainTextBytes);
        }

        public static string FromBase64(this string self) {
            var base64EncodedBytes = Convert.FromBase64String(self);

            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string ToString(this object self, Color color) {
            return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + self + "</color>";
        }

        public static bool AreDictionariesEqual<T1, T2>(Dictionary<T1, T2> lDictionary, Dictionary<T1, T2> rDictionary)
            where T2 : IEquatable<T2> {
            if (rDictionary == null ||
                lDictionary == null) {
                return false;
            }

            foreach (var pair in rDictionary) {
                if (!lDictionary.ContainsKey(pair.Key) ||
                    !lDictionary[pair.Key].Equals(pair.Value)) {
                    return false;
                }
            }

            return true;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> collection) {
            return collection == null || collection.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(this IList<T> collection) {
            return collection == null || collection.Count == 0;
        }

        public static void SetLeft(this RectTransform rt, float left) {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetRight(this RectTransform rt, float right) {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top) {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        public static void SetBottom(this RectTransform rt, float bottom) {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        public static void Shuffle(this IList list) {
            var rand = new System.Random();

            for (int i = list.Count - 1; i > 0; --i) {
                var j = rand.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static T Random<T>(this IList<T> list) {
            var rand = new System.Random();

            return list[rand.Next(list.Count)];
        }

        public static bool SameAs(this string self, string other) {
            return self.Equals(other, StringComparison.OrdinalIgnoreCase);
        }

    }
    
}