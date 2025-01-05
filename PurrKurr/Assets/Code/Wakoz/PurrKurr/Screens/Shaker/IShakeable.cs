using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Shaker
{
    public interface IShakeable
    {
        void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime);
    }
    [System.Serializable]
    public class ShakeData
    {
        public ShakeStyle ShakeStyle = ShakeStyle.Random;
        public float Duration = 0.2f;
        public float Intensity = 0.5f;
        public AnimationCurve IntensityCurve = AnimationCurve.Linear(0, 1, 1, 0);

        public ShakeData(ShakeStyle shakeStyle) 
        { 
            ShakeStyle = shakeStyle; 
        }

        public ShakeData(ShakeStyle shakeStyle, float duration, float intensity = 0.5f, AnimationCurve intensityCurve = null)
        {
            ShakeStyle = shakeStyle;
            Duration = duration;
            Intensity = intensity;
            IntensityCurve = intensityCurve ?? AnimationCurve.Linear(0, 1, 1, 0);
        }
    }
    public enum ShakeStyle
    {
        Random = 0,
        Horizontal = 1,
        Vertical = 2,
        Circular = 3,
        HorizontalCircular = 4,
        VerticalCircular = 5,
    }
    public class ShakeController : MonoBehaviour
    {
        public ShakeHandler ShakeHandler { get; private set; }

        private void Awake()
        {
            ShakeHandler = new ShakeHandler();
        }

        public void TriggerShake(Transform target, ShakeData shakeData)
        {
            ShakeHandler.TriggerShake(target, shakeData);
        }

        // Call this in Update to process active shakes
        private void Update()
        {
            ShakeHandler.ProcessShakes(Time.deltaTime); 
        }
    }
    public class ShakeHandler
    {
        private readonly Dictionary<ShakeStyle, IShakeable> _shakeableImplementations;
        private readonly Dictionary<Transform, (ShakeData shakeData, float elapsedTime)> _activeShakes = new();
        private readonly Dictionary<Transform, (ShakeData shakeData, float elapsedTime)> _updatedShakes = new();

        private IShakeable shakeable;
        private List<Transform> _keysToRemove = new();

        public ShakeHandler()
        {
            _shakeableImplementations = new Dictionary<ShakeStyle, IShakeable>
            {
                { ShakeStyle.Random, new RandomShake() },
                { ShakeStyle.Horizontal, new HorizontalShake() },
                { ShakeStyle.Vertical, new VerticalShake() },
                { ShakeStyle.Circular, new CircularShake() },
                { ShakeStyle.HorizontalCircular, new HorizontalCircularShake() },
                { ShakeStyle.VerticalCircular, new VerticalCircularShake() }
            };
        }

        public void TriggerShake(Transform target, ShakeData shakeData)
        {
            if (target == null)
            {
                return;
            }

            Debug.Log("Trigger shake to target " + target.name);
            // Replace the previous shake
            _activeShakes[target] = (shakeData, 0f);
        }

        public void ProcessShakes(float deltaTime)
        {
            if (_activeShakes.Count == 0)
            {
                return;
            }

            _keysToRemove.Clear();
            _updatedShakes.Clear();

            foreach (var kvp in _activeShakes)
            {
                var target = kvp.Key;
                var (shakeData, elapsedTime) = kvp.Value;

                elapsedTime += deltaTime;

                if (elapsedTime >= shakeData.Duration)
                {
                    _keysToRemove.Add(target);
                    continue;
                }

                PerformShake(ref target, shakeData, elapsedTime);
                _updatedShakes[target] = (shakeData, elapsedTime); // Store updated shake data
            }

            // Update the active shakes with the new elapsed times
            foreach (var kvp in _updatedShakes)
            {
                _activeShakes[kvp.Key] = kvp.Value;
            }

            // Remove completed shakes
            foreach (var key in _keysToRemove)
            {
                _activeShakes.Remove(key);
            }
        }

        private void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            if (_shakeableImplementations.TryGetValue(shakeData.ShakeStyle, out shakeable))
            {
                shakeable.PerformShake(ref targetTransform, shakeData, elapsedTime);
            }
        }

    }
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
            targetTransform.localPosition = shakeOffset;
        }
    }
    public class HorizontalShake : BaseShake
    {
        public override void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            base.PerformShake(ref targetTransform, shakeData, elapsedTime);

            shakeOffset = new Vector3(Random.Range(-1f, 1f), 0, 0) * intensity;

            targetTransform.localPosition = shakeOffset;
        }
    }
    public class VerticalShake : BaseShake
    {
        public override void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            base.PerformShake(ref targetTransform, shakeData, elapsedTime);

            shakeOffset = new Vector3(0, Random.Range(-1f, 1f), 0) * intensity;

            targetTransform.localPosition = shakeOffset;
        }
    }
    public class CircularShake : BaseShake
    {
        protected float angle;

        public override void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            base.PerformShake(ref targetTransform, shakeData, elapsedTime);

            angle = elapsedTime * 360f / shakeData.Duration;
            shakeOffset = new Vector3(Mathf.Cos(angle) * intensity, Mathf.Sin(angle) * intensity, 0);

            targetTransform.localPosition = shakeOffset;
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

            targetTransform.localPosition = shakeOffset;
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

            targetTransform.localPosition = shakeOffset;
        }
    }
    
}