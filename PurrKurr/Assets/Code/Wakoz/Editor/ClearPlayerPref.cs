using System;
using UnityEditor;
using UnityEngine;

namespace Code.Wakoz.Editor
{
    public static class ClearPlayerPrefs {

        [MenuItem("Wakoz/Clear Player Prefs", priority = 1201)]
        public static async void DeletePlayerPrefs() {

            string header = "Delete Player Prefs?";
            string leftButton = "Nah";
            string RightButton = "Yes";

            try {
                await EditorCancelOkPopup.Open(header, leftButton, RightButton);
                PlayerPrefs.DeleteAll();
                Debug.Log("Cleared Player Prefs");
            } catch(OperationCanceledException) {
                Debug.Log("Keeping Player Prefs");
            }
        }
    }
}

