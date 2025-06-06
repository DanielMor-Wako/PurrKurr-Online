﻿using System;
using System.Collections.Generic;
using System.Linq;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Characters {

    [Serializable]
    public class CharacterStats {

        [SerializeField] private CharacterBaseStatsSO _baseStats;
        [SerializeField] private CharacterAbilitiesData _unlockedAbilities;

        [Header("Level")]
        [Tooltip("The current level in percentage, affects stats and body size")]
        [SerializeField][Range(0, 1)] float _levelPercent = 0;
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
        [SerializeField] private float _currentLevel = 0;
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
        
        public float MoveAnimDurationInMilliseconds = 0.15f;
        
        private float _levelInterval;
        private float _healthPercentage;

        public CharacterStats(CharacterAbilitiesData unlockedAbilities) {
            
            _levelInterval = 1f / _baseStats.Data.MaxLevel;

            _unlockedAbilities = unlockedAbilities;
            InitUpgrades();
        }

        public List<Definitions.CharacterAbility> GetAbilities() => _abilities;
        
        public List<Definitions.AttackAbility> GetAttacks() => _attacks;
        
        public List<Definitions.ActionType> GetActions() => _actions;

        public bool HasAbility(Definitions.ActionType ability) => _actions.Contains(ability);

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

        public int GetCurrentLevel() => Mathf.FloorToInt(_levelPercent * MaxLevel);

        public int MaxLevel => _baseStats.Data.MaxLevel;

        public float GetLevelProgressAsPercent(int level) {
            
            if (_levelInterval == 0) {
                _levelInterval = 1f / _baseStats.Data.MaxLevel;
            }
            
            return level * _levelInterval; 
        }
        
        public void UpdateStats(int level) {

            _currentLevel = level;

            _healthPercentage = 1;

            _levelPercent = GetLevelProgressAsPercent(level);
            
            Health =
                Mathf.RoundToInt(CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.HealthBasevalues));

            MaxHealth = Health;

            Damage = 
                Mathf.RoundToInt(CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.DamageBasevalues));
            
            PushbackForce = 
                (CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.PushbackForceBasevalues));
            
            WalkSpeed =
                (CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.WalkSpeedBasevalues));
            
            RunSpeed =
                (CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.RunSpeedBasevalues));
            
            SprintSpeed =
                (CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.SprintSpeedBasevalues));

            JumpForce =
                (CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.JumpForceBasevalues));
            
            AirborneSpeed =
                (CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.airborneSpeedBasevalues));

            AirborneMaxSpeed =
                (CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.airborneMaxSpeedBasevalues));

            MultiTargetsDistance =
                (CalculateWithinMinMaxRange(_levelPercent, _baseStats.Data.multiTargetsDistanceBasevalues));

            // todo: add the body size to the characterBaseStatsData
            var minBodySize = 0.8f;
            BodySize = (1 - minBodySize) * _levelPercent + minBodySize;
        }

        private float CalculateWithinMinMaxRange(float percentage, Vector2 minMaxValue) {

            var minValue = minMaxValue[0];
            var maxValue = minMaxValue[1];
            return (minValue + (percentage * (maxValue-minValue)));
        }

        public bool TryGetAttack(ref Definitions.AttackAbility attackAbility, out AttackBaseStats attackStats) {

            attackStats = new AttackBaseStats();

            if (!IsAttackAvailable(attackAbility)) { 
                return false;
            }
            
            GetAttackProperties(attackAbility, ref attackStats);
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

        public float GetHealthPercentage() => _healthPercentage;

        public void UpdateHealth(int health) {

            Health = health;
            _healthPercentage = (float)Health / MaxHealth;
        }

        public bool TryUnlockAbility(Definitions.ActionType ability) {

            if (_actions.Contains(ability) ||
                _unlockedAbilities.ActionsPool.Contains(ability) ||
                !_baseStats.Data.ActionsPool.Contains(ability)) {
                return false;
            }

            _unlockedAbilities.ActionsPool.Add(ability);
            InitUpgrades();

            return true;
        }

        public void SetUnlockedAbilities(List<Definitions.ActionType> characterAbilities) {

            _unlockedAbilities.ActionsPool.Clear();
            if (characterAbilities != null) 
                _unlockedAbilities.ActionsPool.AddRange(characterAbilities);
            
            InitUpgrades();
        }
    }

}