using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr {
    [DefaultExecutionOrder(1)]
    public abstract class Controller : MonoBehaviour {
        
        //private AcceptBlock _acceptBlock;
        //private Semaphore _initializationSemaphore = new Semaphore(0);
        
        protected abstract Task Initialize();
        protected abstract void Clean();
        
        protected virtual async void Awake() {

            // Debug.Log("Awake on controller " + name + " : " + GetType().Name + " : " + GetInstanceID());

            try {
                SetCoreFeatures();
                await Initialize();
            }
            finally {
                //_initializationSemaphore.Release();
            }
        }
        
        protected virtual void OnDestroy() {
            Clean();
        }

        protected void SetCoreFeatures() {
            
            //_acceptBlock = new AcceptBlock();
        }
        
        /*public async Task WaitForInitializationDone() {
            if(_initializationSemaphore != null) {
                await _initializationSemaphore.WaitAsync();
                _initializationSemaphore = null;
            }
        }*/

        /*
        protected void Accept<T>(System.Action<T> registrant) where T : IPolyEvent {
            _acceptBlock.AcceptEvent(registrant);
        }*/

    }

}