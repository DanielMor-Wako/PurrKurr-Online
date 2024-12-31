using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    [System.Serializable]
    public class CameraOffsetData
    {
        public Vector2 TargetOffset = Vector3.zero;

        [Min(0.01f)] public float SmoothSpeed = 1f;
    }
}
