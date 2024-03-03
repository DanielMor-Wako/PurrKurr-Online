using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool;
using Code.Wakoz.PurrKurr.Views;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Levels {
    [DefaultExecutionOrder(12)]

    public class LevelsController : SingleController {

        [SerializeField] private MultiStateView _levels;

        private InteractablesController _interactablesPool;
        private int _currentLevelIndex = -1;


        public void InitInteractablePools(InteractablesController interactablesPool) {

            _interactablesPool = interactablesPool;

            _interactablesPool.CreateObjectPool(_interactablesPool._projectiles.FirstOrDefault(), 0, 5, "Projectiles");
            _interactablesPool.CreateObjectPool(_interactablesPool._ropes.FirstOrDefault(), 0, 1, "Ropes");
        }

        public void LoadLevel(int levelToLoad) {

            //var levelData = GetLevelData(levelToLoad);

            SetLevel(levelToLoad);
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

            if (_interactablesPool == null) {
                return;
            }

            // todo? remove created pools and their objects?
        }

        protected override Task Initialize() {
            return Task.CompletedTask;
        }

        private async Task GetLevelData(int levelToLoad) {
            
            if (_currentLevelIndex > -1) {
                // Unload previous level
            }
        }

        private void SetLevel(int levelToLoad) {
            
            if (_levels == null) {
                return;
            }

            _currentLevelIndex = levelToLoad;
            _levels.ChangeState(levelToLoad);
        }

    }
}
