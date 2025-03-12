using Code.Wakoz.PurrKurr.DataClasses.GameCore.CollectableItems;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems;
using System;
using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.Screens.PersistentGameObjects {
    public class PersistentGameObjectsManager : IDisposable {

        private Dictionary<string, PersistentGameObject> _objects = new();

        public PersistentGameObjectsManager()
        {
            PersistentGameObject.OnPersistentObjectAdded += AddObject;
            PersistentGameObject.OnPersistentObjectRemoved += RemoveObject;
        }

        public void Dispose()
        {
            PersistentGameObject.OnPersistentObjectAdded -= AddObject;
            PersistentGameObject.OnPersistentObjectRemoved -= RemoveObject;
        }

        /// <summary>
        /// Refresh State for all tagged objects
        /// </summary>
        public void RefreshStateAll()
        {
            UnityEngine.Debug.Log($"{_objects.Count} tagged objects are notified");
            foreach (var item in _objects)
            {
                NotifyTaggedObject(item.Key);
            }
        }

        public void NotifyTaggedObject(string itemId)
        {
            PersistentGameObject tagged;
            if (!_objects.TryGetValue(itemId, out tagged)) {
                return;
            }

            tagged.ChangeState();
        }

        public List<PersistentGameObject> GetTaggedObjectsOfType(Type type)
        {
            List<PersistentGameObject> taggedObjects = new();

            foreach (var item in _objects.Values)
            {
                if (item.GetObjectType() != type)
                    continue;

                taggedObjects.Add(item);
            }

            return taggedObjects;
        }

        public List<PersistentGameObject> GetTaggedObjectsOfType<T>() where T : ITaggable
        {
            List<PersistentGameObject> taggedObjects = new();

            foreach (var item in _objects.Values)
            {
                if (item.ObjectTransform.GetComponent<ITaggable>() is not T)
                    continue;

                taggedObjects.Add(item);
            }

            return taggedObjects;
        }

        public PersistentGameObject GetTaggedObject(string itemId)
        {
            PersistentGameObject tagged;
            if (!_objects.TryGetValue(itemId, out tagged))
            {
                UnityEngine.Debug.LogWarning($"Could not find Tagged object {itemId}");
            }

            return tagged;
        }

        private void RemoveObject(PersistentGameObject behaviour) 
        {
            var objType = behaviour.GetObjectType();
            if (objType == null) {
                return;
            }

            //UnityEngine.Debug.Log($"Removed Tagged - {behaviour.ItemId} {behaviour.GetObjectType()}");
            _objects.Remove(behaviour.ItemId);
        }

        private void AddObject(PersistentGameObject behaviour) 
        {
            var objType = behaviour.GetObjectType();
            if (objType == null) {
                return;
            }

            //UnityEngine.Debug.Log($"Added Tagged + {behaviour.ItemId} {behaviour.GetObjectType()}");
            _objects.Add(behaviour.ItemId, behaviour);
        }
    }

}
