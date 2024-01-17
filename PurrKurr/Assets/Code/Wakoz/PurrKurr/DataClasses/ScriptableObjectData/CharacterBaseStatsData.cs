using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {
    
    [System.Serializable]
    public struct CharacterBaseStatsData {

        [SerializeField][Min(1)] private int _maxLevel;
        
        [Tooltip("Hp, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2Int _healthMinToMax;
        
        [Tooltip("Damage, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2Int _damageMinToMax;
        
        [Tooltip("Force to push enemies, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2 _pushbackForceMinToMax;
        
        [Tooltip("Acceleration, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2 _walkSpeedMinToMax;
        
        [Tooltip("Acceleration, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2 _runSpeedMinToMax;
        
        [Tooltip("Speed, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2 _sprintSpeedMinToMax;
        
        [Tooltip("Jump Force, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2 _jumpForceMinToMax;
        
        [Tooltip("Airborne Speed, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2 _airborneSpeedMinToMax;
        
        [Tooltip("Airborne Speed, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2 _airborneMaxSpeedMinToMax;

        [Tooltip("Multi target distance, Min -> Max, from first level to max level")]
        [SerializeField][Min(0)] private Vector2 _multiTargetsDistanceBasevalues;

        [Header("Movement")]
        [Tooltip("What surfaces the animal can move on")]
        [SerializeField] private List<Definitions.MovementType> _movementTypes;
        
        [Header("Actions")]
        [Tooltip("Actions that the character start with")]
        [SerializeField] private List<Definitions.ActionType> _baseActions;
        
        [Header("Abilities")]
        [Tooltip("Abilities that the character start with")]
        [SerializeField] private List<Definitions.CharacterAbility> _baseAbilities;
        
        [Header("Attacks")]
        [Tooltip("Attacks that the character start with")]
        [SerializeField] private List<Definitions.AttackAbility> _baseAttacks;
        
        [Header("Buffs")]
        [Tooltip("Base Buffs that the character always starts with")]
        [SerializeField] private List<CharacterBuffStats> _baseBuffs;

        [Header("Move Set")]
        [Tooltip("The character move set and detail of each action")]
        [SerializeField] private List<AttackBaseStats> _baseAttackMoves;

        [Header("Mastery")]
        [Tooltip("Abilities and Actions that the character is able to perform")]
        [SerializeField] private CharacterAbilitiesData _abilitiesPool;

        public int MaxLevel => _maxLevel;
        
        public Vector2Int HealthBasevalues => _healthMinToMax;
        
        public Vector2Int DamageBasevalues => _damageMinToMax;
        
        public Vector2 PushbackForceBasevalues => _pushbackForceMinToMax;
        
        public Vector2 WalkSpeedBasevalues => _walkSpeedMinToMax;
        
        public Vector2 RunSpeedBasevalues => _runSpeedMinToMax;
        
        public Vector2 SprintSpeedBasevalues => _sprintSpeedMinToMax;
        
        public Vector2 JumpForceBasevalues => _jumpForceMinToMax;
        public Vector2 airborneSpeedBasevalues => _airborneSpeedMinToMax;
        public Vector2 airborneMaxSpeedBasevalues => _airborneMaxSpeedMinToMax;
        public Vector2 multiTargetsDistanceBasevalues => _multiTargetsDistanceBasevalues;

        public List<Definitions.MovementType> movementTypes => _movementTypes;
        
        public List<Definitions.ActionType> BaseActions => _baseActions;
        
        public List<Definitions.ActionType> ActionsPool => _abilitiesPool.ActionsPool;
        
        public List<Definitions.CharacterAbility> BaseAbilities => _baseAbilities;

        public List<Definitions.AttackAbility> BaseAttacks => _baseAttacks;

        public List<Definitions.CharacterAbility> AbilitiesPool => _abilitiesPool.AbilitiesPool;
        
        public List<AttackBaseStats> baseAttackMoves => _baseAttackMoves;
        
        public List<Definitions.AttackAbility> attackPool => _abilitiesPool.AttackPool;
        
        public List<CharacterBuffStats> Buffs => _baseBuffs;
        
    }

}