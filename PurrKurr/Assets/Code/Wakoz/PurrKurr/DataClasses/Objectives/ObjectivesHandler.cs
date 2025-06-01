using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.PurrKurr.Screens.Levels;
using Code.Wakoz.PurrKurr.Screens.Objectives;
using Code.Wakoz.Utils.BitwiseUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{

    public class ObjectivesHandler : IBindableHandler
    {
        public event Action OnNewObjectives;
        public event Action OnObjectiveUpdated;

        private HashSet<string> _completedObjectivesId = new();
        [Tooltip("Stores the bitmask for collected states of each item in TargetObjectIds")]
        private Dictionary<string, int[]> _objectivesOngoing = new();
        private bool _hasCollectedAny = false;

        private ObjectiveFactory _objectiveFactory;
        private List<IObjective> _objectives = new();

        private GameplayController _gameEvents;
        private ObjectivesController _controller;

        public ObjectivesHandler(GameplayController gameEvents, ObjectivesController controller) {
            
            _objectiveFactory = new ObjectiveFactory();

            _gameEvents = gameEvents;
            _controller = controller;
        }

        public void Bind() {
            
            if (_gameEvents == null)
                return;

            _gameEvents.OnHeroEnterDetectionZone += HandleHeroEnterDetectionZone;
            _gameEvents.OnInteractableDestoyed += HandleInteractableDestroyed;
        }

        public void Unbind() {
            
            if (_gameEvents == null)
                return;

            _gameEvents.OnHeroEnterDetectionZone -= HandleHeroEnterDetectionZone;
            _gameEvents.OnInteractableDestoyed -= HandleInteractableDestroyed;
        }

        public void Dispose() {

            _gameEvents = null;
        }

        public void SetOngoingObjectivesData(Dictionary<string, int[]> objectivesOngoing) 
            => _objectivesOngoing = objectivesOngoing;

        public void SetCompletedObjectivesData(HashSet<string> completedObjectivesId) 
            => _completedObjectivesId = completedObjectivesId;

        public (Dictionary<string, int[]> objectivesOngoing, 
            HashSet<string> completedObjectivesId)
            GetAllData() => (_objectivesOngoing, _completedObjectivesId);

        /// <summary>
        /// Initializes objectives for the level.
        /// Marks completed objectives after creation as finished.
        /// If an objective has existing progress in _objectivesOngoing, it initialized with the progress.
        /// </summary>
        public void Initialize(List<ObjectiveDataSO> objectivesData) {

            _objectives.Clear();

            foreach (var data in objectivesData) {
                var objective = _objectiveFactory.Create(data);
                if (objective != null) {
                    objective.Initialize(data);
                    _objectives.Add(objective);

                    // Check for the objective's existing progress in _objectivesOngoing
                    if (_objectivesOngoing.TryGetValue(data.Objective.UniqueId, out var existingProgress)) {
                        objective.UpdateProgress(existingProgress.CountActiveBits());
                    }
                    // Mark objective as completed
                    else if (_completedObjectivesId.Contains(data.Objective.UniqueId)) {
                        Debug.Log($"Objective {data.Objective.UniqueId} is already completed. Marking as complete.");
                        objective.Finish();
                    }
                }
            }

            SortObjectives();
            NotifyNewObjectives();
        }

        public float GetPercentComplete(List<ObjectiveDataSO> objectivesData) {

            var result = 0f;
            var totalItemIdsCount = 0;

            if (objectivesData == null || objectivesData.Count == 0) 
                return result;

            foreach (var data in objectivesData) {

                if (data == null) {
                    Debug.LogWarning("Empty ref to Objective data");
                    continue;
                }

                var objectiveIds = data.Objective.UniqueId;
                if (objectiveIds != null) {

                    totalItemIdsCount += data.Objective.RequiredQuantity;
                    // Check for the objective's existing progress in _objectivesOngoing
                    if (_objectivesOngoing.TryGetValue(objectiveIds, out var existingProgress)) {
                        result += existingProgress.CountActiveBits();
                    }
                    // Mark objective as completed
                    else if (_completedObjectivesId.Contains(objectiveIds)) {
                        result += data.Objective.RequiredQuantity;
                    }

                    //var percentComplete = Mathf.Clamp01((float)objectiveIds.GetCurrentQuantity() / objectiveIds.GetRequiredQuantity());
                    //percentComplete = Mathf.CeilToInt((percentComplete / totalObjectives) * 100);
                }
                /*var objective = _objectiveFactory.Create(data);
                if (objective != null) {
                    objective.Initialize(data);

                    // Check for the objective's existing progress in _objectivesOngoing
                    if (_objectivesOngoing.TryGetValue(data.Objective.UniqueId, out var existingProgress)) {
                        objective.UpdateProgress(existingProgress.CountActiveBits());
                    }
                    // Mark objective as completed
                    else if (_completedObjectivesId.Contains(data.Objective.UniqueId)) {
                        Debug.Log($"Objective {data.Objective.UniqueId} is already completed. Marking as complete.");
                        objective.Finish();
                    }

                    var percentComplete = Mathf.Clamp01((float)objective.GetCurrentQuantity() / objective.GetRequiredQuantity());
                    percentComplete = Mathf.CeilToInt((percentComplete / totalObjectives) * 100);
                }*/
            }
            totalItemIdsCount = totalItemIdsCount > 0 ? totalItemIdsCount : 1;
            return Mathf.Clamp01(result / totalItemIdsCount);
        }

        /// <summary>
        /// Returns true if an itemId exist, therefore collected
        /// </summary>
        /// <param name="collectableId"></param>
        /// <returns></returns>
        public bool IsCollected(string itemId) {

            for (var i = _objectives.Count - 1; i >= 0; i--) {
                var objective = _objectives[i];

                if (!objective.TargetObjectIds().Contains(itemId)) {
                    continue;
                }

                var uniqueId = objective.GetUniqueId();
                if (IsObjectiveCompleted(uniqueId)) {
                    return true;
                }

                if (!_objectivesOngoing.TryGetValue(uniqueId, out var bitmaskArray)) {
                    return false;
                }

                var indexInOnbjectiveIds = Array.IndexOf(objective.TargetObjectIds(), itemId);

                return bitmaskArray.IsBitSet(indexInOnbjectiveIds);
            }

            return false;
        }

        public bool IsObjectiveCompleted(string uniqueId) 
            => _completedObjectivesId.Contains(uniqueId);

        public bool HasAnyObjectiveProgress() 
            => _hasCollectedAny;

        /// <summary>
        /// Gets all active objective
        /// </summary>
        /// <returns></returns>
        public List<IObjective> GetObjectives() 
            => _objectives;

        /// <summary>
        /// Returns an objective by UniqueId
        /// </summary>
        /// <param name="objectiveUniqueId"></param>
        /// <returns></returns>
        public IObjective GetObjectiveByUniqueId(string objectiveUniqueId) {

            var objectiveData = _objectives.FirstOrDefault(o => o != null && o.GetUniqueId() == objectiveUniqueId);

            if (objectiveData == null) {
                Debug.LogError($"Mission with uniqueId '{objectiveUniqueId}' is not found");
            }

            return objectiveData;
        }

        /// <summary>
        /// Updates the progress of objectives related to a specific collectable type.
        /// </summary>
        public void UpdateObjectiveOfCollectableType(string collectableId, int amountToAdd) {
            
            var hasChanges = false;
            foreach (var objective in _objectives) {
                var uniqueId = objective.GetUniqueId();
                if (!_completedObjectivesId.Contains(uniqueId) && objective.TargetObjectIds().Contains(collectableId)) {
                    var indexInOnbjectiveIds = Array.IndexOf(objective.TargetObjectIds(), collectableId);
                    // Get or create the bitmask array for this objective in _objectivesOngoing
                    if (!_objectivesOngoing.TryGetValue(uniqueId, out var bitmaskArray)) {
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
            if (hasChanges) {
                SortObjectives();
                NotifyObjectivesChanged();
                UpdateCompletedObjectives();
                NotifyTaggedDependentObjects(collectableId);
            }
        }

        /// <summary>
        /// Marks objectives of a specific type as complete
        /// </summary>
        public void CompleteObjectivesOfType(Type type) {

            var hasChanges = false;
            foreach (var objective in _objectives) {
                if (objective.GetType() == type && !_completedObjectivesId.Contains(objective.GetUniqueId())) {
                    var uniqueId = objective.GetUniqueId();
                    EnsureOngoingObjectiveRemoved(uniqueId);

                    Debug.Log($"Force completing Objective: {objective.GetObjectiveDescription()}");
                    objective.Finish();
                    _completedObjectivesId.Add(uniqueId);
                    NotifyTaggedDependentObjects(uniqueId);

                    hasChanges = true;
                }
            }
            if (hasChanges) {
                SortObjectives();
                NotifyObjectivesChanged();
            }
        }

        private void EnsureOngoingObjectiveRemoved(string uniqueId) {

            if (_objectivesOngoing.ContainsKey(uniqueId)) {
                _objectivesOngoing.Remove(uniqueId);
            }
        }

        /// <summary>
        /// Sort logic for objectives.
        /// - Sort by objective progress (higher progress first)
        /// - Completed objectives go to the end
        /// </summary>
        private void SortObjectives() {

            _objectives = _objectives
                .OrderByDescending(obj => !obj.IsComplete())
                .ThenByDescending(obj => (float)obj.GetCurrentQuantity() / obj.GetRequiredQuantity())
                .ToList();
        }

        private void NotifyObjectivesChanged() {

            _hasCollectedAny = true;
            // change the controller to register to the event instead of calling the method
            _controller.HandleObjectivesChanged(_objectives);
            OnObjectiveUpdated?.Invoke();
        }

        private void NotifyNewObjectives() {

            _hasCollectedAny = false;
            // change the controller to register to the event instead of calling the method
            _controller.HandleNewObjectives(_objectives);
            OnNewObjectives?.Invoke();
        }

        /// <summary>
        /// Notify the dependent objects that are tagged with the uniqueId (like itemId or objectiveId)
        /// </summary>
        /// <param name="uniqueId"></param>
        private void NotifyTaggedDependentObjects(string uniqueId) {

            SingleController.GetController<LevelsController>().NotifyTaggedDependedObjects(uniqueId);
        }
        
        /// <summary>
        /// Updates the list of completed objectives.
        /// </summary>
        private void UpdateCompletedObjectives() {

            foreach (var objective in _objectives) {
                var uniqueId = objective.GetUniqueId();
                if (objective.IsComplete() && !_completedObjectivesId.Contains(uniqueId)) {
                    Debug.Log($"Objective Completed: {objective.GetObjectiveDescription()}");
                    _completedObjectivesId.Add(uniqueId);
                    EnsureOngoingObjectiveRemoved(uniqueId);
                    NotifyTaggedDependentObjects(uniqueId);
                }
            }
        }

        private void HandleHeroEnterDetectionZone(DetectionZoneTrigger zone) {

            switch (zone) {
                case DoorController door:

                    var collectable = door.CollectableData;
                    if (collectable != null && !string.IsNullOrEmpty(collectable.ItemId)) {
                        UpdateObjectiveOfCollectableType(collectable.ItemId, 1);
                    }
                    break;

                case CollectableItemController collectableItem:
                    var (id, quantity) = collectableItem.GetData();
                    UpdateObjectiveOfCollectableType(id, quantity);
                    break;

                case ObjectiveZoneTrigger objectiveZone:
                    // RunObjectiveMission(objectiveZone.ObjectiveUniqueId);
                    break;

                case not OverlayWindowTrigger:
                    Debug.Log($"Unhandled detection zone type: {zone.GetType()}");
                    break;
            }
        }

        private void HandleInteractableDestroyed(IInteractableBody interactableBody) {

            var collectableItem = interactableBody.GetTransform().GetComponent<CollectableTaggedItem>();

            if (collectableItem == null)
                return;

            var (id, quantity) = collectableItem.GetData();
            Debug.Log($"CollectableTaggedItem '{id}' {quantity} Interactable destroyed {collectableItem.gameObject.name}");

            UpdateObjectiveOfCollectableType(id, quantity);
        }

    }
}