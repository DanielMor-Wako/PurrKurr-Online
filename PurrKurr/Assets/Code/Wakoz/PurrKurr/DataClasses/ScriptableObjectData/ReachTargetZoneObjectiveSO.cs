using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.Utils.Attributes;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [TypeMarkerMultiClass(typeof(ReachTargetZoneObjective))]
    [CreateAssetMenu(fileName = "ObjectiveData", menuName = "Data/Objective/Reach Target Zone")]
    public class ReachTargetZoneObjectiveSO : ObjectiveDataSO {

        public int targetZoneId = 0;
    }
}
