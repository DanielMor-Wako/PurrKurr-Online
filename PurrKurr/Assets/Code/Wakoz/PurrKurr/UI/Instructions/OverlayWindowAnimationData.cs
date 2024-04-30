using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.UI.Instructions {
    [Serializable]
    public class OverlayWindowAnimationData {
        [Tooltip("Target transform to use as the position for the cursor")]
        [SerializeField] public Transform CursorTarget;
        [Tooltip("Animates Swipe in the angle stated, value range 0 - 360 to be displayed. 0 = up, 90 = left, 180 = down, 270 = right. Default is -1 as not there is no swipe")]
        [SerializeField][Range(-1, 360)] public int SwipeAngle = -1;
        [Tooltip("Animates the swipe only once and does not loop")]
        [SerializeField] public bool IsHoldSwipe;
    }
}