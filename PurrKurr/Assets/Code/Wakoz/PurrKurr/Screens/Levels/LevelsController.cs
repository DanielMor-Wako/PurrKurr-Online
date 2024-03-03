using Code.Wakoz.PurrKurr.Views;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Levels {
    [DefaultExecutionOrder(12)]

    public class LevelsController : SingleController {

        [SerializeField] private MultiStateView _levels;

        private int _currentLevelIndex = -1;

        public void LoadLevel(int levelToLoad) {

            //var levelData = GetLevelData(levelToLoad);

            SetLevel(levelToLoad);
        }

        protected override void Clean() {

        }

        protected override Task Initialize() {

            return Task.CompletedTask;
        }

        private void SetLevel(int levelToLoad) {
            
            if (_levels == null) {
                return;
            }

            _currentLevelIndex = levelToLoad;
            _levels.ChangeState(levelToLoad);
        }

        private async Task GetLevelData(int levelToLoad) {
            
            if (_currentLevelIndex > -1) {
                // Unload previous level
            }
        }

    }
}
