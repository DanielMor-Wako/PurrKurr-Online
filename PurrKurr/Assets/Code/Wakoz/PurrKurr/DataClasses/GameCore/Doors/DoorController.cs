using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.Utils.Attributes;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Doors
{

    [DefaultExecutionOrder(15)]
    //[TypeMarkerMultiClass(typeof(ReachTargetZoneObjective))]
    public class DoorController : DetectionZoneTrigger
    {
        [SerializeField] private CollectableTaggedItem _collectableItem;

        [SerializeField] private bool _isDoorEntrance = true;
        [SerializeField] private int _roomIndex;
        [SerializeField] private DoorController _nextDoor;

        private GameplayController _gameplayController;

        public int GetRoomIndex() => _roomIndex;
        public DoorController GetNextDoor() => _nextDoor;
        public bool IsDoorEntrance() => _isDoorEntrance;
        public CollectableItemData CollectableData => _collectableItem != null ? _collectableItem.CollectableData : null;

        protected override void Clean()
        {
            base.Clean();

            OnColliderEntered -= HandleColliderEntered;
            OnColliderExited -= HandleColliderExited;

            if (_collectableItem != null)
            {
                _collectableItem.OnStateChanged -= HandleStateChange;
            }
        }

        protected override Task Initialize()
        {
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
            Debug.Log($"State changed -> {isCollected} for {gameObject.name}");

            /*if (_state == null)
            {
                return;
            }
            _state.ChangeState(Convert.ToInt32(isCollected));*/
        }

        public void HandleColliderExited(Collider2D triggeredCollider)
        {
            if (_gameplayController == null)
            {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            _gameplayController.OnExitDetectionZone(this, triggeredCollider);
        }

        public void HandleColliderEntered(Collider2D triggeredCollider)
        {
            if (_gameplayController == null)
            {
                Debug.LogError("_gameplayController is missing");
                return;
            }

            _gameplayController.OnEnterDetectionZone(this, triggeredCollider);
        }

    }
}