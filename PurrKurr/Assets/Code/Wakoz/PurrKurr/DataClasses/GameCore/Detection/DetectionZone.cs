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

        private List<Collider2D> _overlappingColliders = new();

        public Collider2D[] GetColliders()
        {
            if (_overlappingColliders == null)
            {
                return null;
            }

            return _overlappingColliders.ToArray();
        }

        private void OnTriggerEnter2D(Collider2D coll)
        {
            if (!_overlappingColliders.Contains(coll))
            {
                _overlappingColliders.Add(coll);
                OnColliderEntered?.Invoke(coll);
            }
        }

        private void OnTriggerExit2D(Collider2D coll)
        {
            if (_overlappingColliders.Contains(coll))
            {
                _overlappingColliders.Remove(coll);
                OnColliderExited?.Invoke(coll);
            }
        }
    }
}