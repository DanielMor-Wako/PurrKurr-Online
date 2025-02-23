using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.PersistentGameObjects;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems
{

    [DefaultExecutionOrder(15)]
    public class CollectableItem : Controller
    {
        public event Action<bool> OnStateChanged;

        [SerializeField] private CollectableItemData _collectableData;
        private PersistentGameObject _taggedObject;

        private bool _isCollectable;

        public bool IsCollected() => !_isCollectable;

        public CollectableItemData CollectableData => _collectableData;

        public (string id, int quantity) GetData() 
            => (_collectableData.ItemId, _collectableData.Quantity);

        private void OnEnable()
        {
            // register the item in the persistentGameObject
            _taggedObject = new PersistentGameObject(typeof(CollectableItem), _collectableData.ItemId);
            _taggedObject.OnStateChanged += NotifyStateChanged;
        }

        private void OnDisable()
        {
            // deregister the item off the persistentGameObject
            if (_taggedObject == null)
            {
                return;
            }

            _taggedObject.OnStateChanged -= NotifyStateChanged;
            _taggedObject.Dispose();
        }

        protected override Task Initialize()
        {

            return Task.CompletedTask;
        }

        protected override void Clean()
        {
        }

        private void NotifyStateChanged()
        {
            // request update by latest state for this ItemId
            var _isCollectable = SingleController.GetController<GameplayController>().Handlers.GetHandler<ObjectivesHandler>().IsCollected(_collectableData.ItemId);
            if (_isCollectable)
            {
                Debug.Log($"{_collectableData.ItemId} marked as collected");
            }
            OnStateChanged?.Invoke(_isCollectable);
        }
    }
}