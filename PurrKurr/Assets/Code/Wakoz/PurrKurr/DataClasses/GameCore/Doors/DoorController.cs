using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors {
    [DefaultExecutionOrder(15)]

    public class DoorController : Controller {

        [SerializeField] private bool isDoorEntrance = true;
        [SerializeField] private int roomIndex;

        [SerializeField] private DetectionZone zone;

        private GameplayController _gameplayController;

        public int GetRoomIndex() => roomIndex;

        protected override void Clean() {

            if (zone != null) {
                zone.OnColliderEntered -= handleColliderEntered;
            }

        }

        protected override Task Initialize() {

            Init();

            return Task.CompletedTask;
        }

        private void Init() {
            
            if (zone == null) {
                return;
            }

            zone.OnColliderEntered += handleColliderEntered;
        }

        private void handleColliderEntered(Collider2D triggeredCollider) {

            _gameplayController ??= SingleController.GetController<GameplayController>();

            if (_gameplayController == null) {
                return;
            }

            _gameplayController.OnCharacterNearDoor(this, triggeredCollider);
        }

    }
}