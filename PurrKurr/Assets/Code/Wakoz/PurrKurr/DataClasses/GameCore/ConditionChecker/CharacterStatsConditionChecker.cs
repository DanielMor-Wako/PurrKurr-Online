namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.ConditionChecker
{
    public class CharacterStatsConditionChecker : CharacterConditionChecker<int>
    {
        /// <summary>
        /// Condition: Min Value to consider as true when a character has entered the zone
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>

        private int _amount;

        public override bool HasMetCondition()
        {
            return _amount > ValueToMatch;
        }

        public override void UpdateCurrentValue()
        {
            _amount = (int)(TargetBody.GetHpPercent() * 100);
        }
    }
}