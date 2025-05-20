using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Init
{
    public class DebugBuildOnlyComponent : MonoBehaviour
    {
        [Tooltip("Destroy or hide any gameobject while build is Debug or Development")]
        [SerializeField] private bool _hideAndNotDestroy = false;

        private void Start() {

            if (Debug.isDebugBuild) return;

            if (_hideAndNotDestroy) {
                gameObject.SetActive(false);

            } else {
                Destroy(gameObject);
            }
        }
    }
}