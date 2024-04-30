using Code.Wakoz.PurrKurr.UI.Instructions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger {
    [Serializable]
    public class OverlayWindowConditionData {
        [Tooltip("Page index to apply the modifiers")]
        [SerializeField] public int PageIndex;
        [Tooltip("Conditional component to check for and then perform an Action")]
        [SerializeField] public Component ConditionRef;
        [Tooltip("Animated Action to display over the UI-Inputs")]
        [SerializeField] public List<OverlayWindowAnimationData> AnimatedAction;
    }
}