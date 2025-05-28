using Code.Core.Services.DataManagement.Serializers;
using Code.Core.Services.DataManagement.Storage;
using Code.Core.Services.DataManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Core.DataClassFactory;
using Code.Wakoz.PurrKurr.DataClasses.ServerData;
using System;

namespace Code.Core
{
    public class GameStateManager {

        private ISerializer _serializer;
        private IStorage _storage;

        private SaveManager _saveManager;
        private DataClassesFactory _dataFactory;
        private Dictionary<string, object> _serializedInstances = new();

        public GameStateManager(IStorage storage, ISerializer serialization) {

            _storage = storage;

            _serializer = serialization;

            _saveManager = new SaveManager(_storage, _serializer);

            _dataFactory = new DataClassesFactory(_serializer);
            // todo: collect all these under typeof(PlayerData)
            _dataFactory.Register("level", typeof(int));
            _dataFactory.Register("exp", typeof(int));
            _dataFactory.Register("mainCharacter", typeof(MainCharacter_SerializeableData));
            _dataFactory.Register("characters", typeof(CharactersIDs_SerializeableData));
            _dataFactory.Register("ongoingObjectives", typeof(OngoingObjectives_SerializeableData));
            _dataFactory.Register("completedObjectives", typeof(CompletedObjectives_SerializeableData));
            _dataFactory.Register("gameState", typeof(GameState_SerializeableData));
        }

        public Dictionary<string, Type> GetRegisteredInstances() {
            return _dataFactory.GetRegisteredInstances();
        }

        public Dictionary<string, object> GetSerializedInstances() 
            => _serializedInstances;
/*
        public async Task SaveGameState(string key, object value) {
*//*
            var newGameState = new GameStateData();

            foreach (var (kvp, vvp) in _serializedInstances) {

                var serializeMethod = _serializer.GetType().GetMethod("Serialize").MakeGenericMethod(vvp.GetType());

                string serializedData = (string)serializeMethod.Invoke(_serializer, new object[] { vvp });

                newGameState.objects.Add(new ObjectRecord() { key = kvp, value = serializedData });
            }
*//*
            await _saveManager.SaveAsync(key, value);
        }*/

        public async Task SavePlayerPublicData(params (string key, object value)[] values) {

            await _saveManager.SavePublicPlayerDataAsync(values);
            CacheDataInstances(values);
        }

        public async Task SavePlayerPrivateData(params (string key, object value)[] values) {

            await _saveManager.SavePrivatePlayerDataAsync(values);
            CacheDataInstances(values);
        }

        /// <summary>
        /// Returns the level of the user account, Level 0 indicates new user,
        /// Level 1 indicates user has initialized basic data
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetUserLevel() {

            var data = await _saveManager.LoadAsyncRaw<int>("level");
            return data;
        }

        /// <summary>
        /// Returns the id of main character, if none exist / or is new account
        /// When no id is found, value return as -1
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetMainCharacterId() {

            var data = await _saveManager.LoadAsyncRaw<MainCharacter_SerializeableData>("mainCharacter");
            var mainCharacterId = data != null && data.id > 0 ? data.id : -1;
            return mainCharacterId;
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            return await _saveManager.GetAllKeys();
        }
/*
        public async Task LoadGameState(string objectKey) {

            var objectType = _dataFactory.GetRegisteredType(objectKey);

            await LoadGameState<objectType>(objectKey);
        }
*/
        public async Task LoadGameState<T>() {

            var objectKey = _dataFactory.GetKeyOfType<T>();

            await LoadGameState<T>(objectKey);
        }

        public async Task LoadGameState<T>(params string[] keys) {

            var keysData = await _saveManager.LoadAsyncRaw<T>(keys);
            /*foreach (var objectKey in keysData) {
                var data = await _saveManager.LoadAsyncRaw<T>(objectKey);
                CacheDataInstances(key)
            }*/
        }
        
        public async Task LoadGameState<T>(string objectKey) {
            UnityEngine.Debug.Log($"{objectKey} as {typeof(T).Name}");
            var data = await _saveManager.LoadAsyncRaw<T>(objectKey);

            CacheDataInstance(objectKey, data);
        }

        private void CacheDataInstances(params (string key, object value)[] values) {
            foreach (var i in values) {
                CacheDataInstance(i.key, i.value);
            }
        }

        private void CacheDataInstance<T>(string objectKey, T data) {

            //var type = _dataFactory.GetRegisteredType(objectKey);

            _serializedInstances[objectKey] = data;

/*
            foreach (var obj in objectValue) {

                Debug.Log($"value for {obj.key} is {obj.value}");

                var instance = _dataFactory.Create(obj.key, obj.value);

                _serializedInstances.Add(obj.key, instance);
            }
*/
        }

        public async Task DeleteAllAsync() {
            await _saveManager.DeleteAllAsync();
        }
    }
}
