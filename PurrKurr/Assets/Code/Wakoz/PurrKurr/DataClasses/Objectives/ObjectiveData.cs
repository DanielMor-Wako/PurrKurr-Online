using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{
    [System.Serializable]
    public class ObjectiveData
    {
        public string UniqueId;
        public string TargetObjectType;
        [Min(1)] public int RequiredQuantity = 1;
        [HideInInspector] public int CurrentQuantity = 0;

        public ObjectiveData() { }

        public ObjectiveData(string uniqueId, string targetObjectType, int requiredQuantity)
        {
            UniqueId = uniqueId;
            TargetObjectType = targetObjectType;
            RequiredQuantity = requiredQuantity;
            CurrentQuantity = 0;
        }

        public ObjectiveData(ObjectiveData other)
        {
            UniqueId = other.UniqueId;
            TargetObjectType = other.TargetObjectType;
            RequiredQuantity = other.RequiredQuantity;
            CurrentQuantity = other.CurrentQuantity;
        }

    }
}