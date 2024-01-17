using Code.Wakoz.PurrKurr.DataClasses.Enums;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData {
    [System.Serializable]
    public struct ActionPadData {

        public string name;
        
        [Tooltip("the action type assigned to the pad - Navigation , Ability etc")]
        [SerializeField] private Definitions.ActionTypeGroup _actionTypeGroup;
        
        [Tooltip("the action type assigned to the pad - Movement , Jump , Attack , Block etc")]
        [SerializeField] private Definitions.ActionType _actionType;
        
        [Tooltip("fixed pad = not moving, flexible pad = has alternative state with joystick")]
        [SerializeField] private Definitions.PadType _padType;
        
        [Tooltip("swipe type required to perform a swipe , based on distance")]
        [SerializeField] private Definitions.SwipeDistanceType _swipeDistanceType;

        public Definitions.ActionTypeGroup ActionTypeGroup => _actionTypeGroup;
        
        public Definitions.ActionType ActionType => _actionType;
        
        public Definitions.PadType PadType => _padType;
        
        public Definitions.SwipeDistanceType SwipeDistanceType => _swipeDistanceType;
        
    }

}