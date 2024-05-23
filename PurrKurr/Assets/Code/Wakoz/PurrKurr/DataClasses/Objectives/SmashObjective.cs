using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives {

    public class SmashObjective : Objective<SmashObjectiveSO> {

        private int _currentKills;

        protected override void InitializeSpecific() {
            _currentKills = 0;
        }

        public override bool IsComplete() 
            => _currentKills >= _data.requiredQuantity;

        public override void UpdateProgress(int amountToAdd) {
            _currentKills += amountToAdd;
        }

        public override string GetObjectiveDescription() 
            => "Defeat " + _data.requiredQuantity + " enemies with ID " + _data.targetObjectType;

        public override void Finish() {
            _currentKills = _data.requiredQuantity;
        }
    }

    public class CollectObjective : Objective<CollectObjectiveSO> {

        private int _currentQuantity;

        protected override void InitializeSpecific() {
            _currentQuantity = 0;
        }

        public override bool IsComplete()
            => _currentQuantity >= _data.requiredQuantity;

        public override void UpdateProgress(int amountToAdd) {
            _currentQuantity += amountToAdd;
        }

        public override string GetObjectiveDescription()
            => "Collect " + _data.requiredQuantity + " items with ID " + _data.targetObjectType;

        public override void Finish() {
            _currentQuantity = _data.requiredQuantity;
        }
    }

    public class ReachTargetZoneObjective : Objective<ReachTargetZoneObjectiveSO> {

        private bool _reached;

        protected override void InitializeSpecific() {
            _reached = false;
        }

        public override bool IsComplete() 
            => _reached;

        public override void UpdateProgress(int amount) {
            _reached = amount > 0;
        }

        public override string GetObjectiveDescription() 
            => "Reach Location " + _data.uniqueId + " with ID " + _data.targetZoneId;

        public override void Finish() {
            _reached = true;
        }
    }
}