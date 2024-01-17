using UnityEngine;

namespace Code.Wakoz.PurrKurr.Views {
    [ExecuteInEditMode]
    public class ActiveDependency : View {
        [SerializeField] private GameObject[] _dependants;

        protected override void ModelChanged() {}
        
        private void OnEnable() {
            
            if (_dependants == null || _dependants.Length == 0) {
                return;
            }
            
            for(var i = 0; i < _dependants.Length; ++i) {
                _dependants[i].SetActive(true);
            }
        }
        
        private void OnDisable() {
            
            if (_dependants == null || _dependants.Length == 0) {
                return;
            }
            
            for (var i = 0; i < _dependants.Length; ++i) {
                _dependants[i].SetActive(false);
            }
        }
    }

}