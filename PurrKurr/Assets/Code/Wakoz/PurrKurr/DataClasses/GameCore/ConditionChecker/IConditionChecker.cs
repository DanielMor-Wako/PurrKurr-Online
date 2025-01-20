using System;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore 
{
    public interface IConditionChecker 
    {
        public void StartCheck(IInteractableBody targetBody, Action onConditionMetAction);
        public void EndCheck();
    }
}