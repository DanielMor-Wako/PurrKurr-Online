using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.PersistentGameObjects {
    [DefaultExecutionOrder(14)]
    public class PersistentGameObject : MonoBehaviour {

        public static event Action<PersistentGameObject> OnPersistentObjectAdded;

        public static event Action<PersistentGameObject> OnPersistentObjectRemoved;

        protected Type _data;

        public Type GetObjectType() => _data;

        public void Init(Type type) {

            _data = type;

            OnPersistentObjectAdded?.Invoke(this);
        }

        private void OnEnable() => OnPersistentObjectAdded?.Invoke(this);

        private void OnDisable() => OnPersistentObjectRemoved?.Invoke(this);
    }

}
