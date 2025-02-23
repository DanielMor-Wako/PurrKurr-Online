using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives {

    public interface IObjective {
        
        // Process
        void Initialize(ObjectiveDataSO data);
        bool IsComplete();
        void UpdateProgress(int quantity);
        void Finish();
        
        // Get Data
        string GetUniqueId();
        string GetObjectType();
        string[] TargetObjectIds();
        string GetObjectiveDescription();
        int GetCurrentQuantity();
        int GetRequiredQuantity();
        Sprite GetObjectiveIcon();
    }
}