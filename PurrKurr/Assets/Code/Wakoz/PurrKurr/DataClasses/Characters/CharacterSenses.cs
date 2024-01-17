using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {
    
    [RequireComponent(typeof(Collider2D))]
    public class CharacterSenses : MonoBehaviour {

        [SerializeField] private CircleCollider2D _senses;

        private List<Collider2D> _nearbyCharacters = new();

        private LayerMask _whatIsDamageableCharacter;
        private LayerMask _whatIsDamageable;
        
        public float Radius => _senses.radius;

        public void Init(LayerMask whatIsDamageableCharacter, LayerMask whatIsDamageable) {
            
            _whatIsDamageableCharacter = whatIsDamageableCharacter;
            _whatIsDamageable = whatIsDamageable;
        }

        public Collider2D[] NearbyCharacters() {

            if (_nearbyCharacters == null) {
                return null;
            }

            return _nearbyCharacters.ToArray();
        }

        private void OnTriggerEnter2D(Collider2D coll) => _nearbyCharacters.Add(coll);

        private void OnTriggerExit2D(Collider2D coll) => _nearbyCharacters.Remove(coll);

    }

}