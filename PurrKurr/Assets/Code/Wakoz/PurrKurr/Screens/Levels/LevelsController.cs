using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using Code.Wakoz.PurrKurr.Screens.PersistentGameObjects;
using Code.Wakoz.PurrKurr.Views;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.Progress;

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
            _persistentGameObjectsManager.RefreshStateAll();
        }
        
        public Transform GetTaggedObject(string itemId)
        {
            var tagged = _persistentGameObjectsManager.GetTaggedObject(itemId);
            return tagged.ObjectTransform;
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
