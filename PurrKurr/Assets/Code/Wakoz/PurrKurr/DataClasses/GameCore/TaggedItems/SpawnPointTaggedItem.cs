using Code.Wakoz.PurrKurr.Screens.PersistentGameObjects;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems
{
    public class SpawnPointTaggedItem : Controller, ITaggable
    {
        [SerializeField] private string _itemId = "";
        private PersistentGameObject _taggedObject;

        private void OnEnable()
        {
            _taggedObject = new PersistentGameObject(typeof(SpawnPointTaggedItem), _itemId, transform);
        }

        private void OnDisable()
        {
            if (_taggedObject == null)
                return;

            _taggedObject.Dispose();
        }

        protected override Task Initialize()
        {
            return Task.CompletedTask;
        }

        protected override void Clean() { }

    }
}