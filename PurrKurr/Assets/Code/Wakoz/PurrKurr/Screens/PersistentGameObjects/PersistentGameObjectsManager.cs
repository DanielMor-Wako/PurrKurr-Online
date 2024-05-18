using System;
using System.Collections.Generic;

namespace Code.Wakoz.PurrKurr.Screens.PersistentGameObjects {
    public class PersistentGameObjectsManager : IDisposable {

        private Dictionary<int, PersistentGameObject> _objects = new();

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

        private void RemoveObject(PersistentGameObject behaviour) {
            var objType = behaviour.GetObjectType();
            if (objType == null) {
                return;
            }

            // ($"Removed - {behaviour.name} {behaviour.GetInstanceID()}");
            _objects.Remove(behaviour.GetInstanceID());
        }

        private void AddObject(PersistentGameObject behaviour) {
            var objType = behaviour.GetObjectType();
            if (objType == null) {
                return;
            }

            // ($"Added + {behaviour.name} {behaviour.GetObjectType()}");
            _objects.Add(behaviour.GetInstanceID(), behaviour);
        }
    }

}
