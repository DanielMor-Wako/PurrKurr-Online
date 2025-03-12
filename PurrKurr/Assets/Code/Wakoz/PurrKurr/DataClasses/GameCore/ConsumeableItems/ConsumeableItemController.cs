using Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems;
using Code.Wakoz.PurrKurr.Views;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.ConsumeableItems
{

    [DefaultExecutionOrder(15)]
    public class ConsumeableItemController : Controller
    {

        [SerializeField] private TaggedItem _collectableItem; // Use ConsumeableTaggedItem? with ConsumeableItemData?

        [Tooltip("Sets the View state to active or deactive when hero has entered the zone")]
        [SerializeField] private MultiStateView _state;

        [SerializeField] private int _quantity = 1;
        private int _currentQuantity = 1;

        public int MaxQuantity => _quantity;

        public (string id, int quantity) GetData()
            => (_collectableItem.GetData().id, _currentQuantity);

        public void SetQuantity(float newQuantitytInPercentage)
        {
            var newAmount = (int)(newQuantitytInPercentage * _quantity * 10);
            UpdateAmount(newAmount);
        }

        public void Consume(int consumeQuantity)
        {
            var newAmount = _currentQuantity - consumeQuantity;
            UpdateAmount(newAmount);
        }

        private void UpdateAmount(int newAmount)
        {
            _currentQuantity = Mathf.Clamp(newAmount, 0, _quantity);

            UpdateStateView();
        }

        protected override void Clean() {}

        protected override Task Initialize()
        {
            _currentQuantity = _quantity; // _collectableItem.GetData().quantity;

            return Task.CompletedTask;
        }

        private void UpdateStateView()
        {

            if (_state == null)
            {
                return;
            }

            var percentage = Mathf.FloorToInt((float)_currentQuantity / _quantity * 10);

            _state.ChangeState(percentage);
        }

    }
}