using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.Utils.Attributes;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [TypeMarkerMultiClass(typeof(SmashObjective))]
    [CreateAssetMenu(fileName = "ObjectiveData", menuName = "Data/Objective/Smash")]
    public class SmashObjectiveSO : ObjectiveDataSO {

    }
}
