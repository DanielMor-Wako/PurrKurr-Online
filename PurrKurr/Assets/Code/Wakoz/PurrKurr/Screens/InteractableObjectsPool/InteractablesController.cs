using Code.Wakoz.PurrKurr.DataClasses.Characters;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Projectiles;
using Code.Wakoz.PurrKurr.DataClasses.GameCore.Ropes;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Code.Wakoz.PurrKurr.Screens.InteractableObjectsPool {

    [DefaultExecutionOrder(12)]
    public sealed class InteractablesController : SingleController {

        // todo; make the InteractablesController responsible for these
        public List<Character2DController> _heroes;
        public List<ProjectileController> _projectiles;
        public List<RopeController> _ropes;
        // todo: make all these to ObjectPoolsController
        private Dictionary<System.Type, object> objectPools = new Dictionary<System.Type, object>();

        protected override void Clean() { }

        protected override Task Initialize() {

            return Task.CompletedTask;
        }

        public void CreateObjectPool<T>(T prefab, int initialSize, int maxCapacity, string containerName) where T : MonoBehaviour/*, IInteractableBody*/ {

            if (!objectPools.ContainsKey(typeof(T))) {
                var container = new GameObject($"{containerName}");
                container.transform.SetParent(transform, false);
                GenericObjectPool<T> pool = new GenericObjectPool<T>(prefab, container.transform, initialSize, maxCapacity);
                objectPools.Add(typeof(T), pool);
            }
        }

        public T GetInstance<T>() where T : MonoBehaviour/*, IInteractableBody*/ {

            if (objectPools.ContainsKey(typeof(T))) {
                GenericObjectPool<T> pool = (GenericObjectPool<T>)objectPools[typeof(T)];
                return pool.GetObjectFromPool();
            } else {
                Debug.LogError("Object pool for type " + typeof(T) + " does not exist.");
                return null;
            }
        }

        public List<T> GetAllInstances<T>() where T : MonoBehaviour/*, IInteractableBody*/ {
            if (objectPools.ContainsKey(typeof(T))) {
                GenericObjectPool<T> pool = (GenericObjectPool<T>)objectPools[typeof(T)];
                return pool.GetAllObjectsFromPool();
            } else {
                Debug.LogError("Object pool for type " + typeof(T) + " does not exist.");
                return new List<T>();
            }
        }

        public Task ReleaseInstance<T>(T instance) where T : MonoBehaviour/*, IInteractableBody*/ {

            if (objectPools.ContainsKey(typeof(T))) {
                GenericObjectPool<T> pool = (GenericObjectPool<T>)objectPools[typeof(T)];
                pool.ReleaseObjectToPool(instance);
            } else {
                Debug.LogError("Object pool for type " + typeof(T) + " does not the instance for release");
            }

            return Task.CompletedTask;
        }

    }

    public class GenericObjectPool<T> where T : MonoBehaviour {

        private ObjectPool<T> objectPool;
        private readonly List<T> _instances = new();
        private Transform _container;
        private int _initialSize;
        private int _maxCapacity;

        public GenericObjectPool(T prefab, Transform container, int initialSize, int maxCapacity) {
            
            _container = container;
            _initialSize = initialSize;
            _maxCapacity = maxCapacity;

            objectPool = new ObjectPool<T>(() => UnityEngine.Object.Instantiate(prefab, container));
        }

        public T GetObjectFromPool() {

            if (objectPool == null) {
                Debug.LogError($"ObjectPool {typeof(T).Name} was not initialized");
                return null;
            }

            if (objectPool.CountAll < _maxCapacity || objectPool.CountInactive > 0) {
                T newObj = objectPool.Get();
                newObj.transform.SetParent(_container, false);
                newObj.gameObject.SetActive(true);
                _instances.Add(newObj);
                return newObj;
            }

            return null;
        }

        public List<T> GetAllObjectsFromPool() => _instances;

        public void ReleaseObjectToPool(T instance) {
            if (objectPool != null && instance != null && _instances.Contains(instance)) {
                instance.gameObject.SetActive(false);
                objectPool.Release(instance);
            } else {
                Debug.LogWarning("Unable to release the object to the pool. Please ensure the object and the object pool are valid.");
            }
        }

        // _updatingState = StartCoroutine(ReleaseObjectsSequentially(_unchainedLinks, 3000, 200, (instance) => SafeRemoveInstance(instance), () => { _updatingState = null; return SafeRemoveAllUnchainedLinks(); })); // 100 milliseconds interval
        public IEnumerator ReleaseObjectsSequentially(List<T> instances, float preDelay, float releaseInterval,
        Func<T, IEnumerator> actionBeforeRelease = null, Func<IEnumerator> actionOnEndProcess = null) {

            if (preDelay > 0) {
                yield return new WaitForSeconds(preDelay * 0.001f);
            }

            foreach (var instance in instances) {

                if (instance == null) {
                    continue;
                }

                if (actionBeforeRelease != null) {
                    yield return actionBeforeRelease(instance);
                }

                ReleaseObjectToPool(instance);

                yield return new WaitForSeconds(releaseInterval * 0.001f);
            }

            if (actionOnEndProcess != null) {
                yield return actionOnEndProcess();
            }

        }
    }
}