using Code.Wakoz.PurrKurr.Screens.Gameplay_Controller.Handlers;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Wakoz.PurrKurr.Screens.Shaker
{
    public sealed class ShakeHandler : IUpdateProcessHandler
    {
        private readonly Dictionary<ShakeStyle, IShakeable> _shakeableImplementations;
        private readonly Dictionary<Transform, (ShakeData shakeData, float elapsedTime)> _activeShakes = new();
        private readonly Dictionary<Transform, (ShakeData shakeData, float elapsedTime)> _updatedShakes = new();

        private IShakeable _shakeable;
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

            Debug.Log("Shake Triggered on target " + target.name);
            _activeShakes[target] = (shakeData, 0f);
        }

        public void UpdateProcess(float deltaTime)
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

            UpdateActiveShakes();

            RemoveCompletedShakes();
        }

        private void PerformShake(ref Transform targetTransform, ShakeData shakeData, float elapsedTime)
        {
            if (_shakeableImplementations.TryGetValue(shakeData.ShakeStyle, out _shakeable))
            {
                _shakeable.PerformShake(ref targetTransform, shakeData, elapsedTime);
            }
        }

        private void UpdateActiveShakes()
        {
            foreach (var kvp in _updatedShakes)
            {
                _activeShakes[kvp.Key] = kvp.Value;
            }
        }

        private void RemoveCompletedShakes()
        {
            foreach (var key in _keysToRemove)
            {
                _activeShakes.Remove(key);
            }
        }

    }
}