using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives {

    // Todo: Consider adding Glpths to each objective type

    public class SmashObjective : Objective<SmashObjectiveSO> {
/*
        private readonly _glyphKey = "";

        public override string GetObjectiveDescription() 
            => $"{_glyphKey} Smash {_data.Objective.TargetObjectType}";

        protected override void InitializeSpecific() {
            base.InitializeSpecific();

            _glyphKey = GetGlypth(_glyphKey);
        }*/
    }

    public class CollectObjective : Objective<CollectObjectiveSO> {
/*
        public override string GetObjectiveDescription()
            => $"Collect {_data.Objective.TargetObjectType}";
*/
    }

    public class ReachTargetZoneObjective : Objective<ReachTargetZoneObjectiveSO> {
/*
        public override string GetObjectiveDescription() 
            => $"{_data.Instructions}";
*/
    }
}