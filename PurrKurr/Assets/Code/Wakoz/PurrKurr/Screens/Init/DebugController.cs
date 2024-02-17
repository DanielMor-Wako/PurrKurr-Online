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

            // Disable All When Out Of Editor;
#if !UNITY_EDITOR
            LogEnabled = false;
            DrawEnabled = false;
#endif
            return Task.CompletedTask;
        }

        public void Log(string msg) {

            if (!LogEnabled) {
                return;
            }

            Debug.Log(msg);
        }

        public void LogWarning(string msg) {

            if (!DrawEnabled) {
                return;
            }

            Debug.LogWarning(msg);
        }

        public void LogError(string msg) {

            if (!DrawEnabled) {
                return;
            }

            Debug.LogError(msg);
        }

        public void DrawRay(Vector3 start, Vector2 dir, Color color, float duration) {

            if (!DrawEnabled) {
                return;
            }

            Debug.DrawRay(start, dir, color, duration);
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, float duration) {

            if (!DrawEnabled) {
                return;
            }

            Debug.DrawLine(start, end, color, duration);
        }

    }
}