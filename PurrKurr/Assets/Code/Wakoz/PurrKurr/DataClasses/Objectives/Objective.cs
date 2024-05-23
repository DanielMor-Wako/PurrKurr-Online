using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives {
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

        public abstract bool IsComplete();

        public abstract void UpdateProgress(int amount);

        public abstract string GetObjectiveDescription();

        protected abstract void InitializeSpecific();

        public abstract void Finish();

        public string GetUniqueId() => _data.uniqueId;

        public string GetObjectType() => _data.targetObjectType;
    }
}