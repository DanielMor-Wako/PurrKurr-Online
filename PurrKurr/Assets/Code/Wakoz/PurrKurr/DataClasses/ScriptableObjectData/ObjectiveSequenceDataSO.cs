using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData
{
    [CreateAssetMenu(fileName = "ObjectiveSequenceData", menuName = "Data/Objective/Sequence")]
    public class ObjectiveSequenceDataSO : ScriptableObject
    {
        [SerializeField] private string _uniqueId;

        [SerializeField] private ObjectiveFinishConditionsData _finishConditions;

        [SerializeField] private List<ObjectiveSequenceData> _sequenceData;

        public string UniqueId => _uniqueId;

        public ObjectiveFinishConditionsData FinishConditions => _finishConditions;

        public List<ObjectiveSequenceData> SequenceData => _sequenceData;
    }
}
