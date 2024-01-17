#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Code.Wakoz.PurrKurr {
    public static class EditorTools {
        private static GUIStyle _boldLabel;
        public static GUIStyle BoldLabel {
            get {
                if(_boldLabel == null) {
                    _boldLabel = new GUIStyle(GUI.skin.label);
                    _boldLabel.fontStyle = FontStyle.Bold;
                }
                return _boldLabel;
            }
        }

        public static int ButtonIntGrid(int count, int lineLength) {
            int i = 0;
            int retVal = -1;
            while(i < count) {
                EditorGUILayout.BeginHorizontal();
                for(int j = 0; (j < lineLength) && (i < count); ++j) {
                    if(GUILayout.Button(i.ToString())) {
                        retVal = i;
                    }
                    ++i;
                }
                EditorGUILayout.EndHorizontal();
            }
            return retVal;
        }

        public delegate GUIContent GUIContentAdapter(int i);
        public delegate Color ColorAdapter(int i);
        public static int ButtonGrid(int count, int lineLength, GUIContentAdapter labelAdapter, ColorAdapter colorAdapter) {
            int i = 0;
            int retVal = -1;
            var origColor = GUI.backgroundColor;
            while(i < count) {
                EditorGUILayout.BeginHorizontal();
                for(int j = 0; (j < lineLength) && (i < count); ++j) {
                    GUI.backgroundColor = colorAdapter(i);
                    if(GUILayout.Button(labelAdapter(i))) {
                        retVal = i;
                    }
                    ++i;
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.backgroundColor = origColor;
            return retVal;
        }
    }
}
#endif