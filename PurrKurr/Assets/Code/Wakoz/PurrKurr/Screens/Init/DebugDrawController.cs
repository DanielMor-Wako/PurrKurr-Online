using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Init
{
    public class DebugDrawController : SingleController
    {

        [Tooltip("When checked, allows for Debug.Draw\n(Auto-Cancelled out of Unity Editor)")]
        public bool DrawEnabled;

        protected override void Clean() { }

        protected override Task Initialize() {

#if !UNITY_EDITOR
            DrawEnabled = false;
#endif
            return Task.CompletedTask;
        }

        public void DrawRay(Vector3 start, Vector2 dir, Color color, float duration) {

            if (!DrawEnabled)
                return;

            Debug.DrawRay(start, dir, color, duration);
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, float duration) {

            if (!DrawEnabled)
                return;

            Debug.DrawLine(start, end, color, duration);
        }

    }
}