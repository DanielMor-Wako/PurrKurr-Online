using Code.Wakoz.PurrKurr.DataClasses.Enums;
using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.OverlayWindowTrigger {
    public class OverlayWindowConditionalCharacterVelocity : OverlayWindowCondition<NewClassy> {

        /// <summary>
        /// Condition: Min Value to consider as true when a character has entered the zone
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns></returns>

        private NewClassy _velocity;

        public override bool HasMetCondition() {
            return true;
            //_velocity.magnitude > ValueToMatch;
        }

        public override void UpdateCurrentValue() {
            //_velocity = (int)(TargetBody.GetHpPercent() * 100);
        }
    }

    // todo: replace the vector 4 with a special class to config the velocity range
    [Serializable]
    public class NewClassy {
        public Definitions.ObjectState nnnn;
        public bool isLefty;
    }
}