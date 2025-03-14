using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.GameCore;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.ConsumeableItems;
using Code.Wakoz.PurrKurr.Screens.CameraSystem;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using Code.Wakoz.PurrKurr.Screens.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ObjectivesMissionHandler(GameplayController gameEvents, LevelsController controller)
        {
            _gameEvents = gameEvents;
            _controller = controller;
        }

        public void Bind()
        {
            if (_gameEvents == null)
                return;

            _gameEvents.OnObjectiveMissionStarted += HandleMissionStarted;
            _gameEvents.OnHeroReposition += HandleLoadingNewLevel;
        }

        public void Unbind()
        {
            if (_gameEvents == null)
                return;

            _gameEvents.OnObjectiveMissionStarted -= HandleMissionStarted;
            _gameEvents.OnHeroReposition -= HandleLoadingNewLevel;
        }

        public void Dispose()
        {
            _gameEvents = null;
        }

        private void HandleLoadingNewLevel(Vector3 vector)
        {
            CancelMission();
        }

        private void CancelMission()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void HandleMissionStarted(IObjective objectiveData, List<ObjectiveSequenceData> data)
        {
            CancelMission();

            var objectIds = objectiveData.TargetObjectIds();
            Debug.Log($"Mission started duration {TotalSequenceDuration(data)}: UniqueId {objectiveData.GetUniqueId()} - {objectIds.Count()} total ObjectIds");

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _ = StartMission(data, token);
        }

        private float TotalSequenceDuration(List<ObjectiveSequenceData> SequenceData)
            => SequenceData.Sum(data => data.WaitTimeInSeconds);

        private async Task StartMission(List<ObjectiveSequenceData> data, CancellationToken cancellationToken)
        {
            for (var i = 0; i < data.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await ProcessSequenceStep(data[i], cancellationToken);
            }
        }

        private async Task ProcessSequenceStep(ObjectiveSequenceData sequenceStep, CancellationToken cancellationToken)
        {
            Debug.Log($"Action {sequenceStep.ActionType} {sequenceStep.TargetObjectIds.Count()} {sequenceStep.StartObjectId}");

            switch (sequenceStep.ActionType)
            {
                case ObjectiveActionType.CameraFocus:

                    var focusItems = _controller.GetTaggedObject(sequenceStep.TargetObjectIds).ToArray();
                    var cameraFocus = new FollowSingleTarget(new CameraData(focusItems, new LifeCycleData(sequenceStep.WaitTimeInSeconds)));

                    SingleController.GetController<GameplayController>().Handlers.GetHandler<CameraCharacterHandler>().NotifyCameraChange(cameraFocus);
                break;

                case ObjectiveActionType.Destroy:
                case ObjectiveActionType.Spawn:

                    var itemsToSpawn = _controller.GetTaggedObject(sequenceStep.TargetObjectIds);
                    if (itemsToSpawn == null)
                        break;

                    foreach (var spawned in itemsToSpawn)
                    {
                        if (!string.IsNullOrEmpty(sequenceStep.StartObjectId))
                        {
                            var spawnItem = _controller.GetTaggedObject(sequenceStep.StartObjectId);
                            if (spawnItem != null)
                            {
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
            if (waitTime > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(waitTime));
            }
        }

        private void ResetSpawnedObject(Transform spawned)
        {
            // todo: consider strategy pattern for objectsStateReset (Revival, Refill consumeable)
            if (TryReviveCharacter(spawned))
            {
                return;
            }

            TryRefillConsumeable(spawned);
        }

        private bool TryReviveCharacter(Transform spawned)
        {
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

        private bool TryRefillConsumeable(Transform spawned)
        {
            var consumeable = spawned.GetComponent<ConsumeableItemController>();
            if (consumeable == null)
                return false;

            consumeable.SetQuantity(consumeable.MaxQuantity);
            return true;
        }
    }
}