using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{
    [System.Serializable]
    public class ObjectiveData
    {
        public string UniqueId;
        public string TargetObjectType;
        public string[] TargetObjectIds;
        [Min(1)] public int RequiredQuantity = 1;
        //[HideInInspector]
        public int CurrentQuantity = 0;

        public Sprite Icon;

        public ObjectiveData() {}

        public ObjectiveData(string uniqueId, string targetObjectType, int requiredQuantity)
        {
            UniqueId = uniqueId;
            TargetObjectType = targetObjectType;
            RequiredQuantity = requiredQuantity;
        }

        public ObjectiveData(ObjectiveData other)
        {
            UniqueId = other.UniqueId;
            TargetObjectType = other.TargetObjectType;
            var targetIds = other.TargetObjectIds;
            if (targetIds != null)
            {
                TargetObjectIds = new string[targetIds.Length];
                for (int i = 0; i < targetIds.Length; i++)
                {
                    TargetObjectIds[i] = targetIds[i];
                }
            } else
            {
                TargetObjectIds = new string[0];
            }
            RequiredQuantity = other.RequiredQuantity;
            CurrentQuantity = other.CurrentQuantity;
        }

        
    }
}