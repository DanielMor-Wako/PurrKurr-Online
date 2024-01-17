using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr {
    [DefaultExecutionOrder(2)]
    public abstract class SingleController : Controller {
        
        private static readonly Dictionary<System.Type, Controller> _instances = new();

        // Victor: Refactor code to include a cancellation source that can stop the initialization when the object was destroyed
        protected override async void Awake() {

             //Debug.Log("Awake on controller " + name + " : " + GetType().Name + " : " + GetInstanceID());

/*#if UNITY_EDITOR
            if (GetInstanceID() > 0) {
                return;
            }
#endif*/
            var type = GetType();

            if (_instances.ContainsKey(type)) {
                Debug.LogWarning("duplicated controller " + name);
                Destroy(gameObject);
            }
            else {

                _instances.Add(type, this);
                SetCoreFeatures();
                //ServiceManager.Inject(this);

                try {
                    await Initialize();
                }
                finally {
                    //_initializationSemaphore.Release();
                }
            }
        }

        protected override void OnDestroy() {
            //Debug.Log("Destroyed controller " + name);
            var type = GetType();

            if (_instances.ContainsKey(type)) {
                var currentInstance = _instances[type];

                if (currentInstance == this) {
                    _instances.Remove(type);
                    Clean();
                }
            }
        }
        
        /// <summary>
        /// Function returns a single controller from the dictionary
        /// </summary>
        public static T GetController<T>() where T : SingleController {
            return _instances[typeof(T)] as T;
            // Usage Example: SingleController.GetController<ExampleController>()
        }

        protected async Task BlockForAction(Func<Task> action) {
            Debug.Log("Activate Overlay Blocker");

            try {
                await action();
            }
            finally {
                Debug.Log("Deactivate Overlay Blocker");
            }
        }
    }

}