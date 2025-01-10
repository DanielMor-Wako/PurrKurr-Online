using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Shaker
{
    [System.Serializable]
    public class ShakeData
    {
        public ShakeStyle ShakeStyle = ShakeStyle.Random;
        public float Duration = 0.2f;
        public float Intensity = 0.5f;
        public AnimationCurve IntensityCurve = AnimationCurve.Linear(0, 1, 1, 0);
        public Vector3 DefaultOffset = Vector3.zero;

        public ShakeData(ShakeStyle shakeStyle) 
        { 
            ShakeStyle = shakeStyle; 
        }

        public ShakeData(ShakeStyle shakeStyle, float duration, float intensity = 0.5f, AnimationCurve intensityCurve = null, Vector3 defaultOffset = default)
        {
            ShakeStyle = shakeStyle;
            Duration = duration;
            Intensity = intensity;
            IntensityCurve = intensityCurve ?? AnimationCurve.Linear(0, 1, 1, 0);
            DefaultOffset = defaultOffset;
        }
    }
    
}