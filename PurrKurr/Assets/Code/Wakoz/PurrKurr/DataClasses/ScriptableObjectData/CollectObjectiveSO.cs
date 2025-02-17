using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.Utils.Attributes;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [TypeMarkerMultiClass(typeof(CollectObjective))]
    [CreateAssetMenu(fileName = "ObjectiveData", menuName = "Data/Objective/Collect")]
    public class CollectObjectiveSO : ObjectiveDataSO {

        //[Min(1)] public int requiredQuantity = 1;
    }
}
