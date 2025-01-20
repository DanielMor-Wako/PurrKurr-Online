using Code.Wakoz.PurrKurr.UI.Instructions;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger {
    public class OverlayWindowConditions : MonoBehaviour {

        [SerializeField] private List<OverlayWindowConditionData> _conditions;

        private Dictionary<int, IConditionChecker> _conditionsPerPage;

        public IConditionChecker GetEventCondition(int pageIndex) {
            return _conditionsPerPage.ContainsKey(pageIndex) ? _conditionsPerPage[pageIndex] : null;
        }

        public List<OverlayWindowAnimationData> GetAnimationData(int pageIndex) {
            return _conditions.Count > 0 && _conditions[0].AnimatedAction.Count > (pageIndex) ? _conditions[pageIndex].AnimatedAction : null;
        }

        public bool IsApplyingSlowMotion() => _conditions[0].ApplySlowMotion;

        private void Awake() {

            InitConditions();
        }

        private void InitConditions() {
            
            if (_conditions == null || _conditions.Count == 0) {
                return;
            }

            _conditionsPerPage = new();
            for (int i = 0; i < _conditions.Count; i++) {
                var eventConditionRef = _conditions[i].CharacterConditionChecker;
                var eventCondition = eventConditionRef != null ? eventConditionRef.GetComponent<IConditionChecker>() : null;
                if (eventCondition == null) {
                    continue;
                }
                _conditionsPerPage.Add(i, eventCondition);
            }
        }
    }
}