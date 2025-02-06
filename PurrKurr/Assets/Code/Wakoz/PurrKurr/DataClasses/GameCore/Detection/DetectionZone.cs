using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection
{

    [RequireComponent(typeof(Collider2D))]
    public class DetectionZone : MonoBehaviour
    {
        public event Action<Collider2D> OnColliderEntered;
        public event Action<Collider2D> OnColliderExited;

        private readonly HashSet<Collider2D> _colliders = new();

        private Collider2D[] _collArray;
        private bool _isDirty = true;

        public Collider2D[] GetColliders()
        {
            if (!_isDirty)
            {
                return _collArray;
            }

            _isDirty = false;

            _collArray = new Collider2D[_colliders.Count];
            _colliders.CopyTo(_collArray);

            return _collArray;
        }

        private void OnTriggerEnter2D(Collider2D coll)
        {
            if (_colliders.Add(coll))
            { //Debug.Log(this.name);
                _isDirty = true;
                OnColliderEntered?.Invoke(coll);
            }
        }

        private void OnTriggerExit2D(Collider2D coll)
        {
            if (_colliders.Remove(coll))
            {
                _isDirty = true;
                OnColliderExited?.Invoke(coll);
            }
        }
    }
}