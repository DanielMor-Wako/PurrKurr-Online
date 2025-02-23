using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{
    public abstract class Objective<T> : IObjective where T : ObjectiveDataSO {

        protected T _data;

        public void Initialize(ObjectiveDataSO data) {

            if (data is not T) {
                Debug.LogError("Invalid objective data for " + typeof(T).Name);
                return;
            }

            _data = (T)data;
            InitializeSpecific();
        }

        protected virtual void InitializeSpecific() 
        {
            _data.Objective.CurrentQuantity = 0;
        }

        public virtual bool IsComplete() 
            => _data.Objective.CurrentQuantity >= _data.Objective.RequiredQuantity;

        public virtual void UpdateProgress(int newQuantity) 
            => _data.Objective.CurrentQuantity = newQuantity;

        public virtual void Finish() 
            => _data.Objective.CurrentQuantity = _data.Objective.RequiredQuantity;

        public abstract string GetObjectiveDescription();

        public string GetUniqueId() 
            => _data.Objective.UniqueId;

        public string GetObjectType() 
            => _data.Objective.TargetObjectType;

        public string[] TargetObjectIds()
            => _data.Objective.TargetObjectIds;

        public int GetCurrentQuantity() 
            => _data.Objective.CurrentQuantity;

        public int GetRequiredQuantity() 
            => _data.Objective.RequiredQuantity;

        public Sprite GetObjectiveIcon()
            => _data.Objective.Icon;
    }
}