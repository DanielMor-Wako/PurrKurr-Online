using Code.Wakoz.PurrKurr.DataClasses.Enums;
using System;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Ui_Controller
{
    [Serializable]
    public class ActionInput
    {
        public Definitions.ActionType ActionType { get; private set; }
        public Definitions.ActionTypeGroup ActionGroupType { get; private set; }
        public Vector2 NormalizedDirection { get; private set; }
        public float SwipeDistanceTraveledInPercentage { get; private set; }
        public float SwipeVelocity { get; private set; }

        public Vector2 StartPosition { get; private set; }

        private float _startTime;
        private float _minSwipeDistance;
        private float _maxSwipeDistance;
        private float _minSwipeVelocity;

        public ActionInput(Definitions.ActionType actionType, Definitions.ActionTypeGroup actionGroupType, Vector2 startPos, float startTime, Vector2 minMaxSwipeDistance, float minSwipeVelocity = 0) {
            ActionType = actionType;
            ActionGroupType = actionGroupType;
            StartPosition = startPos;
            _startTime = startTime;
            _minSwipeDistance = minMaxSwipeDistance[0];
            _maxSwipeDistance = minMaxSwipeDistance[1];
            _minSwipeVelocity = minSwipeVelocity;
            NormalizedDirection = Vector2.zero;
        }

        public bool UpdateSwipe(Vector2 endPosition, float endTime) {
            var positionDelta = endPosition - StartPosition;
            var distance = positionDelta.magnitude;
            var timeDelta = endTime - _startTime;
            SwipeVelocity = distance != 0 && timeDelta != 0 ? distance / timeDelta : 0;
            //Debug.Log("SwipeVelocity: "+ SwipeVelocity+" << "+ distance+" / "+timeDelta);
            if ((distance > _minSwipeDistance) /*&& (SwipeVelocity > _minSwipeVelocity)*/) {
                SwipeDistanceTraveledInPercentage = Mathf.Clamp01((distance - _minSwipeDistance) / (_maxSwipeDistance - _minSwipeDistance));
                NormalizedDirection = positionDelta.normalized;
                return true;
            }

            SwipeDistanceTraveledInPercentage = 0;
            NormalizedDirection = Vector2.zero;
            return false;
        }
    }
}