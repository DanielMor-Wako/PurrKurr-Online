using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives {

    public interface IObjective {
        void Initialize(ObjectiveDataSO data);
        bool IsComplete();
        void UpdateProgress(int amount);
        void Finish();
        string GetUniqueId();
        string GetObjectiveDescription();
        string GetObjectType();
    }
}