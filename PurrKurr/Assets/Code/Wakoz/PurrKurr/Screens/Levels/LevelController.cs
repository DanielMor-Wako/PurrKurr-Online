using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;
using Code.Wakoz.Utils.Attributes;

namespace Code.Wakoz.PurrKurr.Screens.Levels {
    public class LevelController : Controller {

        [SerializeField] private List<ObjectiveDataSO> _objectivesData;

        private ObjectivesManager _objectivesManager;


        protected override void Clean() {

        }

        protected override Task Initialize() {

            return Task.CompletedTask;
        }

        public void CompleteObjectiveOfType(Type type) {

            if (_objectivesManager == null || _objectivesData.Count == 0) {
                return;
            }

            _objectivesManager.CompleteObjectiveOfType(type);
        }

        public void UpdateObjectiveOfCollectableType(string collectableType, int amountToAdd) {

            if (_objectivesManager == null || _objectivesData.Count == 0) {
                return;
            }

            _objectivesManager.UpdateObjectiveOfCollectableType(collectableType, amountToAdd);
        }

        public void InitObjectivesManager(ref List<string> completedObjectivesId) {

            var objectivesList = new List<IObjective>();
            foreach (var data in _objectivesData) {
                // todo: use objective id for marking something as done?
                if (completedObjectivesId.Contains(data.uniqueId)) {
                    Debug.Log($"objective {data.uniqueId} marked as completed");
                    continue;
                }
                IObjective objective = CreateObjective(data);
                if (objective != null) {
                    objective.Initialize(data);
                    objectivesList.Add(objective);
                }
            }

            _objectivesManager ??= new ObjectivesManager();
            _objectivesManager.Initialize(objectivesList, ref completedObjectivesId);
        }

        private IObjective CreateObjective(ObjectiveDataSO data) {

            Type objectiveType = GetObjectiveType(data);
            if (objectiveType != null) {
                if (typeof(IObjective).IsAssignableFrom(objectiveType)) {
                    IObjective objective = (IObjective)Activator.CreateInstance(objectiveType);
                    return objective;
                } else {
                    Debug.LogError("Invalid objective type: " + objectiveType.Name);
                }
            } else {
                Debug.LogError("Failed to determine objective type for: " + data.GetType().Name);
            }
            return null;
        }

        private Type GetObjectiveType(ObjectiveDataSO data) {

            Type objectiveDataType = data.GetType();
            var attribute = (TypeMarkerMultiClassAttribute)Attribute.GetCustomAttribute(objectiveDataType, typeof(TypeMarkerMultiClassAttribute));

            if (attribute != null) {
                return attribute.type;
            } else {
                Debug.LogError("No ObjectiveTypeAttribute found for ObjectiveDataSO: " + objectiveDataType.Name);
                return null;
            }
        }

        public void UpdateObjectives() {

            if (_objectivesManager == null) {
                return;
            }

            _objectivesManager.UpdateObjectives();
        }

        public List<string> GetCompletedObjectivesUniqueId() {

            if (_objectivesManager == null) {
                return null;
            }

            return _objectivesManager.GetCompletedObjectivesIds();
        }
    }

}
