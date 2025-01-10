using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Shaker
{
    public interface IShakeable
    {
        void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime);
    }
}