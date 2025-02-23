using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Views;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems
{

    [DefaultExecutionOrder(15)]
    //[TypeMarkerMultiClass(typeof(CollectObjective))]
    public class CollectableItemController : DetectionZoneTrigger {

        [SerializeField] private CollectableItem _collectableItem;

        [Tooltip("Sets the View state to active or deactive when hero has entered the zone")]
        [SerializeField] private MultiStateView _state;

        private GameplayController _gameplayController;
        private bool _isCollected = false;

        public (string id, int quantity) GetData() 
            => _collectableItem.GetData();

        protected override void Clean() {
            base.Clean();

            OnColliderEntered -= handleColliderEntered;
            OnColliderExited -= handleColliderExited;

            if (_collectableItem != null)
            {
                _collectableItem.OnStateChanged -= HandleStateChange;
            }
        }

        protected override Task Initialize() {
            base.Initialize();

            _gameplayController ??= SingleController.GetController<GameplayController>();

            OnColliderEntered += handleColliderEntered;
            OnColliderExited += handleColliderExited;

            if (_collectableItem != null)
            {
                _collectableItem.OnStateChanged += HandleStateChange;
            }

            return Task.CompletedTask;
        }

        private void HandleStateChange(bool isCollected)
        {
            Debug.Log($"State changed -> {isCollected} for {gameObject.name}");

            if (_state == null)
            {
                return;
            }
            _state.ChangeState(Convert.ToInt32(isCollected));
        }

        public void handleColliderExited(Collider2D triggeredCollider) {

            if (_gameplayController == null) {
                Debug.LogError("_gameplayController is missing");
                return;
            }
            
            if (_isCollected) {
                return;
            }

            _gameplayController.OnExitDetectionZone(this, triggeredCollider);
        }

        public void handleColliderEntered(Collider2D triggeredCollider) {

            if (_gameplayController == null) {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            if (_isCollected) {
                return;
            }

            if (_gameplayController.OnEnterDetectionZone(this, triggeredCollider)) {

                _isCollected = true;
                UpdateStateView();
            }
        }

        private void UpdateStateView() {

            if (_state == null) {
                return;
            }

            _state.ChangeState(_isCollected ? 1 : 0);
        }

    }
}