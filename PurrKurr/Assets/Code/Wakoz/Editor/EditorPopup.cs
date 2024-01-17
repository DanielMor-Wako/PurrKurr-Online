using UnityEditor;
using UnityEngine;

namespace Code.Wakoz.Editor {
    public abstract class EditorPopup : EditorWindow {

        protected virtual Vector2 GetWindowSize() {
            return new Vector2 (300, 100);
        }

        private void OnEnable() {
            var main = EditorGUIUtility.GetMainWindowPosition();
            var size = GetWindowSize();
            position = new Rect(main.x + ((main.width - size.x) / 2),
                                        main.y + ((main.height - size.y) / 2),
                                        size.x,
                                        size.y);
        }

        private void OnGUI() {
            DoHeader();
            DoBody();
            DoButtons();
        }

        protected virtual void DoHeader() { }
        protected virtual void DoBody() { }
        protected virtual void DoButtons() { }
    }
}
