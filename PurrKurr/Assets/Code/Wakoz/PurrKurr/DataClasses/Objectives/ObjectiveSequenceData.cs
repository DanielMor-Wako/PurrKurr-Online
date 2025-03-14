using Code.Wakoz.PurrKurr.DataClasses.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.Objectives
{
    [System.Serializable]
    public class ObjectiveSequenceData
    {
        [Tooltip("The type of action represents")]
        [SerializeField] private ObjectiveActionType _actionType;

        [Tooltip("The target object ids")]
        public List<string> _targetObjectIds;

        [Tooltip("The start object id (SpawnPoint, FocusPoint initial position)")]
        [SerializeField] private string _startObjectId;

        [Tooltip("The amount of time to wait in seconds before moving to the next")]
        [Min(0)] public float _waitTimeInSeconds = 0;

        public ObjectiveActionType ActionType => _actionType;

        public List<string> TargetObjectIds => _targetObjectIds;

        public string StartObjectId => _startObjectId;

        public float WaitTimeInSeconds => _waitTimeInSeconds;
    }
    
    public enum ObjectiveActionType : byte
    {
        Wait = 0,
        Spawn = 1,
        Destroy = 2,
        CameraFocus = 3,
    }
}