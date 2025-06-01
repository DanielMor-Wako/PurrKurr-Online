using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.ConsumeableItems;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using Code.Wakoz.PurrKurr.Screens.CameraSystem;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.PurrKurr.Screens.Levels;
using Code.Wakoz.PurrKurr.Screens.SceneTransition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{

    public class ObjectivesMissionHandler : IBindableHandler
    {
        private GameplayController _gameEvents;
        private LevelsController _controller;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isActiveDelayBeforeMissionFailed;
        private bool _heroHasFailed;
        private float _missionSecondsLeft = 0;

        public ObjectivesMissionHandler(GameplayController gameEvents, LevelsController controller) {
            _gameEvents = gameEvents;
            _controller = controller;
        }

        public void Bind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnObjectiveMissionStarted += HandleMissionStarted;
            _gameEvents.OnHeroReposition += HandleLoadingNewLevel;
            _gameEvents.OnHeroDeath += HandleHeroFailed;
        }

        public void Unbind() {

            if (_gameEvents == null)
                return;

            _gameEvents.OnObjectiveMissionStarted -= HandleMissionStarted;
            _gameEvents.OnHeroReposition -= HandleLoadingNewLevel;
            _gameEvents.OnHeroDeath -= HandleHeroFailed;
        }

        public void Dispose() {

            _gameEvents = null;
        }

        public bool HasActiveMission() 
            => _cancellationTokenSource != null ?
            !_cancellationTokenSource.IsCancellationRequested : false;

        private void HandleLoadingNewLevel(Vector3 vector) {

            CancelMission();
            _gameEvents.InvokeHideTimer();
        }

        private async void HandleHeroFailed() {

            if (_isActiveDelayBeforeMissionFailed || _heroHasFailed) {
                return;
            }

            _isActiveDelayBeforeMissionFailed = true;

            await Task.Delay(TimeSpan.FromSeconds(4));

            _heroHasFailed = true;
        }

        private void CancelMission() {

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        private async void HandleMissionStarted(IObjective objectiveData, ObjectiveSequenceDataSO sequenceSOData) {
            
            CancelMission();

            _isActiveDelayBeforeMissionFailed = false;
            _heroHasFailed = false;

            var initData = sequenceSOData.InitializeData;
            var data = sequenceSOData.SequenceData;
            var finishConditions = sequenceSOData.FinishConditions;

            var objectIds = objectiveData.TargetObjectIds();

            var missionDuration = (TotalSequenceDuration(data));
            Debug.Log($"Mission started duration {missionDuration}: UniqueId {objectiveData.GetUniqueId()} - {objectIds.Count()} total ObjectIds");
            _gameEvents.InvokeNewNotification($"New Mission\n'' {sequenceSOData.MissionTitle} ''");

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            await StartMission(initData, token);

            _ = StartMission(data, token);
            _ = CheckFinishConditionsPeriodically(finishConditions, initData, 1, token);
            _ = SetCountdownTimer(missionDuration, token);
        }

        private async Task SetCountdownTimer(float durationInSeconds, CancellationToken token) {

            _missionSecondsLeft = durationInSeconds;

            while (!token.IsCancellationRequested) {

                _gameEvents.InvokeUpdateTimer(_missionSecondsLeft, durationInSeconds);

                await Task.Delay(TimeSpan.FromSeconds(1), token);

                _missionSecondsLeft--;

                if (!_isActiveDelayBeforeMissionFailed && _missionSecondsLeft < 0) {
                    HandleHeroFailed();
                    _gameEvents.InvokeNewNotification("Time Over", 3);
                }
            }

        }

        private async Task CheckFinishConditionsPeriodically(ObjectiveFinishConditionsData finishConditions, List<ObjectiveSequenceData> initData, int checkRateInSeconds, CancellationToken token) {
            
            var winConditions = finishConditions.WinConditions;
            var winConditionsObjects = new Dictionary<string, Transform>();
            foreach (var winCondition in winConditions) {
                foreach (var objectId in winCondition.targetObjectIds) {
                    var objectsById = _controller.GetTaggedObject(objectId);
                    if (objectsById == null)
                        continue;

                    winConditionsObjects.Add(objectId, objectsById);
                }
            }

            var loseConditions = finishConditions.LoseConditions;
            var loseConditionsObjects = new Dictionary<string, Transform>();
            foreach (var loseCondition in loseConditions) {
                foreach (var objectId in loseCondition.targetObjectIds) {
                    var objectsById = _controller.GetTaggedObject(objectId);
                    if (objectsById == null)
                        continue;

                    loseConditionsObjects.Add(objectId, objectsById);
                }
            }

            if (winConditionsObjects.Count == 0 || loseConditionsObjects.Count == 0) {
                Debug.LogWarning("Objective Finish Conditions are missing");
            }

            while (!token.IsCancellationRequested) {

                var isLoseConditionMet = ProcessAllConditionsMet(loseConditions, loseConditionsObjects);
                if (isLoseConditionMet) {
                    HandleHeroFailed();
                }

                if (_heroHasFailed) {

                    CancelMission();
                    _gameEvents.InvokeHideTimer();
                    ResetLevelState(() => {
                        _ = StartMission(initData, new CancellationToken());
                        SingleController.GetController<GameplayController>().ReviveHeroes();
                    });
                    break;
                }

                var isWinConditionMet = ProcessAllConditionsMet(winConditions, winConditionsObjects);
                if (isWinConditionMet) {

                    var allObject = winConditionsObjects.Concat(loseConditionsObjects).ToDictionary(x => x.Key, x => x.Value);
                    OnMissionComplete(finishConditions, allObject);
                    CancelMission();
                    _gameEvents.InvokeHideTimer();
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(checkRateInSeconds));
            }
        }

        private void ResetLevelState(Action actionOnEnd) {
            SingleController.GetController<SceneTransitionController>().PretendLoad(2.5f, "Mission Failed",
                () => {
                    actionOnEnd.Invoke();
                });
        }

        private void OnMissionComplete(ObjectiveFinishConditionsData finishConditions, Dictionary<string, Transform> objectIds) {

            var stringBuilder = new StringBuilder($"Mission Complete\n{TimeSpan.FromSeconds(_missionSecondsLeft).ToString(@"mm\:ss")} Left");
            var stringBuilderWin = new StringBuilder();
            var stringBuilderLose = new StringBuilder();
            
            ConcludeMissionConditions(finishConditions.WinConditions, ref stringBuilderWin, ref objectIds);
            ConcludeMissionConditions(finishConditions.LoseConditions, ref stringBuilderLose, ref objectIds);

            _gameEvents.InvokeNewNotification(stringBuilder.ToString());
            _gameEvents.InvokeNewNotification(stringBuilderWin.ToString());
            _gameEvents.InvokeNewNotification(stringBuilderLose.ToString());

            Debug.Log(stringBuilder);
        }

        private void ConcludeMissionConditions(List<ObjectiveConditionData> conditions, ref StringBuilder stringBuilder, ref Dictionary<string, Transform> objectIds) {

            foreach (var data in conditions) {
                switch (data.Condition) {

                    case ObjectiveEndCondition.IsAlive:
                    case ObjectiveEndCondition.isDead:

                        stringBuilder.AppendLine($"Smashed {data.targetObjectIds.Count}");
                        break;

                    case ObjectiveEndCondition.IsConsumeable:
                    case ObjectiveEndCondition.IsConsumed:

                        var totalConsumed = 0f;
                        var totalItems = 0;

                        foreach (var targetObjectId in data.targetObjectIds) {

                            var consumeable = objectIds[targetObjectId].GetComponent<ConsumeableItemController>();
                            if (consumeable == null)
                                continue;

                            totalItems++;
                            totalConsumed += (float)consumeable.GetData().quantity / consumeable.MaxQuantity;
                        }

                        var total = Mathf.Floor(totalConsumed / totalItems * 100);
                        stringBuilder.AppendLine($"Protected {total} %");
                        break;
                }
            }
        }

        private bool ProcessAnyConditionsMet(List<ObjectiveConditionData> conditions, Dictionary<string, Transform> objectIds) {

            foreach (var data in conditions) {
                switch (data.Condition) {
                    case ObjectiveEndCondition.isDead:
                    case ObjectiveEndCondition.IsAlive:
                        foreach (var targetObjectId in data.targetObjectIds) {
                            var asInteractable = objectIds[targetObjectId].GetComponent<IInteractableBody>();
                            if (asInteractable == null)
                                continue;

                            var interactableHealth = asInteractable.GetHpPercent();

                            if (data.Condition is ObjectiveEndCondition.IsAlive && interactableHealth > 0 ||
                                data.Condition is ObjectiveEndCondition.isDead && interactableHealth == 0) {
                                return true;
                            }
                        }
                        break;

                    case ObjectiveEndCondition.IsConsumed:
                    case ObjectiveEndCondition.IsConsumeable:
                        foreach (var targetObjectId in data.targetObjectIds) {
                            var asInteractable = objectIds[targetObjectId].GetComponent<ConsumeableItemController>();
                            if (asInteractable == null)
                                continue;

                            var consumeableQuantity = asInteractable.GetData().quantity;

                            if (data.Condition is ObjectiveEndCondition.IsConsumeable && consumeableQuantity > 0 ||
                                data.Condition is ObjectiveEndCondition.IsConsumed && consumeableQuantity == 0) {
                                return true;
                            }
                        }
                        break;
                }
            }

            return false;
        }

        private bool ProcessAllConditionsMet(List<ObjectiveConditionData> conditions, Dictionary<string, Transform> objectIds) {

            foreach (var data in conditions) {
                switch (data.Condition) {
                    case ObjectiveEndCondition.isDead:
                    case ObjectiveEndCondition.IsAlive:
                        foreach (var targetObjectId in data.targetObjectIds) {
                            var asInteractable = objectIds[targetObjectId].GetComponent<IInteractableBody>();
                            if (asInteractable == null)
                                continue;

                            var interactableHealth = asInteractable.GetHpPercent();

                            if (data.Condition is ObjectiveEndCondition.IsAlive && interactableHealth == 0 ||
                                data.Condition is ObjectiveEndCondition.isDead && interactableHealth > 0) {
                                return false;
                            }
                        }
                        break;

                    case ObjectiveEndCondition.IsConsumed:
                    case ObjectiveEndCondition.IsConsumeable:
                        foreach (var targetObjectId in data.targetObjectIds) {
                            var asInteractable = objectIds[targetObjectId].GetComponent<ConsumeableItemController>();
                            if (asInteractable == null)
                                continue;

                            var consumeableQuantity = asInteractable.GetData().quantity;

                            if (data.Condition is ObjectiveEndCondition.IsConsumeable && consumeableQuantity == 0 ||
                                data.Condition is ObjectiveEndCondition.IsConsumed && consumeableQuantity > 0) {
                                return false;
                            }
                        }
                        break;
                }
            }

            return true;
        }

        private float TotalSequenceDuration(List<ObjectiveSequenceData> SequenceData) 
            => SequenceData.Sum(data => data.WaitTimeInSeconds);

        private async Task StartMission(List<ObjectiveSequenceData> data, CancellationToken cancellationToken) {
            for (var i = 0; i < data.Count; i++) {
                cancellationToken.ThrowIfCancellationRequested();

                await ProcessSequenceStep(data[i], cancellationToken);
            }
        }

        private async Task ProcessSequenceStep(ObjectiveSequenceData sequenceStep, CancellationToken cancellationToken) {
            //Debug.Log($"Action {sequenceStep.ActionType} {sequenceStep.TargetObjectIds.Count()} {sequenceStep.StartObjectId}");
            switch (sequenceStep.ActionType) {
                case ObjectiveActionType.CameraFocus:

                    var focusItems = _controller.GetTaggedObject(sequenceStep.TargetObjectIds);
                    var cameraFocus = new FollowSingleTarget(new CameraData(focusItems, new LifeCycleData(sequenceStep.WaitTimeInSeconds)));

                    SingleController.GetController<GameplayController>().Handlers.GetHandler<CameraCharacterHandler>().NotifyCameraChange(cameraFocus);
                    break;

                case ObjectiveActionType.Destroy:
                case ObjectiveActionType.Spawn:

                    var itemsToSpawn = _controller.GetTaggedObject(sequenceStep.TargetObjectIds);
                    if (itemsToSpawn == null)
                        break;

                    foreach (var spawned in itemsToSpawn) {
                        if (!string.IsNullOrEmpty(sequenceStep.StartObjectId)) {
                            var spawnItem = _controller.GetTaggedObject(sequenceStep.StartObjectId);
                            if (spawnItem != null) {
                                spawned.transform.position = spawnItem.transform.position;
                            }
                        }

                        if (sequenceStep.ActionType is not ObjectiveActionType.Spawn)
                            continue;

                        ResetSpawnedObject(spawned);
                    }
                    break;
            }

            var waitTime = sequenceStep.WaitTimeInSeconds;
            if (waitTime > 0) {
                await Task.Delay(TimeSpan.FromSeconds(waitTime));
            }
        }

        private void ResetSpawnedObject(Transform spawned) {
            // todo: consider strategy pattern for objectsStateReset (Revival, Refill consumeable)
            if (TryReviveCharacter(spawned)) {
                return;
            }

            TryRefillConsumeable(spawned);
        }

        private bool TryReviveCharacter(Transform spawned) {

            var interactableBody = spawned.GetComponent<IInteractableBody>();
            if (interactableBody == null)
                return false;

            var character = interactableBody as Character2DController;
            if (character == null)
                return false;

            character.Revive();
            character.State?.MarkLatestInteraction(null);
            return true;
        }

        private bool TryRefillConsumeable(Transform spawned) {

            var consumeable = spawned.GetComponent<ConsumeableItemController>();
            if (consumeable == null)
                return false;

            consumeable.SetQuantity(consumeable.MaxQuantity);
            return true;
        }
    }
}