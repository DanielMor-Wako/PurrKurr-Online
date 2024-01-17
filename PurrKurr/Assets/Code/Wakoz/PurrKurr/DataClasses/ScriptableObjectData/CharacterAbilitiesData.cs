using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [System.Serializable]
    public struct CharacterAbilitiesData {

        [Tooltip("Actions of the character")]
        [SerializeField] private List<Definitions.ActionType> _actionsPool;
        
        [Tooltip("Abilities of the character")]
        [SerializeField] private List<Definitions.CharacterAbility> _abilitiesPool;
        
        [Tooltip("Attacks of that the character")]
        [SerializeField] private List<Definitions.AttackAbility> _attackPool;
        
        [Tooltip("Buffs of the character")]
        [SerializeField] private List<CharacterBuffStats> _buffsPool;

        public List<Definitions.ActionType> ActionsPool => _actionsPool;
        
        public List<Definitions.CharacterAbility> AbilitiesPool => _abilitiesPool;
        
        public List<Definitions.AttackAbility> AttackPool => _attackPool;
        
        public List<CharacterBuffStats> BuffsPool => _buffsPool;
        
    }

}