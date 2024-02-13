using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using static Code.Wakoz.PurrKurr.DataClasses.Enums.Definitions;

namespace Code.Wakoz.PurrKurr.DataClasses.Effects {

    [DefaultExecutionOrder(12)]
    public sealed class EffectsController : SingleController {

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

        public void PlayEffect(EffectData effectData, Transform assignedObject, Quaternion initialRotation, List<Effect2DType> stopWhenAnyEffectStarts) {
            
            if (!activeEffects.ContainsKey(assignedObject)) {
                activeEffects.Add(assignedObject, new List<EffectData> { effectData });

            } else {
                activeEffects[assignedObject].Add(effectData);
            }

            StartCoroutine(PlayEffectAndReturn(effectData, assignedObject, initialRotation, stopWhenAnyEffectStarts)); 
        }

        private void SetEmission(ParticleSystem particleSystem, bool isEnabled) {
            
            var emission = particleSystem.emission;
            emission.enabled = isEnabled;

            if (isEnabled) {
                particleSystem.Play();
            } else {
                particleSystem.Stop();
            }

        }

        private IEnumerator PlayEffectAndReturn(EffectData effect, Transform assignedObject, Quaternion _initialRotation, List<Effect2DType> effectsThatKillTheProcess) {

            float duration = effect.DurationInSeconds;
            var particleSystem = GetEffectInstance(effect.Effect);
            if (particleSystem == null) {
                DeactivateEffect(assignedObject, effect.Effect);
                yield break;
            }

            if (assignedObject != null) {
                if (_initialRotation == null) {
                    _initialRotation = Quaternion.identity;
                }
                particleSystem.transform.SetPositionAndRotation(assignedObject.transform.position, _initialRotation);
            }

            particleSystem.gameObject.SetActive(true);
            SetEmission(particleSystem, true);
            var endTime = Time.time + duration;

            var hasProcessKillers = effectsThatKillTheProcess != null;

            while (Time.time < endTime && assignedObject != null &&
                (!hasProcessKillers || hasProcessKillers && !IsEffectTypePlayedForAssignedObject(assignedObject, effectsThatKillTheProcess))) {
                
                if (effect.TrackPosition) {
                    particleSystem.transform.position = assignedObject.transform.position;
                }

                yield return new WaitForFixedUpdate();
            }

            SetEmission(particleSystem, false);

            var wasKilledEarly = hasProcessKillers && Time.time < endTime;

            yield return new WaitForSeconds(effect.DurationInSeconds);

            if (!wasKilledEarly) {
                yield return new WaitForSeconds(DelayBeforeGameObjectIsDeactivated);
            }

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
                particleSystemPools[effect] = new ObjectPool<ParticleSystem>(() => Instantiate(effect, container.transform));
            }

            if (particleSystemPools[effect].CountAll <= MaxPoolCount || particleSystemPools[effect].CountInactive > 0) {
                return particleSystemPools[effect].Get();
            }

            return null;
        }

        private void DeactivateGameObjectWhenEmissionIsOff(ParticleSystem instance, ParticleSystem particleSystem) {

            if (instance.emission.enabled || !instance.gameObject.activeSelf) {
                return;
            }

            particleSystemPools[particleSystem].Release(instance);
            instance.gameObject.SetActive(false);
        }

        private bool IsEffectTypePlayedForAssignedObject(Transform assignedObject, List<Effect2DType> effectsThatKillTheProcess) {

            ParticleSystem particleSystemInstance;

            if (effectsThatKillTheProcess == null) {
                return false;
            }

            foreach (var kvp in activeEffects) {

                if (kvp.Key != assignedObject) {
                    continue;
                }

                foreach (var effectData in kvp.Value) {

                    if (effectData == null || effectData.Effect == null) {
                        continue;
                    }

                    if (effectsThatKillTheProcess.Contains(effectData.EffectType)) {
                        Debug.Log($"effect {effectData.EffectType} was active and marked to kill process on assigned {assignedObject.name}");
                        // get the active instance for the assignedObject from a list and validate that gameobject.activeSelf is false
                        //effectData.Effect.gameObject.SetActive(false);
                        //particleSystemPools[effectData.Effect].Release(effectData.Effect); // Release the ParticleSystem instance back to the pool
                        return true;
                    }
                }
            }

            return false;
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