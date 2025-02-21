using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives {

    public class SmashObjective : Objective<SmashObjectiveSO> {

        public override string GetObjectiveDescription() 
            => $"Defeat {_data.Objective.TargetObjectType}";

    }

    public class CollectObjective : Objective<CollectObjectiveSO> {

        public override string GetObjectiveDescription()
            => $"Collect {_data.Objective.TargetObjectType}";

    }

    public class ReachTargetZoneObjective : Objective<ReachTargetZoneObjectiveSO> {

        public override string GetObjectiveDescription() 
            => $"{_data.Instructions}";

    }
}