using System;
using System.Collections.Generic;
using System.Linq;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {

    [Serializable]
    public class CharacterStats {

        [SerializeField] private CharacterBaseStatsSO _baseStats;
        [SerializeField] private CharacterAbilitiesData _unlockedAbilities;
        
        [Header("Actions")]
        [Tooltip("Actions that the character have (unlocked + base)")]
        [SerializeField] private List<Definitions.ActionType> _actions;
        [Header("Abilities")]
        [Tooltip("Abilities that the character have (unlocked + base)")]
        [SerializeField] private List<Definitions.CharacterAbility> _abilities;
        [Header("Attacks")]
        [Tooltip("Attacks that the character have (unlocked + base)")]
        [SerializeField] private List<Definitions.AttackAbility> _attacks;
        [Header("Buffs")]
        [Tooltip("Base Buffs that the character have (unlocked + base)")]
        [SerializeField] private List<CharacterBuffStats> _buffs;
        
        [Header("Stats")]
        public int Health = 100;
        public int MaxHealth = 100;
        public int Damage = 2000;
        public float PushbackForce = 5;
        public float WalkSpeed = 300;
        public float RunSpeed = 700;
        public float SprintSpeed = 2000;
        public float JumpForce = 1500;
        public float AirborneSpeed = 10;
        public float AirborneMaxSpeed = 20;
        public float MultiTargetsDistance = 5f;
        public float BodySize = 1;
        
        public int AttackDurationInMilliseconds = 150;
        
        private float _levelInterval;
        
        public CharacterStats(CharacterAbilitiesData unlockedAbilities) {
            
            _levelInterval = 1f / _baseStats.Data.MaxLevel;

            _unlockedAbilities = unlockedAbilities;
            InitUpgrades();
        }

        public List<Definitions.CharacterAbility> GetAbilities() => _abilities;
        
        public List<Definitions.AttackAbility> GetAttacks() => _attacks;
        
        public List<Definitions.ActionType> GetActions() => _actions;

        public void InitUpgrades() {
            
            var unlockedActions = _unlockedAbilities.ActionsPool;
            var unlockedAbilities = _unlockedAbilities.AbilitiesPool;
            var unlockedAttacks = _unlockedAbilities.AttackPool;
            var unlockedBuffs = _unlockedAbilities.BuffsPool;
            
            _actions = new List<Definitions.ActionType>(_baseStats.Data.BaseActions);
            InitUpgrade(ref _actions, unlockedActions);

            _abilities = new List<Definitions.CharacterAbility>(_baseStats.Data.BaseAbilities);
            InitUpgrade(ref _abilities, unlockedAbilities);

            _attacks = new List<Definitions.AttackAbility>(_baseStats.Data.BaseAttacks);
            InitUpgrade(ref _attacks, unlockedAttacks);

            _buffs = new List<CharacterBuffStats>(_baseStats.Data.Buffs);
            InitUpgrade(ref _buffs, unlockedBuffs);
        }

        public void InitUpgrade<T>(ref List<T> baseList, List<T> upgrades) {

            if (upgrades == null) {
                return;
            }

            var newList = new List<T>(baseList);
            newList.AddRange(upgrades);
            var uniqueSet = new HashSet<T>(newList);
            var uniqueList = new List<T>(uniqueSet);
            baseList = uniqueList;
        }

        public int MaxLevel => _baseStats.Data.MaxLevel;

        public float GetLevelProgressAsPercent(int level) {
            
            if (_levelInterval == 0) {
                _levelInterval = 1f / _baseStats.Data.MaxLevel;
            }
            
            return level * _levelInterval; 
        }
        
        public void UpdateStats(int level) {
            
            var percentage = GetLevelProgressAsPercent(level);
            
            Health =
                Mathf.RoundToInt(CalculateStateWithinMinMax(percentage, _baseStats.Data.HealthBasevalues));

            MaxHealth = Health;

            Damage = 
                Mathf.RoundToInt(CalculateStateWithinMinMax(percentage, _baseStats.Data.DamageBasevalues));
            
            PushbackForce = 
                (CalculateStateWithinMinMax(percentage, _baseStats.Data.PushbackForceBasevalues));
            
            WalkSpeed =
                (CalculateStateWithinMinMax(percentage, _baseStats.Data.WalkSpeedBasevalues));
            
            RunSpeed =
                (CalculateStateWithinMinMax(percentage, _baseStats.Data.RunSpeedBasevalues));
            
            SprintSpeed =
                (CalculateStateWithinMinMax(percentage, _baseStats.Data.SprintSpeedBasevalues));

            JumpForce =
                (CalculateStateWithinMinMax(percentage, _baseStats.Data.JumpForceBasevalues));
            
            AirborneSpeed =
                (CalculateStateWithinMinMax(percentage, _baseStats.Data.airborneSpeedBasevalues));

            AirborneMaxSpeed =
                (CalculateStateWithinMinMax(percentage, _baseStats.Data.airborneMaxSpeedBasevalues));

            MultiTargetsDistance =
                (CalculateStateWithinMinMax(percentage, _baseStats.Data.multiTargetsDistanceBasevalues));

            // todo: add the body size to the characterBaseStatsData
            var minBodySize = 0.7f;
            BodySize = (1 - minBodySize) * percentage + minBodySize;
        }

        private float CalculateStateWithinMinMax(float percentage, Vector2 minMaxValue) {
            var minValue = minMaxValue[0];
            var maxValue = minMaxValue[1];
            return (minValue + (percentage * (maxValue-minValue)));
        }

        public bool TryGetAttack(Definitions.AttackAbility attackAbility, out AttackBaseStats attackStats) {

            attackStats = new AttackBaseStats();

            if (!IsAttackAvailable(attackAbility)) { 
                return false;
            }
            
            GetAttackProperties(attackAbility, ref attackStats);
            Debug.Log("returned attackStats for " + attackAbility);
            return true;
        }

        public bool GetAttackProperties(Definitions.AttackAbility attackAbility, ref AttackBaseStats attackProperties) {

            foreach (var attackMove in _baseStats.Data.baseAttackMoves.Where(attackMove 
                         => attackMove.Ability == attackAbility)) {
                
                attackProperties = attackMove;
                return true;
            }

            return false;
        }

        public bool IsAttackAvailable(Definitions.AttackAbility attackAbility) 
            => _attacks.Any(attackMove => attackMove == attackAbility);

        public float GetHealthPercentage() => (float)Health / MaxHealth;
    }

}