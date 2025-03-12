using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems;
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

        [SerializeField] private CollectableTaggedItem _collectableItem;

        [Tooltip("Sets the View state to active or deactive when hero has entered the zone")]
        [SerializeField] private MultiStateView _state;

        private GameplayController _gameplayController;
        private bool _isCollected = false;

        public (string id, int quantity) GetData() 
            => _collectableItem.GetData();

        protected override void Clean() {
            base.Clean();

            OnColliderEntered -= HandleColliderEntered;
            OnColliderExited -= HandleColliderExited;

            if (_collectableItem != null)
            {
                _collectableItem.OnStateChanged -= HandleStateChange;
            }
        }

        protected override Task Initialize() {
            base.Initialize();

            _gameplayController ??= SingleController.GetController<GameplayController>();

            OnColliderEntered += HandleColliderEntered;
            OnColliderExited += HandleColliderExited;

            if (_collectableItem != null)
            {
                _collectableItem.OnStateChanged += HandleStateChange;
            }

            return Task.CompletedTask;
        }

        private void HandleStateChange(bool isCollected)
        {
            _isCollected = isCollected;

            Debug.Log($"{gameObject.name} State -> {isCollected}");
            UpdateStateView();
        }

        public void HandleColliderExited(Collider2D triggeredCollider) {

            if (_gameplayController == null) {
                Debug.LogError("_gameplayController is missing");
                return;
            }
            
            if (_isCollected) {
                return;
            }

            _gameplayController.OnExitDetectionZone(this, triggeredCollider);
        }

        public void HandleColliderEntered(Collider2D triggeredCollider) {

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

            _state.ChangeState(Convert.ToInt32(_isCollected));
        }

    }
}