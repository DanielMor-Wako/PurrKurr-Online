using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {

    [System.Serializable]
    public class CharacterBuffStats {

        [Tooltip("Immunities against debuffs")]
        [SerializeField] Definitions.CharacterBuff _buff;
        
        [Tooltip("Damage Reduction to apply in percentage")]
        [SerializeField][Range(0,1)] private float _amountToReduce;
        
        [Tooltip("Duration Reduction to apply in percentage")]
        [SerializeField][Range(0,1)] private float _durationToReduce;

        public Definitions.CharacterBuff Buff => _buff;
        
        public float damageToReduce => _amountToReduce;
        
        public float Amount => _durationToReduce;
    }

}