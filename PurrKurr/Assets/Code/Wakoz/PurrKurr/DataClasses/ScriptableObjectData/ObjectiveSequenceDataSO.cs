using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData
{
    [CreateAssetMenu(fileName = "ObjectiveSequenceData", menuName = "Data/Objective/Sequence")]
    public class ObjectiveSequenceDataSO : ScriptableObject
    {
        [SerializeField] private string _uniqueId;

        [SerializeField] private List<ObjectiveSequenceData> _sequenceData;

        public string UniqueId => _uniqueId;

        public List<ObjectiveSequenceData> SequenceData => _sequenceData;
    }
}
