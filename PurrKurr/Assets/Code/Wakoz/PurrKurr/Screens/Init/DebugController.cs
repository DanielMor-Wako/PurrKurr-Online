using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Init {
    public class DebugController : SingleController {

        [Tooltip("When checked, allows for Debug.Log\n(Auto-Cancelled out of Unity Editor)")]
        public bool LogEnabled;

        [Tooltip("When checked, allows for Debug.Draw\n(Auto-Cancelled out of Unity Editor)")]
        public bool DrawEnabled;

        protected override void Clean() { }

        protected override Task Initialize() {

            DisableAllWhenOutOfEditor();

            return Task.CompletedTask;
        }

        private void DisableAllWhenOutOfEditor() {

#if !UNITY_EDITOR
            LogEnabled = false;
            DrawEnabled = false;
#endif
        }

        public void Log(string msg) {

#if UNITY_EDITOR      
            if (!LogEnabled) {
                return;
            }

            Debug.Log(msg);
#endif
        }
        
        public void LogWarning(string msg) {

#if UNITY_EDITOR      
            if (!DrawEnabled) {
                return;
            }

            Debug.LogWarning(msg);
#endif
        }
        
        public void LogError(string msg) {

#if UNITY_EDITOR      
            if (!DrawEnabled) {
                return;
            }

            Debug.LogError(msg);
#endif
        }

        public void DrawRay(Vector3 start, Vector2 dir, Color color, int duration) {

#if UNITY_EDITOR
            if (!DrawEnabled) {
                return;
            }

            Debug.DrawRay(start, dir, color, duration);
#endif
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, int duration) {

#if UNITY_EDITOR      
            if (!DrawEnabled) {
                return;
            }

            Debug.DrawLine(start, end, color, duration);
#endif
        }

    }
}