using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.CameraSystem
{
    [System.Serializable]
    public class LifeCycleData
    {
        [Min(0f)] public float MinimumDuration;

        public LifeCycleData(float minimum = 0)
        {
            MinimumDuration = minimum;
        }
    }
}
