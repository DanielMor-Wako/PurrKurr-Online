using Code.Wakoz.PurrKurr.DataClasses.Enums;
using System;
using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.Agents
{
    [System.Serializable]
    public class GoalData
    {
        public Definitions.AgentGoal Goal;
        public List<string> TargetStrings; // List of target strings for the goal
        public List<GoalConditionData> Conditions;
    }

    [System.Serializable]
    public class GoalConditionData
    {
        public Definitions.GoalCondition Condition;
        public float targetValue;
    }
}