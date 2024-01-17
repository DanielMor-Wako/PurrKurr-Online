using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [System.Serializable]
    public struct AttackBaseStats {

        public string _elementName;
        
        [Tooltip("The Attack Ability")]
        [SerializeField] Definitions.AttackAbility _attackAbility;
        
        [Tooltip("The attack properties when activated")]
        [SerializeField]private List<Definitions.AttackProperty> _properties;

        [Tooltip("The character conditional state for perform the action")]
        [SerializeField] private List<Definitions.CharacterState> _characterStateCondition;
        
        [Tooltip("The opponent conditional state for landing a hit")]
        [SerializeField] private List<Definitions.CharacterState> _opponentStateCondition;


        // todo: maybe add the projectile attack to the attack list?
        public Definitions.AttackAbility Ability => _attackAbility;
        
        public List<Definitions.AttackProperty> Properties => _properties;
        
        public List<Definitions.CharacterState> CharacterStateConditions => _characterStateCondition;
        
        public List<Definitions.CharacterState> OpponentStateConditions => _opponentStateCondition;

    }

}