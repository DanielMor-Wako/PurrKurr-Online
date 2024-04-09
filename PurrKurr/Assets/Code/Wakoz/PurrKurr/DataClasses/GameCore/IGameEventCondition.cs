using System;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore {

    public interface IGameEventCondition {
        public void StartCheckingCondition(IInteractableBody targetBody, Action onConditionMetAction);
        public void EndCheckingCondition();
    }
}