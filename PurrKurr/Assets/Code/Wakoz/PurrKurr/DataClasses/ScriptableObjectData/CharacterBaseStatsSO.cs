using System.Collections.Generic;
using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [CreateAssetMenu(fileName = "CharacterBaseStats", menuName = "Data/CharacterBaseStats")]
    public class CharacterBaseStatsSO : ScriptableObject {
        
        [Header("Base Stats")]
        
        [Tooltip("Character animal type")]
        [SerializeField] private Definitions.PlayableCharacterType _animal;
        
        [Tooltip("Preferred food type to eat")]
        [SerializeField] private Definitions.FoodType _diet;
                
        [Header("Stats")]
        [SerializeField] private CharacterBaseStatsData _data;

        [Header("Conf")]
        
        [Tooltip("Senses that the character posses")]
        [SerializeField] private List<Definitions.SenseType> _senses;
        [Tooltip("Aggressiveness to the same species")]
        [SerializeField][Range(0,1)] private float _aggressivenessToOthersAnimals;
        [Tooltip("Aggressiveness to other species")]
        [SerializeField][Range(0,1)] private float _aggressivenessToSameAnimals;

        public Definitions.PlayableCharacterType Animal => _animal;
        public Definitions.FoodType Diet => _diet;
        public CharacterBaseStatsData Data => _data;

    }

}