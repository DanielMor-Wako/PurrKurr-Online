//using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.Editor {
    public class EditorInputFieldPopup : EditorCancelOkPopup {
        
        // todo: switch to UniTask
        private string _retVal;

        public static async Task<string> Open(string header) {

            var window = CreateInstance<EditorInputFieldPopup>();
            window.Header = header;
            window.LeftButton = "Cancel";
            window.RightButton = "Ok";

            window.ShowPopup();
            await window.Sem.WaitAsync(window.Cancellation.Token);
            return window._retVal;
        }

        protected override void DoBody() {
            _retVal = GUILayout.TextField(_retVal);
        }
    }
}
