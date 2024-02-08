using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Code.Wakoz.PurrKurr.DataClasses.GameCore.Anchors {

    [DefaultExecutionOrder(12)]
    public class EffectsController : SingleController {

        // Wait 1 second after emission ends and before turning off the gameobject to allow particles to die
        // todo: add and get this value from the EffectData
        [SerializeField] private float DelayBeforeGameObjectIsDeactivated = 1f;
        [SerializeField] private int MaxPoolCount = 5;
        private Dictionary<Transform, List<EffectData>> activeEffects = new Dictionary<Transform, List<EffectData>>();
        private Dictionary<ParticleSystem, ObjectPool<ParticleSystem>> particleSystemPools = new Dictionary<ParticleSystem, ObjectPool<ParticleSystem>>();

        private static List<GameObject> _effectsLayers;

        protected override Task Initialize() {

            _effectsLayers = new List<GameObject>();

            return Task.CompletedTask;
        }

        protected override void Clean() {
            CleanUp();
        }

        public void PlayEffect(EffectData effectData, Transform assignedObject) {
            
            if (!activeEffects.ContainsKey(assignedObject)) {
                activeEffects.Add(assignedObject, new List<EffectData> { effectData });

            } else {
                activeEffects[assignedObject].Add(effectData);
            }

            StartCoroutine(PlayEffectAndReturn(effectData, assignedObject));
        }

        private void SetEmission(ParticleSystem particleSystem, bool isEnabled) {
            
            var emission = particleSystem.emission;
            emission.enabled = isEnabled;
        }

        private IEnumerator PlayEffectAndReturn(EffectData effect, Transform assignedObject) {

            float duration = effect.DurationInSeconds;
            var particleSystem = GetEffectInstance(effect.Effect);
            if (particleSystem == null) {
                DeactivateEffect(assignedObject, effect.Effect);
                yield break;
            }

            if (assignedObject != null) {
                particleSystem.transform.position = assignedObject.transform.position;
            }

            particleSystem.gameObject.SetActive(true);
            SetEmission(particleSystem, true);
            var endTime = Time.time + duration;

            while (Time.time < endTime && assignedObject != null) {
                if (effect.TrackPosition) {
                    particleSystem.transform.position = assignedObject.transform.position;
                }

                yield return new WaitForFixedUpdate();
            }

            SetEmission(particleSystem, false);

            yield return new WaitForSeconds(effect.DurationInSeconds + DelayBeforeGameObjectIsDeactivated);

            DeactivateGameObjectWhenEmissionIsOff(particleSystem, effect.Effect);

            DeactivateEffect(assignedObject, effect.Effect);
        }

        private void DeactivateEffect(Transform assignedObject, ParticleSystem particleSystem) {

            if (assignedObject == null || particleSystem == null) {
                return;
            }

            if (activeEffects.ContainsKey(assignedObject)) {
                activeEffects[assignedObject].RemoveAll(effectData => effectData.Effect == particleSystem);
                if (activeEffects[assignedObject].Count == 0) {
                    activeEffects.Remove(assignedObject);
                }
            }

        }

        private ParticleSystem GetEffectInstance(ParticleSystem effect) {

            if (!particleSystemPools.ContainsKey(effect)) {

                var container = new GameObject($"{effect.name}");
                container.transform.SetParent(transform, false);
                _effectsLayers.Add(container);
                particleSystemPools[effect] = new ObjectPool<ParticleSystem>(() => Instantiate(effect, container.transform), null, null, null, true, 0, MaxPoolCount);
            }

            return particleSystemPools[effect].Get();
        }

        private void DeactivateGameObjectWhenEmissionIsOff(ParticleSystem instance, ParticleSystem particleSystem) {

            if (instance.emission.enabled || !instance.gameObject.activeSelf) {
                return;
            }

            particleSystemPools[particleSystem].Release(instance);
            instance.gameObject.SetActive(false);
        }

        public void CleanUp() {

            /*foreach (var kvp in activeEffects) {
                foreach (var effectData in kvp.Value) {
                    if (effectData != null && effectData.Effect != null) {
                        effectData.Effect.gameObject.SetActive(false);
                        particleSystemPools[effectData.Effect].Release(effectData.Effect); // Release the ParticleSystem instance back to the pool
                    }
                }
            }*/

            foreach (var effectsLayer in _effectsLayers) {
                if (effectsLayer != null) {
                    Destroy(effectsLayer);
                }
            }

            activeEffects.Clear();
        }
    }

}