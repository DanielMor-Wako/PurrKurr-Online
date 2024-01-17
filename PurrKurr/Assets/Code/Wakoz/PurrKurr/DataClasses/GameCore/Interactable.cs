using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore {

    public class Interactable : MonoBehaviour, IInteractable {

        [SerializeField] private IInteractableBody _damageableBody;
       
        public IInteractableBody GetInteractable() => _damageableBody;

        private void Awake() {
            
            _damageableBody ??= GetComponent<IInteractableBody>();
            _damageableBody ??= GetComponentInParent<IInteractableBody>();
        }

    }

}