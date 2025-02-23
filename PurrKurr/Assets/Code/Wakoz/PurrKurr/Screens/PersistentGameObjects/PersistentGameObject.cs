using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.PersistentGameObjects {
    [DefaultExecutionOrder(14)]
    public class PersistentGameObject : IDisposable {

        public static event Action<PersistentGameObject> OnPersistentObjectAdded;

        public static event Action<PersistentGameObject> OnPersistentObjectRemoved;

        public event Action OnStateChanged;

        public readonly string ItemId;
        protected Type _type;

        public Type GetObjectType() => _type;

        public PersistentGameObject(Type type, string itemId)
        {
            _type = type;
            ItemId = itemId;

            Init();
        }

        public void ChangeState()
        {
            OnStateChanged?.Invoke();
        }

        public void Dispose()
        {
            OnPersistentObjectRemoved?.Invoke(this);
        }

        private void Init() 
        {
            OnPersistentObjectAdded?.Invoke(this);
        }
    }

}
