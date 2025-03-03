using Code.Wakoz.PurrKurr.Screens.PersistentGameObjects;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems
{
    public class TaggedItem : Controller
    {
        [SerializeField] private string _itemId = "";
        private PersistentGameObject _taggedObject;

        public (string id, int quantity) GetData()
            => (_itemId, 1);

        private void OnEnable()
        {
            // register the item in the persistentGameObject
            _taggedObject = new PersistentGameObject(typeof(TaggedItem), _itemId, transform);
            //_taggedObject.OnStateChanged += NotifyStateChanged;
        }

        private void OnDisable()
        {
            // deregister the item off the persistentGameObject
            if (_taggedObject == null)
            {
                return;
            }

            //_taggedObject.OnStateChanged -= NotifyStateChanged;
            _taggedObject.Dispose();
        }

        protected override Task Initialize()
        {
            return Task.CompletedTask;
        }

        protected override void Clean()
        {
        }

    }
}