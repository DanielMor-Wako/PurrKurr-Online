using UnityEngine;

namespace Code.Wakoz.PurrKurr.DataClasses.ScriptableObjectData
{

    [DefaultExecutionOrder(1)]
    public abstract class SOData 
    {
        private string _assetName = "";
        private const string ASSET_PATH = "DataManagement/GameConfig/";
        private string assetFullPath => ASSET_PATH + _assetName;
        
        private ScriptableObject _sOData;

        public string GetAssetName() => _assetName;
        
        protected SOData(string assetName) => CreateInstance(assetName);

        private void CreateInstance(string assetName) 
        {
            _assetName = assetName;
            GetAsset();
            if (_sOData != null) {
                Init();
            }
        }

        protected abstract void Init();

        protected ScriptableObject GetAsset() 
        {
            if (string.IsNullOrEmpty(_assetName)) {
                Debug.LogError("_assetName is not set");
                return null;
            }

            return _sOData ??= LoadSoAsset(_assetName);
        }

        private ScriptableObject LoadSoAsset(string assetName) {

            if (assetName == "") {
                Debug.LogWarning("Asset Name is not set");
                return null;
            }
            
            _assetName = assetName;
            var assetPath = assetFullPath;
            
            if (assetPath == "") {
                Debug.LogError("Asset Path is not set");
                return null;
            }
            
            _sOData ??= Resources.Load<ScriptableObject>(assetPath);
            
            if (_sOData == null) {
                Debug.LogError($"Asset is missing: {assetPath}");
            } else {
                Debug.Log($"Asset loaded: {assetPath}");
            }

            return _sOData;
        }

    }

    [DefaultExecutionOrder(2)]
    public abstract class SOData<T> : SOData where T : ScriptableObject 
    {
        protected SOData(string assetName) : base(assetName) {}
        protected T Data 
        {
            get {
                if (_cachedData == null) {
                    _cachedData = GetAsset() as T;
                }
                return _cachedData;
            }
        }
        private T _cachedData;
    }

}