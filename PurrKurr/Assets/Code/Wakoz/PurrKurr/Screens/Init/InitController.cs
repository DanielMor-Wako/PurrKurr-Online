using System.Globalization;
using System.Threading;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Init
{
    public class InitController : MonoBehaviour {
        
        [SerializeField] private bool _destroyScriptOnLoad = true;

        [SerializeField] private bool _keepGameObjectOnDestroy = true;

        [Tooltip("When value is above 0, it overrides the target frame rate. Otherwise, uses the Default value")]
        [SerializeField][Min(0)] private int _testTargetFrameRate = 0;

        private const int DEFAULT_TARGET_FRAME_RATE = 60;

        private static bool IsInitialized = false;
        
        private void Awake()
        {
            if (IsInitialized)
                return;

            SetTargtFrameRate();
            SetScreenOrientation();
#if !UNITY_EDITOR
            SetLocalization();
#endif
            IsInitialized = true;
        }

        private void SetTargtFrameRate() 
        {
            Application.targetFrameRate = _testTargetFrameRate > 0 ? _testTargetFrameRate : DEFAULT_TARGET_FRAME_RATE;
        }

        private static void SetScreenOrientation() 
        {    
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToPortrait = false;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }

        private static void SetLocalization() 
        {
#if UNITY_ANDROID || UNITY_IPHONE
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
#endif
        }

        private void Start() 
        {
            if (_destroyScriptOnLoad) {

                if (_keepGameObjectOnDestroy) {
                    Destroy(this);
                    
                } else {
                    Destroy(gameObject);
                }
            }
            
        }

        /*
        void OnApplicationQuit() 
        {
#if UNITY_EDITOR
            var constructor = SynchronizationContext.Current.GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);
            var newContext = constructor.Invoke(new object[] { Thread.CurrentThread.ManagedThreadId });
            SynchronizationContext.SetSynchronizationContext(newContext as SynchronizationContext);
#endif
        }
        */
    }
        
}