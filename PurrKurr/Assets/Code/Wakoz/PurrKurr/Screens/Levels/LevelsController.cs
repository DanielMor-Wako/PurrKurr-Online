using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using Code.Wakoz.PurrKurr.Screens.PersistentGameObjects;
using Code.Wakoz.PurrKurr.Views;
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

        private InteractablesController _interactablesPool;
        private int _currentLevelIndex = -1;
        private PersistentGameObjectsManager _persistentGameObjectsManager;

        public int CurrentLevelIndex => _currentLevelIndex;

        public void InitInteractablePools(InteractablesController interactablesPool) {

            _interactablesPool = interactablesPool;

            _interactablesPool.CreateObjectPool(_interactablesPool._projectiles.FirstOrDefault(), 0, 5, "Projectiles");
            _interactablesPool.CreateObjectPool(_interactablesPool._ropes.FirstOrDefault(), 0, 1, "Ropes");
        }

        public void LoadLevel(int levelToLoad) {

            _currentLevelIndex = levelToLoad;

            SetSingleLevel(levelToLoad);
        }

        public LevelController GetLevel(int index)
        {
            if (_levelControllers == null)
            {
                Debug.LogError("Level Controllers has no ref");
                return null;
            }

            if (index < 0 || index >= _levelControllers.Count)
            {
                Debug.LogError($"Error no level with Index {index}");
                return null;
            }

            return _levelControllers[index];
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

        public void NotifyAllTaggedObjects()
        {
            _persistentGameObjectsManager.NotifyAll();
        }

        public void NotifyTaggedDependedObjects(string itemId) {

            _persistentGameObjectsManager.NotifyTaggedDependedObjects(itemId);
        }

        public List<Transform> GetTaggedObjectsOfType(Type type)
        {
            List<Transform> result= new();

            var taggedObjects = _persistentGameObjectsManager.GetTaggedObjectsOfType(type);
            foreach (var item in taggedObjects)
            {
                result.Add(item.ObjectTransform);
            }

            return result;
        }

        public List<Transform> GetTaggedObjectsOfType<T>() where T : ITaggable
        {
            List<Transform> result = new();

            var taggedObjects = _persistentGameObjectsManager.GetTaggedObjectsOfType<T>();
            foreach (var item in taggedObjects)
            {
                result.Add(item.ObjectTransform);
            }

            return result;
        }

        public List<Transform> GetTaggedObject(List<string> itemIds)
        {
            List<Transform> result = new();
            foreach (var itemId in itemIds)
            {
                var item = GetTaggedObject(itemId);
                if (item == null)
                {
                    continue;
                }
                result.Add(item);
            }
            return result;
        }

        public Transform GetTaggedObject(string itemId)
        {
            var retrievedItem = _persistentGameObjectsManager.GetTaggedObject(itemId);
            return retrievedItem != null ? retrievedItem.ObjectTransform : null;
        }

        protected override void Clean() {

            CleanPersistentGameObjectsManager();

            CleanInteractablesPool();
        }

        protected override Task Initialize() {

            InitPersistentGameObjectsManager();

            return Task.CompletedTask;
        }

        private void InitPersistentGameObjectsManager()
        {
            _persistentGameObjectsManager ??= new PersistentGameObjectsManager();
        }

        private void CleanPersistentGameObjectsManager()
        {
            _persistentGameObjectsManager.Dispose();
        }

        private void CleanInteractablesPool()
        {
            if (_interactablesPool == null)
            {
                return;
            }

            // todo? remove created pools and their objects
        }

        private void SetSingleLevel(int levelToLoad) {
            
            if (_levels == null) {
                return;
            }

            _levels.ChangeState(levelToLoad);
        }

    }
}
