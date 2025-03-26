using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{
    [System.Serializable]
    public class ObjectiveFinishConditionsData
    {
        [Tooltip("The type of action represents")]
        [SerializeField] private List<ObjectiveConditionData> _winConditions;

        [Tooltip("The type of action represents")]
        [SerializeField] private List<ObjectiveConditionData> _loseConditions;

        public List<ObjectiveConditionData> WinConditions => _winConditions;

        public List<ObjectiveConditionData> LoseConditions => _loseConditions;
    }

    [System.Serializable]
    public class ObjectiveConditionData
    {
        public ObjectiveEndCondition Condition;
        public List<string> targetObjectIds;
    }

    public enum ObjectiveEndCondition : byte
    {
        IsAlive = 0,
        isDead = 1,
        IsConsumeable = 2,
        IsConsumed = 3,

    }

}