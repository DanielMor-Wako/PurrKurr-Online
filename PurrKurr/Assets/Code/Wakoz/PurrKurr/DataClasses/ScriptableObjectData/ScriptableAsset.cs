using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData
{
    /// <summary>
    /// Abstract base class for managing ScriptableObject assets in a Unity project
    /// Provides lazy loading and initialization for ScriptableObject data
    /// </summary>
    [DefaultExecutionOrder(1)]
    public abstract class ScriptableAsset
    {
        public string AssetName => _assetName;

        private readonly string _assetName;
        private readonly string _assetPath;
        private bool _isInitialized;
        private ScriptableObject _scriptableObject;

        protected ScriptableAsset(string assetName, string pathPrefix) {

            if (string.IsNullOrEmpty(assetName)) {
                Debug.LogError("Asset name cannot be null or empty.");
                return;
            }

            _assetName = assetName;
            _assetPath = $"{pathPrefix}{assetName}";
            EnsureLoaded();
        }

        /// <summary>
        /// Ensures the ScriptableAsset is loaded and initialized
        /// </summary>
        private void EnsureLoaded() {

            _scriptableObject = LoadAsset();
            if (_scriptableObject != null) {
                Init();
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Implement this method to define custom initialization logic for the ScriptableObject
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// Retrieves the loaded ScriptableObject, loading it if necessary
        /// </summary>
        protected ScriptableObject GetAsset() {

            if (_scriptableObject == null && !_isInitialized) {
                EnsureLoaded();
            }
            return _scriptableObject;
        }

        /// <summary>
        /// Loads the ScriptableObject from the specified asset path
        /// </summary>
        private ScriptableObject LoadAsset() {

            if (string.IsNullOrEmpty(_assetPath)) {
                Debug.LogError($"Invalid asset path for asset: {_assetName}");
                return null;
            }

            var asset = Resources.Load<ScriptableObject>(_assetPath);
            if (asset == null) {
                Debug.LogError($"Failed to load ScriptableObject at path: {_assetPath}");
            }
#if UNITY_EDITOR
            else {
                Debug.Log($"Loaded ScriptableObject: {_assetPath}");
            }
#endif
            return asset;
        }

        /// <summary>
        /// Unloads the ScriptableObject to free resources
        /// </summary>
        public virtual void Unload() {

            if (_scriptableObject != null) {
                Resources.UnloadAsset(_scriptableObject);
                _scriptableObject = null;
                _isInitialized = false;
            }
        }
        /// <summary>
        /// Forces a reload of the ScriptableAsset, reinitializing it.
        /// </summary>
        public void Reload() {
            Unload();
            EnsureLoaded();
        }
    }

    /// <summary>
    /// Generic manager for typed ScriptableObject assets
    /// </summary>
    /// <typeparam name="T">The type of ScriptableObject to manage.</typeparam>
    [DefaultExecutionOrder(2)]
    public abstract class ScriptableAsset<T> : ScriptableAsset where T : ScriptableObject
    {
        private T _cachedData;

        protected ScriptableAsset(string assetName, string pathPrefix) : base(assetName, pathPrefix) { }

        /// <summary>
        /// Gets the typed ScriptableObject data, loading it if necessary
        /// </summary>
        protected T Data {
            get {
                if (_cachedData == null) {
                    _cachedData = GetAsset() as T;
                    if (_cachedData == null) {
                        Debug.LogError($"Failed to cast ScriptableObject to type {typeof(T).Name} for asset: {AssetName}");
                    }
                }
                return _cachedData;
            }
        }

        /// <summary>
        /// Unloads the ScriptableObject and clears the cached data
        /// </summary>
        public override void Unload() {
            base.Unload();

            _cachedData = null;
        }
    }

}