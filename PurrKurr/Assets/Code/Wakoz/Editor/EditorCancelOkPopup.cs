//using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.Editor {
    public class EditorCancelOkPopup : EditorPopup {

        // todo: switch to UniTask
        //protected Semaphore Sem = new Semaphore(0);
        protected SemaphoreSlim Sem = new (0);
        protected CancellationTokenSource Cancellation = new CancellationTokenSource();

        protected string Header;
        protected string LeftButton;
        protected string RightButton;

        public static async Task Open(string header, string leftButton = "Cancel", string RightButton = "Ok") {

            var window = CreateInstance<EditorCancelOkPopup>();
            window.Header = header;
            window.LeftButton = leftButton;
            window.RightButton = RightButton;

            window.ShowPopup();
            await window.Sem.WaitAsync(window.Cancellation.Token);
        }

        protected override void DoHeader() {
            GUILayout.Space(20);
            var style = GUI.skin.GetStyle("Label");
            style.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(Header, style);
        }

        protected override void DoButtons() {
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(LeftButton)) {
                Cancellation.Cancel();
                Close();
            }
            if(GUILayout.Button(RightButton)) {
                Sem.Release();
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}


