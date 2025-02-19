using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.PurrKurr.Screens.Objectives;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{

    public class ObjectivesHandler : IBindableHandler
    {
        private ObjectiveFactory _objectiveFactory; // Handles objective creation
        private List<IObjective> _objectives = new();
        private HashSet<string> _completedObjectivesId = new(); // Tracks completed objectives efficiently
     
        private GameplayController _gameEvents;
        private ObjectivesController _controller;

        public ObjectivesHandler(GameplayController gameEvents, ObjectivesController controller)
        {
            _objectiveFactory = new ObjectiveFactory();

            _gameEvents = gameEvents;
            _controller = controller;
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

                    // Mark objective as completed
                    if (_completedObjectivesId.Contains(data.Objective.UniqueId))
                    {
                        Debug.Log($"Objective {data.Objective.UniqueId} is already completed. Marking as complete.");
                        objective.Finish();
                    }
                }
            }

            SortObjectives();
            NotifyNewObjectives();
        }

        private void SortObjectives()
        {
            _objectives = _objectives
                .OrderByDescending(obj => !obj.IsComplete()) // Completed objectives go to the end
                .ThenByDescending(obj => (float)obj.GetCurrentQuantity() / obj.GetRequiredQuantity()) // Sort by progress (higher progress first)
                .ToList();
        }

        /// <summary>
        /// Updates the progress of objectives related to a specific collectable type.
        /// </summary>
        public void UpdateObjectiveOfCollectableType(string collectableType, int amountToAdd)
        {
            var hasChanges = false;
            foreach (var objective in _objectives)
            {
                if (objective.GetObjectType() == collectableType && !_completedObjectivesId.Contains(objective.GetUniqueId()))
                {
                    Debug.Log($"Updating Objective: increased by {amountToAdd} for {objective.GetObjectiveDescription()} ");
                    objective.UpdateProgress(amountToAdd);
                    hasChanges = true;
                }
            }
            if (hasChanges)
            {
                SortObjectives();
                NotifyObjectivesChanged();
                UpdateCompletedObjectives();
            }
        }

        /// <summary>
        /// Marks objectives of a specific type as complete.
        /// </summary>
        public void CompleteObjectiveOfType(Type type)
        {
            var hasChanges = false;
            foreach (var objective in _objectives)
            {
                if (objective.GetType() == type && !_completedObjectivesId.Contains(objective.GetUniqueId()))
                {
                    Debug.Log($"Force completing Objective: {objective.GetObjectiveDescription()}");
                    objective.Finish();
                    _completedObjectivesId.Add(objective.GetUniqueId());
                    hasChanges = true;
                }
            }
            if (hasChanges)
            {
                SortObjectives();
                NotifyObjectivesChanged();
            }
        }

        private void NotifyObjectivesChanged()
        {
            _controller.HandleObjectivesChanged(_objectives);
        }

        private void NotifyNewObjectives()
        {
            _controller.HandleNewObjectives(_objectives);
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
        /// Gets all active objective
        /// </summary>
        /// <returns></returns>
        public List<IObjective> GetObjectives()
        {
            return _objectives;
        }
    }
}