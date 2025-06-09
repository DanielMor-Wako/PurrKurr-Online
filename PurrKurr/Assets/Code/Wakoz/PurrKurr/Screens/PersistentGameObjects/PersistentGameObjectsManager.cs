using Code.Wakoz.PurrKurr.DataClasses.GameCore.TaggedItems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Code.Wakoz.PurrKurr.Screens.PersistentGameObjects
{
    public class PersistentGameObjectsManager : IDisposable
    {

        private Dictionary<string, PersistentGameObject> _objects = new();
        private Dictionary<string, List<PersistentGameObject>> _dependentObjects = new();

        public PersistentGameObjectsManager() {
            PersistentGameObject.OnPersistentObjectAdded += AddObject;
            PersistentGameObject.OnPersistentObjectRemoved += RemoveObject;
        }

        public void Dispose() {
            PersistentGameObject.OnPersistentObjectAdded -= AddObject;
            PersistentGameObject.OnPersistentObjectRemoved -= RemoveObject;
        }

        /// <summary>
        /// Refresh State for all tagged objects and dependent objects
        /// </summary>
        public void NotifyAll() {

            // todo: remove this hell of logs
            var stringBuilder = new StringBuilder($"{_objects.Count} Tagged objects Info:\n");
            foreach (var item in _objects) {
                item.Value.ChangeState();
                stringBuilder.Append($"{item.Value.ItemId} | ");
            }
            UnityEngine.Debug.Log(stringBuilder);

            stringBuilder = new StringBuilder($"{_dependentObjects.Count} Tagged dependent objects Info:\n");
            foreach (var dependentItems in _dependentObjects) {
                NotifyTaggedDependedObjects(dependentItems.Key);
                foreach (var dependentItem in dependentItems.Value) {
                    stringBuilder.Append($"{dependentItem.ItemId} | ");
                }
            }
            UnityEngine.Debug.Log(stringBuilder);
        }

        public void NotifyTaggedObject(string itemId) {

            if (!_objects.TryGetValue(itemId, out var tagged)) {
                return;
            }

            tagged.ChangeState();
        }

        /// <summary>
        /// Notify all dependent objects by the uniqueId
        /// </summary>
        /// <param name="uniqueId"></param>
        public void NotifyTaggedDependedObjects(string uniqueId) {

            if (!_dependentObjects.TryGetValue(uniqueId, out var taggedDependent)) {
                return;
            }

            foreach (var dependentItem in taggedDependent) {
                dependentItem.ChangeState();
            }
        }

        public List<PersistentGameObject> GetTaggedObjectsOfType(Type type) {

            List<PersistentGameObject> taggedObjects = new();

            foreach (var item in _objects.Values) {
                if (item.GetObjectType() != type)
                    continue;

                taggedObjects.Add(item);
            }

            return taggedObjects;
        }

        public List<PersistentGameObject> GetTaggedObjectsOfType<T>() where T : ITaggable {

            List<PersistentGameObject> taggedObjects = new();

            foreach (var item in _objects.Values) {
                if (item.ObjectTransform.GetComponent<ITaggable>() is not T)
                    continue;

                taggedObjects.Add(item);
            }

            return taggedObjects;
        }

        public PersistentGameObject GetTaggedObject(string itemId) {

            PersistentGameObject tagged;
            if (!_objects.TryGetValue(itemId, out tagged)) {
                UnityEngine.Debug.LogWarning($"Could not find Tagged object {itemId}");
            }

            return tagged;
        }

        private void RemoveObject(PersistentGameObject taggedObject) {

            var objType = taggedObject.GetObjectType();
            if (objType == null) {
                return;
            }

            if (objType == typeof(DependentTaggedItem)) {
                RemoveDependentObject(taggedObject);
                return;
            }

            //UnityEngine.Debug.Log($"Removed Tagged - {taggedObject.ItemId} {taggedObject.GetObjectType()}");
            _objects.Remove(taggedObject.ItemId);
        }

        private void AddObject(PersistentGameObject taggedObject) {

            var objType = taggedObject.GetObjectType();
            if (objType == null) {
                return;
            }

            if (objType == typeof(DependentTaggedItem)) {
                AddDependentObject(taggedObject);
                return;
            }

            //UnityEngine.Debug.Log($"Added Tagged + {taggedObject.ItemId} {taggedObject.GetObjectType()}");
            _objects.Add(taggedObject.ItemId, taggedObject);
        }


        private void RemoveDependentObject(PersistentGameObject dependentObject) {

            if (!_dependentObjects.ContainsKey(dependentObject.ItemId)) {
                return;
            }

            //UnityEngine.Debug.Log($"Removed TaggedDependent - {dependentObject.ItemId} {dependentObject.GetObjectType()}");
            _dependentObjects[dependentObject.ItemId].Remove(dependentObject);

            if (_dependentObjects[dependentObject.ItemId].Count > 0) {
                return;
            }

            _dependentObjects.Remove(dependentObject.ItemId);
        }

        private void AddDependentObject(PersistentGameObject dependentObject) {

            if (!_dependentObjects.ContainsKey(dependentObject.ItemId)) {
                _dependentObjects[dependentObject.ItemId] = new List<PersistentGameObject>();
            }

            //UnityEngine.Debug.Log($"Added TaggedDependent + {dependentObject.ItemId} {dependentObject.GetObjectType()}");
            _dependentObjects[dependentObject.ItemId].Add(dependentObject);
        }
    }

}
