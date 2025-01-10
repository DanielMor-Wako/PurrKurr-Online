using UnityEngine;

namespace Code.Wakoz.PurrKurr.Views
{
    public class MultiStateView : View {
        
        [SerializeField] private GameObject[] _states;
        [SerializeField] private GameObject _currentState;

        public int? CurrentState => GetCurrentState();
        
        public void ChangeState(int state) {
            for(int i = 0; i < _states.Length; i++) {
                if(_states[i] != null) {
                    _states[i].SetActive(false);
                }
            }
            _currentState = null;
            if(state < _states.Length) {
                _currentState = _states[state];
                if (_currentState != null) {
                    _currentState.SetActive(true);
                }
            }
        }
        
        private int? GetCurrentState() {
            for (int i = 0; i < _states.Length; i++) {
                if (_states[i] == _currentState)
                    return i;
            }
            return null;
        }

        protected override void ModelChanged() {}

        #region Editor
#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(MultiStateView))]
        public class Drawer : UnityEditor.Editor {
            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
                var tar = (MultiStateView)target;
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.LabelField("Set Initial State", EditorTools.BoldLabel);
                if (tar._states != null) {
                    var clickedIndex = EditorTools.ButtonIntGrid(tar._states.Length, 5);
                    if (clickedIndex > -1) {
                        for (int k = 0; k < tar._states.Length; ++k) {
                            if (tar._states[k] != null) {
                                tar._states[k].SetActive(false);
                            }
                        }
                        tar._currentState = tar._states[clickedIndex];
                        if (tar._currentState != null) {
                            tar._currentState.SetActive(true);
                        }
                    }
                }
            }
        }
#endif
        #endregion
    }
}
