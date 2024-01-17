using System;
using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [CreateAssetMenu(fileName = "AbilitiesData", menuName = "Data/Abilities")]
    public class AbilitiesDataSO : ScriptableObject {
        
        [Header("Pads Config")]
        [SerializeField] private List<ActionPadData> _padsConfig;
        
        public List<ActionPadData> GetActionPadsData() {

            var result = new List<ActionPadData>();

            var allEnums = Enum.GetValues(typeof(Definitions.ActionType));
            
            foreach (Definitions.ActionType actionType in allEnums) {
                
                if (actionType == Definitions.ActionType.Empty) {
                    continue;
                }
                
                if (TryGetActionPadData(actionType, out var newData)) {
                    result.Add(newData);
                }
            }

            return result;
        }

        private bool TryGetActionPadData(Definitions.ActionType type, out ActionPadData matchingData) {

            foreach (var pad in _padsConfig) {
                
                if (pad.ActionType != type) {
                    continue;
                }

                matchingData = pad;
                return true;
            }

            matchingData = new ActionPadData();
            return false;
        }

    }

}