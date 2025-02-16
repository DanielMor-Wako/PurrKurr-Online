using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.Utils.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives {

    public class ObjectivesHandler : IBindableHandler
    {

        private List<IObjective> _objectives;
        private List<string> _completedObjectivesId = new();

        private GameplayController _gameEvents;

        public ObjectivesHandler(GameplayController gameEvents)
        {
            _gameEvents = gameEvents;
        }

        public void Unbind()
        {
            if (_gameEvents == null)
                return;

            _gameEvents.OnHeroEnterDetectionZone -= HandleHeroEnterDetectionZone;
        }

        public void Bind()
        {
            if (_gameEvents == null)
                return;

            _gameEvents.OnHeroEnterDetectionZone += HandleHeroEnterDetectionZone;
        }

        public void Dispose()
        {
            _gameEvents = null;
        }

        public void Create(List<ObjectiveDataSO> objectives)
        {
            var objectivesList = new List<IObjective>();
            foreach (var data in objectives)
            {
                // todo: use objective id for marking something as done?
                if (_completedObjectivesId.Contains(data.uniqueId))
                {
                    Debug.Log($"objective {data.uniqueId} marked as completed");
                    continue;
                }
                IObjective objective = CreateObjective(data);
                if (objective != null)
                {
                    objective.Initialize(data);
                    objectivesList.Add(objective);
                }
            }

            Initialize(objectivesList, ref _completedObjectivesId);
        }

        private IObjective CreateObjective(ObjectiveDataSO data)
        {
            Type objectiveType = GetObjectiveType(data);
            if (objectiveType != null)
            {
                if (typeof(IObjective).IsAssignableFrom(objectiveType))
                {
                    IObjective objective = (IObjective)Activator.CreateInstance(objectiveType);
                    return objective;
                }
                else
                {
                    Debug.LogError("Invalid objective type: " + objectiveType.Name);
                }
            }
            else
            {
                Debug.LogError("Failed to determine objective type for: " + data.GetType().Name);
            }
            return null;
        }

        private Type GetObjectiveType(ObjectiveDataSO data)
        {

            Type objectiveDataType = data.GetType();
            var attribute = (TypeMarkerMultiClassAttribute)Attribute.GetCustomAttribute(objectiveDataType, typeof(TypeMarkerMultiClassAttribute));

            if (attribute != null)
            {
                return attribute.type;
            }
            else
            {
                Debug.LogError("No ObjectiveTypeAttribute found for ObjectiveDataSO: " + objectiveDataType.Name);
                return null;
            }
        }

        private void HandleHeroEnterDetectionZone(DetectionZoneTrigger zone)
        {
            switch (zone)
            {
                case DoorController:
                {

                    var door = zone as DoorController;

                    if (!door.IsDoorEntrance())
                    {
                        // _currentLevelIndex ??= door.GetNextDoor().GetRoomIndex()
                        CompleteObjectiveOfType(typeof(ReachTargetZoneObjective));
                    }

                    break;
                }

                case CollectableItemController:
                {

                    var collectableItem = zone as CollectableItemController;

                    UpdateObjectiveOfCollectableType(collectableItem.GetItemId(), collectableItem.GetItemQuantity());

                    break;
                }

                case not OverlayWindowTrigger:
                Debug.Log("No explicit interpreter for " + zone.GetType());
                break;
            }
        }

        public List<string> GetObjectives() 
        {
            var res = new List<string>();
            foreach (var objective in _objectives) {
                res.Add(objective.GetUniqueId());
            }
            return res;
        }

        public void Initialize(List<IObjective> objectivesList, ref List<string> completedObjectivesList) {

            _objectives = objectivesList;
            _completedObjectivesId = completedObjectivesList;
        }

        public void UpdateObjectiveOfCollectableType(string collectableType, int amountToAdd) {

            foreach (var objective in _objectives) {

                if (objective.GetObjectType() != collectableType) {
                    continue;
                }

                if (!objective.IsComplete() || !_completedObjectivesId.Contains(objective.GetUniqueId())) {

                    Debug.Log($"Objective: {objective.GetObjectiveDescription()} updated by {amountToAdd}");
                    objective.UpdateProgress(amountToAdd);

                    break;
                }
            }
            UpdateCompletedObjectives();
        }

        public void UpdateCompletedObjectives()
        {
            foreach (var objective in _objectives)
            {

                if (objective.IsComplete() && !_completedObjectivesId.Contains(objective.GetUniqueId()))
                {

                    _completedObjectivesId.Add(objective.GetUniqueId());
                    Debug.Log($"Objective: {objective.GetObjectiveDescription()} completed");
                }
            }
        }

        public void CompleteObjectiveOfType(Type type) {

            foreach (var objective in _objectives) {

                if (objective.GetType() != type) {
                    continue;
                }

                if (!objective.IsComplete() || !_completedObjectivesId.Contains(objective.GetUniqueId())) {

                    Debug.Log($"Objective: {objective.GetObjectiveDescription()} force completed");
                    objective.Finish();

                    _completedObjectivesId.Add(objective.GetUniqueId());
                }
            }
        }

    }
}