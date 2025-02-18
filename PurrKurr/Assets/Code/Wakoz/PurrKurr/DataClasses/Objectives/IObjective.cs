using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives {

    public interface IObjective {
        
        // Process
        void Initialize(ObjectiveDataSO data);
        bool IsComplete();
        void UpdateProgress(int amount);
        void Finish();

        // Get Data
        string GetUniqueId();
        string GetObjectType();
        string GetObjectiveDescription();
        int GetCurrentQuantity();
        int GetRequiredQuantity();
    }
}