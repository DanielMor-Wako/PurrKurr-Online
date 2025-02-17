using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{
    public abstract class Objective<T> : IObjective where T : ObjectiveDataSO {

        protected T _data;

        public void Initialize(ObjectiveDataSO data) {

            if (data is not T) {
                UnityEngine.Debug.LogError("Invalid objective data for " + typeof(T).Name);
                return;
            }

            _data = (T)data;
            InitializeSpecific();
        }

        public virtual bool IsComplete() 
            => _data.Objective.CurrentQuantity >= _data.Objective.RequiredQuantity;

        public virtual void UpdateProgress(int amountToAdd) 
            => _data.Objective.CurrentQuantity += amountToAdd;

        public abstract string GetObjectiveDescription();

        protected virtual void InitializeSpecific()
        {
            _data.Objective.CurrentQuantity = 0;
        }

        public virtual void Finish() 
            => _data.Objective.CurrentQuantity = _data.Objective.RequiredQuantity;

        public string GetUniqueId() 
            => _data.Objective.UniqueId;

        public string GetObjectType() 
            => _data.Objective.TargetObjectType;
    }
}