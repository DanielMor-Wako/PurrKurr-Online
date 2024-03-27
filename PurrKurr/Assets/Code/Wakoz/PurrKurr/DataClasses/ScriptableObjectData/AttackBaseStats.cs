using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [System.Serializable]
    public struct AttackBaseStats {

        public string _elementName;
        
        [Tooltip("The Attack Ability")]
        [SerializeField] private Definitions.AttackAbility _attackAbility;

        [Tooltip("The duration of the delay before the action start, 0.15f is the fastest duration that a character can move so its the min action duration")]
        [SerializeField][Min(0.15f)] private float _actionDuration;

        [Tooltip("The duration of the cooldown after the action")]
        [SerializeField][Min(0)] private float _actionCooldownDuration;

        [Tooltip("The attack properties when activated")]
        [SerializeField] private List<Definitions.AttackProperty> _properties;

        [Tooltip("The character conditional state for perform the action")]
        [SerializeField] private List<Definitions.ObjectState> _characterStateCondition;
        
        [Tooltip("The opponent conditional state for landing a hit")]
        [SerializeField] private List<Definitions.ObjectState> _opponentStateCondition;


        // todo: maybe add the projectile attack to the attack list?
        public Definitions.AttackAbility Ability => _attackAbility;

        public float ActionDuration => _actionDuration;
               
        public float ActionCooldownDuration => _actionCooldownDuration;

        public List<Definitions.AttackProperty> Properties => _properties;
        
        public List<Definitions.ObjectState> CharacterStateConditions => _characterStateCondition;
        
        public List<Definitions.ObjectState> OpponentStateConditions => _opponentStateCondition;

    }

}