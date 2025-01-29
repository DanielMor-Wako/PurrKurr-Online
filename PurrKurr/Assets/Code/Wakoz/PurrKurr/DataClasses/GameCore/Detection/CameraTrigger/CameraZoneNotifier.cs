using Code.Wakoz.PurrKurr.Screens.CameraSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Detection
{
    public class CameraZoneNotifier : MonoBehaviour, ICameraZoneNotifier
    {
        [Header("Zone - Camera Data")]

        [Tooltip("Lifecycle when zone is active")]
        [SerializeField] private LifeCycleData _lifeCycle;

        [Tooltip("Transition settings while moving towards a new position")]
        [SerializeField] private CameraTransitionsData _transition;

        [Tooltip("Offset settings towards the gameobject transform position")]
        [SerializeField] private CameraOffsetData _offset;

        public CameraData GetCameraData()
        {
            return new CameraData(new List<Transform>() { transform },
                        _lifeCycle, _transition, _offset);
        }
    }
}