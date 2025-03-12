using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData
{

    public class ObjectiveDataSO : ScriptableObject {

        [SerializeField]
        private ObjectiveData _objectiveData;

        public ObjectiveData Objective => _objectiveData;

        /// <summary>
        /// Creates a new duplicated instance of the objective
        /// </summary>
        /// <returns></returns>
        public ObjectiveData GetObjectiveData() => new ObjectiveData(_objectiveData);

        [Tooltip("Populates TargetObjectIds by RequiredQuantity\nObjectType + Number")]
        [ContextMenu("Populate Target Object Ids")]
        public void PopulateTargetObjects()
        {
            var objectIds = new List<string>();

            var objectType = _objectiveData.TargetObjectType;
            for (var numb = 1; numb <= _objectiveData.RequiredQuantity; numb++)
            {
                objectIds.Add($"{objectType}{numb}");
            }

            _objectiveData.TargetObjectIds = objectIds.ToArray();
        }
    }
}
