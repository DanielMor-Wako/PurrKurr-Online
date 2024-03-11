using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors {

    [DefaultExecutionOrder(15)]
    public class DoorController : DetectionZoneTrigger {

        [SerializeField] private bool isDoorEntrance = true;
        [SerializeField] private int roomIndex;
        [SerializeField] private DoorController NextDoor;

        private GameplayController _gameplayController;

        public int GetRoomIndex() => roomIndex;
        public DoorController GetNextDoor() => NextDoor;

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

            _gameplayController.OnExitDetectionZone(this, triggeredCollider);
        }

        public void handleColliderEntered(Collider2D triggeredCollider) {

            if (_gameplayController == null) {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            _gameplayController.OnEnterDetectionZone(this, triggeredCollider);
        }

    }

}