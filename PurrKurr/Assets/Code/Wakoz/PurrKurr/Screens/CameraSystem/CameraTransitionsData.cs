using UnityEngine;
using System;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    [System.Serializable]
    public class CameraTransitionsData
    {
        [Min(0.01f)] public float SmoothSpeed = 8f;
        
        [Min(1)] public float MinZoom = 25f;
        
        [Min(1)] public float MaxZoom = 6f;
        
        [Min(1)] public float ZoomLimiter = 65f;

        [Range(0.1f, 5f)] public float ZoomSpeed = 3f;
    }
}
