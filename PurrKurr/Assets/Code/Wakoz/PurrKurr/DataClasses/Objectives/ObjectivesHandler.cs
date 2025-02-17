using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{

    public class ObjectivesHandler : IBindableHandler
    {
        private ObjectiveFactory _objectiveFactory; // Handles objective creation
        private List<IObjective> _objectives = new();
        private HashSet<string> _completedObjectivesId = new(); // Tracks completed objectives efficiently
        
        private GameplayController _gameEvents;

        public ObjectivesHandler(GameplayController gameEvents)
        {
            _gameEvents = gameEvents;
            _objectiveFactory = new ObjectiveFactory();
        }

        public void Bind()
        {
            if (_gameEvents == null)
                return;

            _gameEvents.OnHeroEnterDetectionZone += HandleHeroEnterDetectionZone;
        }

        public void Unbind()
        {
            if (_gameEvents == null)
                return;

            _gameEvents.OnHeroEnterDetectionZone -= HandleHeroEnterDetectionZone;
        }

        public void Dispose()
        {
            _gameEvents = null;
        }

        /// <summary>
        /// Initializes objectives for the level.
        /// Marks completed objectives after creation as finished.
        /// </summary>
        public void Initialize(List<ObjectiveDataSO> objectivesData)
        {
            _objectives.Clear();

            foreach (var data in objectivesData)
            {
                var objective = _objectiveFactory.Create(data);
                if (objective != null)
                {
                    objective.Initialize(data);
                    _objectives.Add(objective);

                    // Marks the objective as completed
                    if (_completedObjectivesId.Contains(data.Objective.UniqueId))
                    {
                        Debug.Log($"Objective {data.Objective.UniqueId} is already completed. Marking as complete.");
                        objective.Finish();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the progress of objectives related to a specific collectable type.
        /// </summary>
        public void UpdateObjectiveOfCollectableType(string collectableType, int amountToAdd)
        {
            foreach (var objective in _objectives)
            {
                if (objective.GetObjectType() == collectableType && !_completedObjectivesId.Contains(objective.GetUniqueId()))
                {
                    Debug.Log($"Updating Objective: increased by {amountToAdd} for {objective.GetObjectiveDescription()} ");
                    objective.UpdateProgress(amountToAdd);
                }
            }
            UpdateCompletedObjectives();
        }

        /// <summary>
        /// Marks objectives of a specific type as complete.
        /// </summary>
        public void CompleteObjectiveOfType(Type type)
        {
            foreach (var objective in _objectives)
            {
                if (objective.GetType() == type && !_completedObjectivesId.Contains(objective.GetUniqueId()))
                {
                    Debug.Log($"Force completing Objective: {objective.GetObjectiveDescription()}");
                    objective.Finish();
                    _completedObjectivesId.Add(objective.GetUniqueId());
                }
            }
        }

        /// <summary>
        /// Updates the list of completed objectives.
        /// </summary>
        public void UpdateCompletedObjectives()
        {
            foreach (var objective in _objectives)
            {
                if (objective.IsComplete() && !_completedObjectivesId.Contains(objective.GetUniqueId()))
                {
                    Debug.Log($"Objective Completed: {objective.GetObjectiveDescription()}");
                    _completedObjectivesId.Add(objective.GetUniqueId());
                }
            }
        }

        private void HandleHeroEnterDetectionZone(DetectionZoneTrigger zone)
        {
            switch (zone)
            {
                case DoorController door:

                    if (!door.IsDoorEntrance())
                    {
                        CompleteObjectiveOfType(typeof(ReachTargetZoneObjective));
                    }
                    break;

                case CollectableItemController collectableItem:
                    UpdateObjectiveOfCollectableType(collectableItem.GetId(), collectableItem.GetQuantity());
                    break;

                case not OverlayWindowTrigger:
                    Debug.Log($"Unhandled detection zone type: {zone.GetType()}");
                    break;
            }
        }

        /// <summary>
        /// Gets all objective by their IDs for saving progression
        /// </summary>
        /// <returns></returns>
        public List<string> GetObjectives()
        {
            var result = new List<string>();
            foreach (var objective in _objectives)
            {
                result.Add(objective.GetUniqueId());
            }
            return result;
        }
    }
}