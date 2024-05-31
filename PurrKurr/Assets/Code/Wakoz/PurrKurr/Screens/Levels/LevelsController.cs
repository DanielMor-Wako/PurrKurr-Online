using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger;
using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using Code.Wakoz.PurrKurr.Screens.PersistentGameObjects;
using Code.Wakoz.PurrKurr.Views;
using Code.Wakoz.Utils.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Levels {
    [DefaultExecutionOrder(12)]

    public class LevelsController : SingleController {

        [SerializeField] private List<LevelController> _levelControllers;
        [SerializeField] private MultiStateView _levels;

        List<string> _completeObjectives = new();

        private InteractablesController _interactablesPool;
        public int _currentLevelIndex = -1;
        private PersistentGameObjectsManager _persistentGameObjectsManager;

        private GameplayController _gameplayController;

        public void InitInteractablePools(InteractablesController interactablesPool) {

            _interactablesPool = interactablesPool;

            _interactablesPool.CreateObjectPool(_interactablesPool._projectiles.FirstOrDefault(), 0, 5, "Projectiles");
            _interactablesPool.CreateObjectPool(_interactablesPool._ropes.FirstOrDefault(), 0, 1, "Ropes");
        }

        public void BindEvents(GameplayController gameplayController)
        {
            UnBindEvents();
            _gameplayController = gameplayController;
            _gameplayController.OnHeroEnterDetectionZone += HandleHeroEnterDetectionZone;
        }

        private void UnBindEvents()
        {
            if (_gameplayController == null) {
                return;
            }
            _gameplayController.OnHeroEnterDetectionZone -= HandleHeroEnterDetectionZone;
            _gameplayController = null;
        }

        private void HandleHeroEnterDetectionZone(DetectionZoneTrigger zone) {

            switch (zone) {
                case DoorController: {

                        var door = zone as DoorController;

                        if (!door.IsDoorEntrance()) {
                            // _currentLevelIndex ??= door.GetNextDoor().GetRoomIndex()
                            CompleteObjectiveOfType(_currentLevelIndex, typeof(ReachTargetZoneObjective));
                        }

                        break;
                    }

                case CollectableItemController: {

                        var collectableItem = zone as CollectableItemController;
                        
                        UpdateObjectiveOfCollectableType(_currentLevelIndex, collectableItem.GetItemId(), collectableItem.GetItemQuantity());
                        
                        break;
                    }

                case not OverlayWindowTrigger:
                    Debug.Log("No explicit interpreter for " + zone.GetType());
                    break;
            }
        }
        // todo: make the classes that derive from DetectionZoneTrigger, use the TypeMarkerMultiClassAttribute to mark the class,
        // like in CollectableItemController or DoorController
        /*private void HandleHeroEnterDetectionZone(DetectionZoneTrigger zone) {
            Type zoneType = zone.GetType();
            Type objectiveType = GetObjectiveTypeForZone(zoneType);

            if (objectiveType != null) {
                CompleteObjective(_currentLevelIndex, objectiveType);
            } else if (zone is not OverlayWindowTrigger) {
                Debug.Log("No explicit interpreter for " + zoneType);
            }
        }

        private Type GetObjectiveTypeForZone(Type zoneType) {
            var attributes = zoneType.GetCustomAttributes(typeof(TypeMarkerMultiClassAttribute), true);
            if (attributes.Length > 0) {
                var attribute = (TypeMarkerMultiClassAttribute)attributes[0];
                return attribute.type;
            }
            return null;
        }*/

        public void LoadLevel(int levelToLoad) {

            //var levelData = GetLevelData(levelToLoad);

            _currentLevelIndex = levelToLoad;

            SetSingleLevel(levelToLoad);

            InitLevel(levelToLoad);
        }

        public void RefreshSpritesOrder(Character2DController mainHero) {

            var _heroes = _interactablesPool._heroes;

            if (_heroes == null) {
                return;
            }

            foreach (var character in _heroes) {

                if (character == null) {
                    continue;
                }

                RefreshSpriteOrder(character, character == mainHero);
            }
        }

        public void RefreshSpriteOrder(Character2DController character, bool isMainHero) {

            var isAlive = character.Stats.GetHealthPercentage() > 0;
            character.SetSpriteOrder(isMainHero ? 2 : isAlive ? 1 : 0);
        }


        protected override void Clean() {

            CleanPersistentGameObjectsManager();

            UnBindEvents();

            if (_interactablesPool == null) {
                return;
            }

            // todo? remove created pools and their objects?
        }

        protected override Task Initialize() {

            InitPersistentGameObjectsManager();

            return Task.CompletedTask;
        }

        private async Task GetLevelData(int levelToLoad) {
            
            if (_currentLevelIndex > -1) {
                // Unload previous level
            }
        }

        private void SetSingleLevel(int levelToLoad) {
            
            if (_levels == null) {
                return;
            }

            _levels.ChangeState(levelToLoad);
        }

        private void InitLevel(int levelToLoad) {

            if (_levelControllers == null) {
                return;
            }

            if (levelToLoad >= 0 && levelToLoad < _levelControllers.Count) {
                var levelController = _levelControllers[levelToLoad];
                if (levelController != null)
                {
                    Debug.Log("Initializing level " + levelToLoad);

                    //var completedObjectives = _completeObjectives[levelToLoad];
                    levelController.InitObjectivesManager(ref _completeObjectives);
                } else {
                    Debug.LogError("ObjectiveManagerScript for level " + levelToLoad + " is missing or null.");
                }
            }
            else
            {
                Debug.LogError("Invalid level index: " + levelToLoad);
            }
        }
        
        public List<string> GetCompletedObjectivesUniqueId(int levelIndex) {
            
            return _levelControllers[levelIndex].GetCompletedObjectivesUniqueId();
        }

        /*private void FixedUpdate() {

            _levelControllers[_currentLevelIndex].UpdateObjectives();
        }*/

        private void InitPersistentGameObjectsManager() {

            _persistentGameObjectsManager ??= new PersistentGameObjectsManager();
        }

        private void CleanPersistentGameObjectsManager() {

            _persistentGameObjectsManager.Dispose();
        }

        public void UpdateObjectiveOfCollectableType(int levelIndex, string collectableType, int amountToAdd) {

            var levelController = GetLevelController(levelIndex);

            if (levelController == null) {
                return;
            }

            levelController.UpdateObjectiveOfCollectableType(collectableType, amountToAdd);

            levelController.UpdateObjectives();
        }

        public void CompleteObjectiveOfType(int levelIndex, Type type) {

            var levelController = GetLevelController(levelIndex);

            if (levelController == null) {
                return;
            }
            
            levelController.CompleteObjectiveOfType(type);

            levelController.UpdateObjectives();
        }

        private LevelController GetLevelController(int levelIndex) {

            LevelController levelController = null;
            if (levelIndex >= 0 && levelIndex < _levelControllers.Count) {
                levelController = _levelControllers[levelIndex];
            }

            if (levelController == null) {
                Debug.LogError($"no level controller with Index {levelIndex}");
            }

            return levelController;
        }
    }
}
