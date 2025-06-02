using Code.Core;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Code.Wakoz.Editor
{
    public static class ClearLocalCache
    {
        [MenuItem("Wakoz/Clear Local Cache", priority = 1202)]
        public static async void DeleteLocalCache() {

            string header = $"Delete {CacheConfig.CacheFilePath}?";
            string leftButton = "Nah";
            string RightButton = "Yes";

            try {
                await EditorCancelOkPopup.Open(header, leftButton, RightButton);
                var filePath = Path.Combine(Application.persistentDataPath, CacheConfig.CacheFilePath);
                if (File.Exists(filePath)) {
                    File.Delete(filePath);
                }
                Debug.Log("Cleared Cache file");
            }
            catch (OperationCanceledException) {
                Debug.Log("Keeping Cache file");
            }
        }
    } 
}

