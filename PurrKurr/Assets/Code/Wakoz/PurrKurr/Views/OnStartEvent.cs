using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Wakoz.PurrKurr.Views {
    public class OnStartEvent : View {
        
        public UnityEvent OnStart;
        
        [SerializeField][Range(0, 10000)]
        private int _preActionDelay;

        protected override void ModelChanged() {}
        
        private async void Awake()
        {
            await Task.Delay(_preActionDelay);
            OnStart?.Invoke();
        }

    }

}