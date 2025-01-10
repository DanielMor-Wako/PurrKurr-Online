using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Shaker
{
    public abstract class BaseShake : IShakeable
    {
        protected float normalizedTime;
        protected float intensity;
        protected Vector3 shakeOffset;

        public virtual void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            normalizedTime = elapsedTime / shakeData.Duration;
            intensity = shakeData.IntensityCurve.Evaluate(normalizedTime) * shakeData.Intensity;
        }
    }

    public class RandomShake : BaseShake
    {
        public override void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            base.PerformShake(ref targetTransform, shakeData, elapsedTime);

            shakeOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * intensity;

            targetTransform.localPosition = shakeOffset + shakeData.DefaultOffset;
        }
    }

    public class HorizontalShake : BaseShake
    {
        public override void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            base.PerformShake(ref targetTransform, shakeData, elapsedTime);

            shakeOffset = new Vector3(Random.Range(-1f, 1f) * intensity, 0, 0);

            targetTransform.localPosition = shakeOffset + shakeData.DefaultOffset;
        }
    }

    public class VerticalShake : BaseShake
    {
        public override void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            base.PerformShake(ref targetTransform, shakeData, elapsedTime);

            shakeOffset = new Vector3(0, Random.Range(-1f, 1f) * intensity, 0);

            targetTransform.localPosition = shakeOffset + shakeData.DefaultOffset;
        }
    }

    public class CircularShake : BaseShake
    {
        protected float angle;

        public override void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            base.PerformShake(ref targetTransform, shakeData, elapsedTime);

            angle = elapsedTime * 360f / shakeData.Duration;
            shakeOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * intensity;

            targetTransform.localPosition = shakeOffset + shakeData.DefaultOffset;
        }
    }

    public class HorizontalCircularShake : BaseShake
    {
        protected float angle;

        public override void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            base.PerformShake(ref targetTransform, shakeData, elapsedTime);

            angle = elapsedTime * 360f / shakeData.Duration;
            shakeOffset = new Vector3(Mathf.Cos(angle) * intensity, 0, 0);

            targetTransform.localPosition = shakeOffset + shakeData.DefaultOffset;
        }
    }

    public class VerticalCircularShake : BaseShake
    {
        protected float angle;

        public override void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            base.PerformShake(ref targetTransform, shakeData, elapsedTime);

            angle = elapsedTime * 360f / shakeData.Duration;
            shakeOffset = new Vector3(0, Mathf.Sin(angle) * intensity, 0);

            targetTransform.localPosition = shakeOffset + shakeData.DefaultOffset;
        }
    }
}