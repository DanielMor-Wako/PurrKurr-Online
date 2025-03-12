using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.Objectives;
using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller;
using Code.Wakoz.PurrKurr.Screens.PersistentGameObjects;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems
{

    [DefaultExecutionOrder(15)]
    public class CollectableTaggedItem : Controller, ITaggable
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
            _taggedObject = new PersistentGameObject(typeof(CollectableTaggedItem), _collectableData.ItemId, transform);
            _taggedObject.OnStateChanged += NotifyStateChanged;
        }

        private void OnDisable()
        {
            if (_taggedObject == null)
                return;

            _taggedObject.OnStateChanged -= NotifyStateChanged;
            _taggedObject.Dispose();
        }

        protected override Task Initialize()
        {
            return Task.CompletedTask;
        }

        protected override void Clean() {}

        private void NotifyStateChanged()
        {
            var _isCollectable = SingleController.GetController<GameplayController>().Handlers.GetHandler<ObjectivesHandler>().IsCollected(_collectableData.ItemId);
            
            OnStateChanged?.Invoke(_isCollectable);
        }
    }
}