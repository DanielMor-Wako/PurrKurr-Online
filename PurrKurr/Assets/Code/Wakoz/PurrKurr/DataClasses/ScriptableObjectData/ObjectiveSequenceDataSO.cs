using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData
{
    [CreateAssetMenu(fileName = "ObjectiveSequenceData", menuName = "Data/Objective/Sequence")]
    public class ObjectiveSequenceDataSO : ScriptableObject
    {
        [SerializeField] private string _uniqueId;

        [Tooltip("Finish conditions as Win or Lose during active mission")]
        [SerializeField] private ObjectiveFinishConditionsData _finishConditions;

        [Tooltip("Instructions steps before mission starts the sequence data")]
        [SerializeField] private List<ObjectiveSequenceData> _initializeData;

        [Tooltip("Instructions steps for the mission as sequence data")]
        [SerializeField] private List<ObjectiveSequenceData> _sequenceData;

        public string UniqueId => _uniqueId;

        public ObjectiveFinishConditionsData FinishConditions => _finishConditions;

        public List<ObjectiveSequenceData> InitializeData => _initializeData;

        public List<ObjectiveSequenceData> SequenceData => _sequenceData;
    }
}
