using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger {

    [DefaultExecutionOrder(15)]
    public class OverlayWindowTrigger : DetectionZoneTrigger {

        [Tooltip("Sets the max activation for the window. 0 counts as infinite activations")]
        [SerializeField][Min(0)] private int _activationLimit = 0;

        private GameplayController _gameplayController;
        private int _activationsCount = 0;

        protected override void Clean() {
            base.Clean();

            OnColliderEntered -= handleColliderEntered;
            OnColliderExited -= handleColliderExited;
        }

        protected override Task Initialize() {
            base.Initialize();

            _gameplayController ??= SingleController.GetController<GameplayController>();

            OnColliderEntered += handleColliderEntered;
            OnColliderExited += handleColliderExited;

            return Task.CompletedTask;
        }

        public void handleColliderExited(Collider2D triggeredCollider) {

            if (_gameplayController == null) {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            if (HasReachedMaxCount()) {
                return;
            }

            _gameplayController.OnCharacterExitDoor(this, triggeredCollider);
        }

        public void handleColliderEntered(Collider2D triggeredCollider) {

            if (_gameplayController == null) {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            if (HasReachedMaxCount()) {
                return;
            }

            if (_gameplayController.OnCharacterNearDoor(this, triggeredCollider)) {
                _activationsCount++;
            }
        }

        private bool HasReachedMaxCount() => _activationLimit > 0 && _activationsCount > _activationLimit;

    }

}