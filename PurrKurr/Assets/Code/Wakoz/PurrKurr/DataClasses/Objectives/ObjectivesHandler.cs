using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.PurrKurr.Screens.Objectives;
using Code.Wakoz.Utils.BitwiseUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{

    public class ObjectivesHandler : IBindableHandler
    {
        private ObjectiveFactory _objectiveFactory; // Handles objective creation
        private List<IObjective> _objectives = new();
        private HashSet<string> _completedObjectivesId = new(); // Tracks completed objectives efficiently
        [Tooltip("Stores the bitmask for collected states of each item in TargetObjectIds")]
        private Dictionary<string, int[]> _objectivesOngoing = new(); // Tracks progress of ongoing objectives

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
        /// If an objective has existing progress in _objectivesOngoing, it initialized with the progress.
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

                    // Check if this objective has existing progress in _objectivesOngoing
                    if (_objectivesOngoing.TryGetValue(data.Objective.UniqueId, out var existingProgress))
                    {
                        objective.UpdateProgress(existingProgress.CountActiveBits());
                    }
                    // Mark objective as completed
                    else if (_completedObjectivesId.Contains(data.Objective.UniqueId))
                    {
                        Debug.Log($"Objective {data.Objective.UniqueId} is already completed. Marking as complete.");
                        objective.Finish();
                    }
                }
            }

            SortObjectives();
            NotifyNewObjectives();
        }

        /// <summary>
        /// Returns true if an itemId exist, therefore collected
        /// </summary>
        /// <param name="collectableId"></param>
        /// <returns></returns>
        public bool IsCollected(string itemId)
        {
            for (var i = _objectives.Count; i >= 0; i--)
            {
                var objective = _objectives[i];

                if (!objective.TargetObjectIds().Contains(itemId))
                {
                    continue;
                }

                var uniqueId = objective.GetUniqueId();
                if (_completedObjectivesId.Contains(uniqueId))
                {
                    return true;
                }

                if (!_objectivesOngoing.TryGetValue(uniqueId, out var bitmaskArray))
                {
                    return false;
                }

                var indexInOnbjectiveIds = Array.IndexOf(objective.TargetObjectIds(), itemId);

                return bitmaskArray.IsBitSet(indexInOnbjectiveIds);
            }

            return false;
        }

        /// <summary>
        /// Gets all active objective
        /// </summary>
        /// <returns></returns>
        public List<IObjective> GetObjectives()
        {
            return _objectives;
        }

        /// <summary>
        /// Sort logic for objectives.
        /// - Completed objectives go to the end
        /// - Sort by progress (higher progress first)
        /// </summary>
        private void SortObjectives()
        {
            _objectives = _objectives
                .OrderByDescending(obj => !obj.IsComplete())
                .ThenByDescending(obj => (float)obj.GetCurrentQuantity() / obj.GetRequiredQuantity())
                .ToList();
        }

        /// <summary>
        /// Updates the progress of objectives related to a specific collectable type.
        /// </summary>
        public void UpdateObjectiveOfCollectableType(string collectableId, int amountToAdd)
        {
            var hasChanges = false;
            foreach (var objective in _objectives)
            {
                var uniqueId = objective.GetUniqueId();
                if (!_completedObjectivesId.Contains(uniqueId) && objective.TargetObjectIds().Contains(collectableId))
                {
                    var indexInOnbjectiveIds = Array.IndexOf(objective.TargetObjectIds(), collectableId);
                    // Get or create the bitmask array for this objective in _objectivesOngoing
                    if (!_objectivesOngoing.TryGetValue(uniqueId, out var bitmaskArray))
                    {
                        bitmaskArray = new int[Mathf.CeilToInt(objective.TargetObjectIds().Length / 32f)];
                        _objectivesOngoing[uniqueId] = bitmaskArray;
                    }
                    // Update the bitmask and CurrentQuantity
                    bitmaskArray.SetBit(indexInOnbjectiveIds); // Mark item as collected
                    //objective.CurrentQuantity = BitwiseUtility.CountActiveBits(bitmaskArray); // Update CurrentQuantity

                    Debug.Log($"Updating Objective: increased by {amountToAdd} for {objective.GetObjectiveDescription()}");
                    objective.UpdateProgress(bitmaskArray.CountActiveBits());
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
        /// Marks objectives of a specific type as complete
        /// </summary>
        public void CompleteObjectivesOfType(Type type)
        {
            var hasChanges = false;
            foreach (var objective in _objectives)
            {
                if (objective.GetType() == type && !_completedObjectivesId.Contains(objective.GetUniqueId()))
                {
                    var uniqueId = objective.GetUniqueId();
                    // Remove from ongoing objectives
                    if (_objectivesOngoing.TryGetValue(uniqueId, out var bitmask))
                    {
                        _objectivesOngoing.Remove(uniqueId);
                    }

                    Debug.Log($"Force completing Objective: {objective.GetObjectiveDescription()}");
                    objective.Finish();
                    _completedObjectivesId.Add(uniqueId);

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
        private void UpdateCompletedObjectives()
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
                        UpdateObjectiveOfCollectableType("Door_Exit", 1);
                        //CompleteObjectivesOfType(typeof(ReachTargetZoneObjective));
                    }
                    break;

                case CollectableItemController collectableItem:
                    var (id, quantity) = collectableItem.GetData();
                    UpdateObjectiveOfCollectableType(id, quantity);
                    break;

                case not OverlayWindowTrigger:
                    Debug.Log($"Unhandled detection zone type: {zone.GetType()}");
                    break;
            }
        }

    }
}