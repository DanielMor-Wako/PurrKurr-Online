using Code.Wakoz.PurrKurr.DataClasses.GameCore.ConditionChecker;
using Code.Wakoz.PurrKurr.UI.Instructions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger {
    [Serializable]
    public class OverlayWindowConditionData {
        [Tooltip("Page index to apply the modifiers")]
        public int PageIndex;
        [Tooltip("Conditional component to check specific about a character for and then perform an Action when conditions are met")]
        public Component CharacterConditionChecker;
        [Tooltip("Apply Slow Motion during")]
        public bool ApplySlowMotion = false;
        [Tooltip("Animated Action to display over the UI-Inputs")]
        public List<OverlayWindowAnimationData> AnimatedAction;
    }
}