using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Code.Wakoz.Utils.Extensions {
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

        public static Vector2 RotateVector(this Vector2 originalVector, float rotationAngleDegrees) {

            // Convert the rotation angle to radians
            float rotationAngleRadians = rotationAngleDegrees * (float)Math.PI / 180;

            float cos = (float)Math.Cos(rotationAngleRadians);
            float sin = (float)Math.Sin(rotationAngleRadians);

            float newX = originalVector.x * cos - originalVector.y * sin;
            float newY = originalVector.x * sin + originalVector.y * cos;

            return new Vector2(newX, newY);
        }

        public static List<Vector2> GenerateVectorsBetween(this Vector2 startPosition, Vector2 endPosition, float linkSize) {

            float distance = (endPosition - startPosition).magnitude;
            int numberOfLinks = Mathf.CeilToInt(distance / linkSize);

            List<Vector2> points = new List<Vector2> {
                startPosition
            };

            for (int i = 1; i < numberOfLinks; i++) {
                float t = i / (float)numberOfLinks;
                points.Add(Vector2.Lerp(startPosition, endPosition, t) );
            }

            var aletrnative = false;
            for (int i = 1; i < numberOfLinks; i++) {
                Debug.DrawLine(points[i - 1], points[i], aletrnative ? Color.black : Color.white, 1);
                aletrnative = !aletrnative;
            }

            return points;
        }

        public static Vector2 CalculateRotatedVector2(this Vector2 startPoint, Vector2 endPoint, float angleOffset) {

            Vector2 direction = (endPoint - startPoint).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle += angleOffset;

            return Quaternion.Euler(0, 0, angle) * Vector2.right;
        }

        public static Quaternion CalculateRotatedQuaternion(this Vector2 startPoint, Vector2 endPoint, float angleOffset) {
            Vector2 direction = (endPoint - startPoint).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle += angleOffset;

            return Quaternion.Euler(0, 0, angle);
        }

        public static float PercentReachedBetweenPoints(Vector3 startVector, Vector3 endVector, Vector3 refVector) {

            // Returns float that represents the percentage reached between two vectors by a third vector as refference point
            var distanceStartToEnd = (endVector - startVector).magnitude;
            var distanceStartToTarget = (refVector - startVector).magnitude;
            return Mathf.Clamp01(distanceStartToTarget / distanceStartToEnd);
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