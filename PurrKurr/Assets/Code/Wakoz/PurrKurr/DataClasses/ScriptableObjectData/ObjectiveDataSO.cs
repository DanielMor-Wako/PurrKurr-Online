using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    public class ObjectiveDataSO : ScriptableObject {

        [SerializeField]
        private ObjectiveData _objectiveData;

        public ObjectiveData Objective => _objectiveData;

        /// <summary>
        /// Creates a new duplicated instance of the objective
        /// </summary>
        /// <returns></returns>
        public ObjectiveData GetObjectiveData() => new ObjectiveData(_objectiveData);

        // Initialize only if it's null
/*
        private void OnValidate()
            => data ??= new ObjectiveData();
*/

    }
}
