using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.StrategyPatterns;
using Code.Wakoz.Utils.Attributes;
using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{
    /// <summary>
    /// Factory for creating IObjective instances.
    /// </summary>
    public class ObjectiveFactory : FactoryPattern<IObjective>
    {
        public IObjective Create(ObjectiveDataSO data)
        {
            Type objectiveType = GetObjectiveType(data);
            if (objectiveType != null && typeof(IObjective).IsAssignableFrom(objectiveType))
            {
                return Create(objectiveType);
            }

            Debug.LogError($"Failed to create objective and to determine type for data: {data.Objective.UniqueId}");
            return null;
        }

        private Type GetObjectiveType(ObjectiveDataSO data)
        {
            Type objectiveDataType = data.GetType();
            var attribute = Attribute.GetCustomAttribute(objectiveDataType, 
                typeof(TypeMarkerMultiClassAttribute)) as TypeMarkerMultiClassAttribute;

            if (attribute == null)
            {
                Debug.LogError($"No attribute found for ObjectiveDataSO: {objectiveDataType.Name}");
                return null;
            }

            return attribute.type;
        }
    }
}